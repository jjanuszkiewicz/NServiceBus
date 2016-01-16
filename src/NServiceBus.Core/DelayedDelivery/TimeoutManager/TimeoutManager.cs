namespace NServiceBus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.ConsistencyGuarantees;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.ObjectBuilder;
    using NServiceBus.Settings;
    using NServiceBus.Timeout.Core;
    using NServiceBus.Transports;

    /// <summary>
    /// Used to configure the timeout manager that provides message deferral.
    /// </summary>
    public class TimeoutManager : Feature
    {
        internal TimeoutManager()
        {
            Defaults(s => s.SetDefault("TimeToWaitBeforeTriggeringCriticalErrorForTimeoutPersisterReceiver", TimeSpan.FromSeconds(2)));
            EnableByDefault();

            Prerequisite(context => !context.Settings.GetOrDefault<bool>("Endpoint.SendOnly"), "Send only endpoints can't use the timeoutmanager since it requires receive capabilities");
            Prerequisite(context => !HasAlternateTimeoutManagerBeenConfigured(context.Settings), "A user configured timeoutmanager address has been found and this endpoint will send timeouts to that endpoint");
            Prerequisite(c => !c.DoesTransportSupportConstraint<DelayedDeliveryConstraint>(), "The selected transport supports delayed delivery natively");
        }

        /// <summary>
        /// See <see cref="Feature.Setup" />.
        /// </summary>
        protected internal override void Setup(FeatureConfigurationContext context)
        {
            var storageCleaner = new RetryStorageCleaner();

            SetupStorageSatellite(context, storageCleaner);

            var dispatcherAddress = SetupDispatcherSatellite(context, storageCleaner);

            SetupTimeoutPoller(context, dispatcherAddress);
        }

        static void SetupTimeoutPoller(FeatureConfigurationContext context, string dispatcherAddress)
        {
            context.Container.ConfigureComponent(b =>
            {
                var waitTime = context.Settings.Get<TimeSpan>("TimeToWaitBeforeTriggeringCriticalErrorForTimeoutPersisterReceiver");

                var criticalError = b.Build<CriticalError>();

                var circuitBreaker = new RepeatedFailuresOverTimeCircuitBreaker("TimeoutStorageConnectivity",
                    waitTime,
                    ex => criticalError.Raise("Repeated failures when fetching timeouts from storage, endpoint will be terminated.", ex));

                return new ExpiredTimeoutsPoller(b.Build<IQueryTimeouts>(), b.Build<IDispatchMessages>(), dispatcherAddress, circuitBreaker);
            }, DependencyLifecycle.SingleInstance);

            context.RegisterStartupTask(b => new TimeoutPollerRunner(b.Build<ExpiredTimeoutsPoller>()));
        }

        static string SetupDispatcherSatellite(FeatureConfigurationContext context, RetryStorageCleaner storageCleaner)
        {
            var errorQueueAddress = ErrorQueueSettings.GetConfiguredErrorQueue(context.Settings);
            var requiredTransactionSupport = context.Settings.GetRequiredTransactionModeForReceives();

            string dispatcherAddress;
            var dispatcherProcessorPipeline = context.AddSatellitePipeline("Timeout Dispatcher Processor", "TimeoutsDispatcher", requiredTransactionSupport, PushRuntimeSettings.Default, out dispatcherAddress);


            dispatcherProcessorPipeline.Register("DispatchTimeoutRecoverability",
                b => CreateTimeoutRecoverabilityBehavior(errorQueueAddress, dispatcherAddress, b, storageCleaner),
                "Handles failures when dispatching timeouts");
      
            dispatcherProcessorPipeline.Register("TimeoutDispatcherProcessor", b => new DispatchTimeoutBehavior(
                b.Build<IDispatchMessages>(),
                b.Build<IPersistTimeouts>(),
                requiredTransactionSupport),
                "Terminates the satellite responsible for dispatching expired timeouts to their final destination");
            return dispatcherAddress;
        }

        static void SetupStorageSatellite(FeatureConfigurationContext context, RetryStorageCleaner storageCleaner)
        {
            var errorQueueAddress = ErrorQueueSettings.GetConfiguredErrorQueue(context.Settings);
            var requiredTransactionSupport = context.Settings.GetRequiredTransactionModeForReceives();

            string processorAddress;
            var messageProcessorPipeline = context.AddSatellitePipeline("Timeout Message Processor", "Timeouts", requiredTransactionSupport, PushRuntimeSettings.Default, out processorAddress);


            messageProcessorPipeline.Register("StoreTimeoutRecoverability",
                b => CreateTimeoutRecoverabilityBehavior(errorQueueAddress, processorAddress, b, storageCleaner),
                "Handles failures when storing timeouts");

            messageProcessorPipeline.Register("StoreTimeoutTerminator", b => new StoreTimeoutBehavior(b.Build<ExpiredTimeoutsPoller>(),
                b.Build<IDispatchMessages>(),
                b.Build<IPersistTimeouts>(),
                context.Settings.EndpointName().ToString()),
                "Terminates the satellite responsible for storing timeouts into timeout storage");

            context.Settings.Get<TimeoutManagerAddressConfiguration>().Set(processorAddress);
        }

        static TimeoutRecoverabilityBehavior CreateTimeoutRecoverabilityBehavior(string errorQueueAddress, string processorAddress, IBuilder b, RetryStorageCleaner storageCleaner)
        {
            var behavior = new TimeoutRecoverabilityBehavior(errorQueueAddress, processorAddress, b.Build<IDispatchMessages>(), b.Build<CriticalError>());

            storageCleaner.RegisterForCleanup(behavior);

            return behavior;
        }

        bool HasAlternateTimeoutManagerBeenConfigured(ReadOnlySettings settings)
        {
            return settings.Get<TimeoutManagerAddressConfiguration>().TransportAddress != null;
        }

        class RetryStorageCleaner : FeatureStartupTask
        {
            public RetryStorageCleaner()
            {
                behaviors = new List<TimeoutRecoverabilityBehavior>();
            }

            protected override Task OnStart(IBusSession session)
            {
                var clearingInterval = TimeSpan.FromMinutes(5);

                timer = new Timer(state => behaviors.ForEach(b => b.ClearFailures()), null, clearingInterval, clearingInterval);
                return TaskEx.Completed;
            }

            protected override Task OnStop(IBusSession session)
            {
                timer?.Dispose();
                return TaskEx.Completed;
            }

            public void RegisterForCleanup(TimeoutRecoverabilityBehavior timeoutRecoverabilityBehavior)
            {
                behaviors.Add(timeoutRecoverabilityBehavior);
            }

            List<TimeoutRecoverabilityBehavior> behaviors;
            Timer timer;
        }
    }
}