﻿namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    
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
        string GetDiscriminatorForThisEndpointInstance()
        {
            return "\\";
        }

        /// <summary>
        /// Gets the highest supported transaction mode for the this transport.
        /// </summary>
        public override TransportTransactionMode GetSupportedTransactionMode() => TransportTransactionMode.SendsAtomicWithReceive;
     
        /// <summary>
        /// Will be called if the transport has indicated that it has native support for pub sub.
        /// Creates a transport address for the input queue defined by a logical address.
        /// </summary>
        public override IManageSubscriptions GetSubscriptionManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the discriminator for this endpoint instance.
        /// </summary>
        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance, ReadOnlySettings settings)
        {
            return instance;
        }

        /// <summary>
        /// Configures transport for receiving.
        /// </summary>
        protected internal override TransportReceivingConfigurationResult ConfigureForReceiving(TransportReceivingConfigurationContext context)
        {
            return new TransportReceivingConfigurationResult(() => new DevelopmentTransportMessagePump(), () => new DevelopmentTransportQueueCreator(), () => Task.FromResult(StartupCheckResult.Success));
        }

        /// <summary>
        /// Configures transport for sending.
        /// </summary>
        protected internal override TransportSendingConfigurationResult ConfigureForSending(TransportSendingConfigurationContext context)
        {
            return new TransportSendingConfigurationResult(() => new Dispatcher(), () => Task.FromResult(StartupCheckResult.Success));
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
            return Path.Combine(logicalAddress.EndpointInstance.Endpoint.ToString(), logicalAddress.Qualifier ?? "");
        }

        /// <summary>
        /// Returns the outbound routing policy selected for the transport.
        /// </summary>
        public override OutboundRoutingPolicy GetOutboundRoutingPolicy(ReadOnlySettings settings)
        {
            return new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Unicast, OutboundRoutingType.Unicast);
        }
    }
}