namespace NServiceBus
{
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    class OutboundTransport
    {
        public TransportDefinition Definition { get; }
        public bool IsDefault { get; }

        public OutboundTransport(TransportDefinition definition, bool isDefault)
        {
            Definition = definition;
            IsDefault = isDefault;
        }

        public TransportSendInfrastructure Configure(ReadOnlySettings settings)
        {
            var connectionString = settings.Get<TransportConnectionString>().GetConnectionStringOrRaiseError(Definition);
            return Definition.Infrastructure.ConfigureSendInfrastructure(connectionString);
        }
    }
}