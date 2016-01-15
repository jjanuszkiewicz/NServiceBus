namespace NServiceBus.Transports
{
    using System;

    /// <summary>
    /// Represents the result for configuring the transport for subscribing.
    /// </summary>
    public class TransportSubcriptionInfrastructure
    {
        /// <summary>
        /// Creates new result object.
        /// </summary>
        public TransportSubcriptionInfrastructure(Func<IManageSubscriptions> subscriptionManagerFactory)
        {
            Guard.AgainstNull(nameof(subscriptionManagerFactory), subscriptionManagerFactory);
            SubscriptionManagerFactory = subscriptionManagerFactory;
        }

        internal Func<IManageSubscriptions> SubscriptionManagerFactory { get; }
    }
}