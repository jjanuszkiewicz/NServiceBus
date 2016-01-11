namespace NServiceBus
{
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    class InboundTransport
    {
        public TransportDefinition Definition { get; }

        public InboundTransport(TransportDefinition transportDefinition)
        {
            Definition = transportDefinition;
        }

        public TransportReceivingConfigurationResult Configure(ReadOnlySettings settings)
        {
            var connectionString = settings.Get<TransportConnectionString>().GetConnectionStringOrRaiseError(Definition);
            return Definition.Support.CreateReceivingConfiguration(connectionString);
        }        
    }
}