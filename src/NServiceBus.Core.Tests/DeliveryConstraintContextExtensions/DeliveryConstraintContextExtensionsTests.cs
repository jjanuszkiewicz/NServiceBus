namespace NServiceBus.Core.Tests.DeliveryConstraintContextExtensions
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Features;
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

            protected override FactoriesDefinitions Initialize(SettingsHolder settings)
            {
                return new FactoriesDefinitions(s => new TransportSendingConfigurationResult(() => null, () => null));
            }

            public override string ExampleConnectionStringForErrorMessage { get; } = String.Empty;

            public override IEnumerable<Type> DeliveryConstraints => new[]
            {
                typeof(DelayDeliveryWith)
            };

            public override TransportTransactionMode TransactionMode { get; }
            public override OutboundRoutingPolicy OutboundRoutingPolicy { get; }
        }
    }
}