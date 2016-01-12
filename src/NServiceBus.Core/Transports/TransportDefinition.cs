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
        public bool RequireOutboxConsent { get; protected set; }

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
        /// Initializes all the factories used for the transport.
        /// </summary>
        /// <param name="settings">An instance of the current settings.</param>
        /// <returns>The supported factories.</returns>
        protected abstract FactoriesDefinitions Initialize(SettingsHolder settings);

        /// <summary>
        /// Gets an example connection string to use when reporting lack of configured connection string to the user.
        /// </summary>
        public abstract string ExampleConnectionStringForErrorMessage { get; }

        /// <summary>
        /// Used by implementations to control if a connection string is necessary.
        /// </summary>
        public virtual bool RequiresConnectionString => true;


        /// <summary>
        /// The factories for this transport.
        /// </summary>
        public FactoriesDefinitions Support { get; private set; }

        internal void InitializeTransportSupport(SettingsHolder settings)
        {
            Support = Initialize(settings);
        }
    }

    /// <summary>
    /// Transport factory definitions.
    /// </summary>
    public class FactoriesDefinitions
    {
        /// <summary>
        /// Creates a new instance of <see cref="FactoriesDefinitions"/>.
        /// </summary>
        /// <param name="createSendingConfiguration">The factory to create <see cref="IDispatchMessages"/>.</param>
        /// <param name="createReceivingConfiguration">The factory to create <see cref="IPushMessages"/>.</param>
        /// <param name="subscriptionManager">The instance of <see cref="IManageSubscriptions"/>.</param>
        /// <param name="deliveryConstraints">The delivery constraints.</param>
        /// <param name="transactionMode">The transaction mode.</param>
        /// <param name="outboundRoutingPolicy">The outbound routing policy.</param>
        public FactoriesDefinitions(IEnumerable<Type> deliveryConstraints, TransportTransactionMode transactionMode, OutboundRoutingPolicy outboundRoutingPolicy, Func<string, TransportSendingConfigurationResult> createSendingConfiguration, Func<string, TransportReceivingConfigurationResult> createReceivingConfiguration = null, IManageSubscriptions subscriptionManager = null)
        {
            DeliveryConstraints = deliveryConstraints;
            TransactionMode = transactionMode;
            OutboundRoutingPolicy = outboundRoutingPolicy;
            SubscriptionManager = subscriptionManager;
            CreateReceivingConfiguration = createReceivingConfiguration;
            CreateSendingConfiguration = createSendingConfiguration;
        }

        /// <summary>
        /// Gets the factory to receive message.
        /// </summary>
        public Func<string, TransportReceivingConfigurationResult> CreateReceivingConfiguration { get; }

        /// <summary>
        /// Gets the factory to send message.
        /// </summary>
        public Func<string, TransportSendingConfigurationResult> CreateSendingConfiguration { get; }

        /// <summary>
        /// Gets the instance to manage subscriptions.
        /// </summary>
        public IManageSubscriptions SubscriptionManager { get; }

        /// <summary>
        /// Returns the list of supported delivery constraints for this transport.
        /// </summary>
        public IEnumerable<Type> DeliveryConstraints { get; }

        /// <summary>
        /// Gets the highest supported transaction mode for the this transport.
        /// </summary>
        public TransportTransactionMode TransactionMode { get; }

        /// <summary>
        /// Returns the outbound routing policy selected for the transport.
        /// </summary>
        public OutboundRoutingPolicy OutboundRoutingPolicy { get; }
    }
}