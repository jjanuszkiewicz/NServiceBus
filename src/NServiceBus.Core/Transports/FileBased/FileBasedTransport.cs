namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    using NServiceBus.Transports.FileBased;

    /// <summary>
    /// A file based transport.
    /// </summary>
    public class FileBasedTransport : TransportDefinition
    {
        /// <summary>
        /// Gets an example connection string to use when reporting lack of configured connection string to the user.
        /// </summary>
        public override string ExampleConnectionStringForErrorMessage { get; } = "";

        /// <summary>
        /// Returns the discriminator for this endpoint instance.
        /// </summary>
        public override string GetDiscriminatorForThisEndpointInstance()
        {
            return "\\";
        }

        /// <summary>
        /// Gets the supported transactionallity for this transport.
        /// </summary>
        public override TransactionSupport GetTransactionSupport() => TransactionSupport.MultiQueue;

        /// <summary>
        /// Will be called if the transport has indicated that it has native support for pub sub.
        /// Creates a transport address for the input queue defined by a logical address.
        /// </summary>
        public override IManageSubscriptions GetSubscriptionManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Configures transport for receiving.
        /// </summary>
        protected internal override void ConfigureForReceiving(TransportReceivingConfigurationContext context)
        {
            context.SetMessagePumpFactory(ce => new MessagePump());
            context.SetQueueCreatorFactory(() => new QueueCreator());
        }

        /// <summary>
        /// Configures transport for sending.
        /// </summary>
        protected internal override void ConfigureForSending(TransportSendingConfigurationContext context)
        {
            context.SetDispatcherFactory(() => new Dispatcher());
        }

        /// <summary>
        /// Returns the list of supported delivery constraints for this transport.
        /// </summary>
        public override IEnumerable<Type> GetSupportedDeliveryConstraints()
        {
            return new List<Type>
            {
                typeof(DiscardIfNotReceivedBefore)
            };
        }

        /// <summary>
        /// Converts a given logical address to the transport address.
        /// </summary>
        /// <param name="logicalAddress">The logical address.</param>
        /// <returns>The transport address.</returns>
        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            return Path.Combine(logicalAddress.EndpointInstanceName.EndpointName.ToString(), logicalAddress.Qualifier ?? "");
        }

        /// <summary>
        /// Returns the outbound routing policy selected for the transport.
        /// </summary>
        public override OutboundRoutingPolicy GetOutboundRoutingPolicy(ReadOnlySettings settings)
        {
            return new OutboundRoutingPolicy(OutboundRoutingType.DirectSend, OutboundRoutingType.DirectSend, OutboundRoutingType.DirectSend);
        }
    }
}