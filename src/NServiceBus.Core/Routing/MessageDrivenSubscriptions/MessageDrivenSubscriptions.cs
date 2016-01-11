namespace NServiceBus.Features
{
    using System;
    using NServiceBus.Transports;

    /// <summary>
    /// Allows subscribers to register by sending a subscription message to this endpoint.
    /// </summary>
    public class MessageDrivenSubscriptions : Feature
    {

        internal MessageDrivenSubscriptions()
        {
            EnableByDefault();
        }

        /// <summary>
        /// See <see cref="Feature.Setup"/>.
        /// </summary>
        protected internal override void Setup(FeatureConfigurationContext context)
        {
            var transportDefinition = context.Settings.Get<TransportDefinition>();
            if (transportDefinition.Support.OutboundRoutingPolicy.Publishes != OutboundRoutingType.Unicast)
            {
                var message = $"The transport {transportDefinition.GetType().Name} supports native publish-subscribe so subscriptions are not managed by the transport in the publishing endpoint.";
                throw new Exception(message);
            }

            context.Pipeline.Register<SubscriptionReceiverBehavior.Registration>();
            var authorizer = context.Settings.GetSubscriptionAuthorizer();
            if (authorizer == null)
            {
                authorizer = _ => true;
            }
            context.Container.RegisterSingleton(authorizer);
        }
    }
}