namespace NServiceBus.AcceptanceTests.FakeTransport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.Routing;
    using Settings;
    using Transports;

    public class FakeTransport : TransportDefinition
    {
        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance, ReadOnlySettings settings)
        {
            return instance;
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            return logicalAddress.ToString();
        }

        public override bool RequiresConnectionString => false;

        public override IEnumerable<Type> DeliveryConstraints => Enumerable.Empty<Type>();

        public override TransportTransactionMode TransactionMode => TransportTransactionMode.ReceiveOnly;

        public override OutboundRoutingPolicy OutboundRoutingPolicy => new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Unicast, OutboundRoutingType.Unicast);

        protected override FactoriesDefinitions Initialize(SettingsHolder settings)
        {
            return new FactoriesDefinitions(s => new TransportSendingConfigurationResult(() => new FakeDispatcher(), () => Task.FromResult(StartupCheckResult.Success)),
                s => new TransportReceivingConfigurationResult(() => new FakeReceiver(settings.Get<Exception>()), () => new FakeQueueCreator(), () => Task.FromResult(StartupCheckResult.Success)));
        }

        public override string ExampleConnectionStringForErrorMessage => null;
    }
}