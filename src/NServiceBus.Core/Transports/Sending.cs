namespace NServiceBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.Features;
    using NServiceBus.Transports;

    class Sending : Feature
    {
        public Sending()
        {
            EnableByDefault();
            DependsOn<Transport>();
        }

        protected internal override void Setup(FeatureConfigurationContext context)
        {
            var transport = context.Settings.Get<OutboundTransport>();
            var lazySendingConfigResult = new Lazy<TransportSendingConfigurationResult>(() => transport.Configure(context.Settings), LazyThreadSafetyMode.ExecutionAndPublication);
            context.Container.ConfigureComponent(c =>
            {
                var dispatcher = lazySendingConfigResult.Value.DispatcherFactory();
                return dispatcher;
            }, DependencyLifecycle.SingleInstance);

            context.RegisterStartupTask(new PrepareForSending(lazySendingConfigResult));
        }
        
        class PrepareForSending : FeatureStartupTask
        {
            private readonly Lazy<TransportSendingConfigurationResult> lazy;

            public PrepareForSending(Lazy<TransportSendingConfigurationResult> lazy)
            {
                this.lazy = lazy;
            }

            protected override async Task OnStart(IBusSession session)
            {
                var result = await lazy.Value.PreStartupCheck().ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    throw new Exception("Pre start-up check failed: "+ result.ErrorMessage);
                }
            }

            protected override Task OnStop(IBusSession session)
            {
                return TaskEx.Completed;
            }
        }
    }
}