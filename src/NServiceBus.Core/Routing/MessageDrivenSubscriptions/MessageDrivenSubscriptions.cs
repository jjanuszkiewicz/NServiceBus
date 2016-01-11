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
                return;
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