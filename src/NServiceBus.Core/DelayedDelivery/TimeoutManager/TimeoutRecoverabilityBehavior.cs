namespace NServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Logging;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Routing;
    using NServiceBus.Transports;

    class TimeoutRecoverabilityBehavior : Behavior<ITransportReceiveContext>
    {
        public TimeoutRecoverabilityBehavior(string errorQueueAddress, string localAddress, IDispatchMessages dispatcher, CriticalError criticalError)
        {
            this.localAddress = localAddress;
            this.errorQueueAddress = errorQueueAddress;
            this.dispatcher = dispatcher;
            this.criticalError = criticalError;
        }

        public override async Task Invoke(ITransportReceiveContext context, Func<Task> next)
        {
            try
            {
                await next().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                var message = context.Message;
                var numRetries = failuresPerMessage.AddOrUpdate(message.MessageId, 1, (s, i) => i + 1);

                if (numRetries <= MaxRetries)
                {
                    context.AbortReceiveOperation();
                    return;
                }

                try
                {
                    Logger.Error($"Moving timeout message '{message.MessageId}' to the error queue because processing failed due to an exception:", exception);

                    message.SetExceptionHeaders(exception, localAddress);

                    var outgoingMessage = new OutgoingMessage(message.MessageId, message.Headers, message.Body);
                    var routingStrategy = new UnicastRoutingStrategy(errorQueueAddress);
                    var addressTag = routingStrategy.Apply(new Dictionary<string, string>());

                    var transportOperations = new TransportOperations(new TransportOperation(outgoingMessage, addressTag, DispatchConsistency.Default));

                    await dispatcher.Dispatch(transportOperations, context.Extensions).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    criticalError.Raise("Failed to forward failed timeout message to error queue", ex);
                    throw;
                }
            }
        }

        public void ClearFailures()
        {
            failuresPerMessage.Clear();
        }

        CriticalError criticalError;
        IDispatchMessages dispatcher;
        string errorQueueAddress;

        ConcurrentDictionary<string, int> failuresPerMessage = new ConcurrentDictionary<string, int>();

        string localAddress;
        static int MaxRetries = 4;

        static ILog Logger = LogManager.GetLogger<TimeoutRecoverabilityBehavior>();
    }
}