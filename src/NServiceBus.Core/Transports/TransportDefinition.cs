namespace NServiceBus.Transports
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.Routing;
    using Settings;

    /// <summary>
    /// Defines a transport.
    /// </summary>
    public abstract partial class TransportDefinition
    {
        /// <summary>
        /// True if the transport.
        /// </summary>
        public bool RequireOutboxConsent { get; set; }

        /// <summary>
        /// Returns the discriminator for this endpoint instance.
        /// </summary>
        public abstract EndpointInstance BindToLocalEndpoint(EndpointInstance instance, ReadOnlySettings settings);

        /// <summary>
        /// Converts a given logical address to the transport address.
        /// </summary>
        /// <param name="logicalAddress">The logical address.</param>
        /// <returns>The transport address.</returns>
        public abstract string ToTransportAddress(LogicalAddress logicalAddress);

        /// <summary>
        /// Returns the canonical for of the given transport address so various transport addresses can be effectively compared and deduplicated.
        /// </summary>
        /// <param name="transportAddress">A transport address.</param>
        public virtual string MakeCanonicalForm(string transportAddress)
        {
            return transportAddress;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public abstract SupportedByTransport Initialize(SettingsHolder settings);

        /// <summary>
        /// Gets an example connection string to use when reporting lack of configured connection string to the user.
        /// </summary>
        public abstract string ExampleConnectionStringForErrorMessage { get; }

        /// <summary>
        /// Used by implementations to control if a connection string is necessary.
        /// </summary>
        public virtual bool RequiresConnectionString => true;

        internal void InitializeTransportSupport(SettingsHolder settings)
        {
            Support = Initialize(settings);
        }

        /// <summary>
        /// 
        /// </summary>
        public SupportedByTransport Support { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SupportedByTransport
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionMode"></param>
        /// <param name="outboundRoutingPolicy"></param>
        /// <param name="createSendingConfiguration"></param>
        /// <param name="createReceivingConfiguration"></param>
        /// <param name="deliveryConstraints"></param>
        /// <param name="subscriptionManager"></param>
        public SupportedByTransport(TransportTransactionMode transactionMode, OutboundRoutingPolicy outboundRoutingPolicy, Func<string, TransportSendingConfigurationResult> createSendingConfiguration, Func<string, TransportReceivingConfigurationResult> createReceivingConfiguration = null, IEnumerable<Type> deliveryConstraints = null, IManageSubscriptions subscriptionManager = null)
        {
            SubscriptionManager = subscriptionManager;
            DeliveryConstraints = deliveryConstraints;
            TransactionMode = transactionMode;
            OutboundRoutingPolicy = outboundRoutingPolicy;
            CreateReceivingConfiguration = createReceivingConfiguration;
            CreateSendingConfiguration = createSendingConfiguration;
        }

        /// <summary>
        /// 
        /// </summary>
        public Func<string, TransportReceivingConfigurationResult> CreateReceivingConfiguration { get; }
            
        /// <summary>
        ///     
        /// </summary>
        public Func<string, TransportSendingConfigurationResult> CreateSendingConfiguration { get; }

        /// <summary>
        /// 
        /// </summary>
        public IManageSubscriptions SubscriptionManager { get; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Type> DeliveryConstraints { get; }
        /// <summary>
        /// 
        /// </summary>
        public TransportTransactionMode TransactionMode { get; }
        /// <summary>
        /// 
        /// </summary>
        public OutboundRoutingPolicy OutboundRoutingPolicy { get; }
    }
}