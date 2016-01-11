namespace NServiceBus.AcceptanceTests.FakeTransport
{
    using System;
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

        public override SupportedByTransport Initialize(SettingsHolder settings)
        {
            return new SupportedByTransport(TransportTransactionMode.ReceiveOnly,
                new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Unicast, OutboundRoutingType.Unicast),
                s => new TransportSendingConfigurationResult(() => new FakeDispatcher(), () => Task.FromResult(StartupCheckResult.Success)),
                s => new TransportReceivingConfigurationResult(() => new FakeReceiver(settings.Get<Exception>()), () => new FakeQueueCreator(), () => Task.FromResult(StartupCheckResult.Success)));
        }

        public override string ExampleConnectionStringForErrorMessage => null;
    }
}