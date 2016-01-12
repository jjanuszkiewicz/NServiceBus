namespace NServiceBus.Core.Tests.DeliveryConstraintContextExtensions
{
    using System;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Features;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    using NUnit.Framework;

    [TestFixture]
    public class DeliveryConstraintContextExtensionsTests
    {
        [Test]
        public void Should_be_able_to_determine_if_delivery_constraint_is_supported()
        {
            var settings = new SettingsHolder();
            var fakeTransportDefinition = new FakeTransportDefinition();
            settings.Set<TransportDefinition>(fakeTransportDefinition);
            var context = new FeatureConfigurationContext(settings, null, null);
            fakeTransportDefinition.InitializeTransportSupport(settings);
            var result = context.DoesTransportSupportConstraint<DeliveryConstraint>();
            Assert.IsTrue(result);
        }

        class FakeTransportDefinition : TransportDefinition
        {
            public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance, ReadOnlySettings settings)
            {
                throw new NotImplementedException();
            }

            public override string ToTransportAddress(LogicalAddress logicalAddress)
            {
                throw new NotImplementedException();
            }

            public override SupportedByTransport Initialize(SettingsHolder settings)
            {
                return new SupportedByTransport(TransportTransactionMode.None, new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Unicast, OutboundRoutingType.Unicast),
                    s => new TransportSendingConfigurationResult(() => null, () => null), deliveryConstraints: new[]
                    {
                        typeof(DiscardIfNotReceivedBefore)
                    });
            }

            public override string ExampleConnectionStringForErrorMessage { get; } = String.Empty;
        }
    }
}