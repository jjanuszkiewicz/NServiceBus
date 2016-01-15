namespace NServiceBus.AcceptanceTests.FakeTransport
{
    using System;
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

        protected override TransportInfrastructure Initialize(SettingsHolder settings)
        {
            return new TransportInfrastructure(
                Enumerable.Empty<Type>(),
                TransportTransactionMode.ReceiveOnly,
                new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Unicast, OutboundRoutingType.Unicast),
                s => new TransportSendInfrastructure(() => new FakeDispatcher(), () => Task.FromResult(StartupCheckResult.Success)),
                s => new TransportReceiveInfrastructure(() => new FakeReceiver(settings.Get<Exception>()), () => new FakeQueueCreator(), () => Task.FromResult(StartupCheckResult.Success)));
        }

        public override string ExampleConnectionStringForErrorMessage => null;
    }
}