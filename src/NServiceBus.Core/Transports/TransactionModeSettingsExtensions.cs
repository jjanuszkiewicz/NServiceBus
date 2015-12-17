namespace NServiceBus.ConsistencyGuarantees
{
    using System;
    using Settings;
    using Transports;

    /// <summary>
    /// Extension methods to provide access to various consistency releated convenience methods.
    /// </summary>
    public static class TransactionModeSettingsExtensions
    {
        /// <summary>
        /// Returns the transactions required by the transport.
        /// </summary>
        public static TransportTransactionMode GetRequiredTransactionModeForReceives(this ReadOnlySettings settings)
        {
            var transportTransactionSupport = settings.Get<TransportDefinition>().GetSupportedTransactionMode();

            TransportTransactionMode requestedTransportTransactionMode;
            
            //if user haven't asked for a explicit level use what the transport supports
            if (!settings.TryGet(out requestedTransportTransactionMode))
            {
                return transportTransactionSupport;
            }

            if (requestedTransportTransactionMode > transportTransactionSupport)
            {
                    throw new Exception($"Requested transaction mode `{requestedTransportTransactionMode}` can't be satisfied since the transport only supports `{transportTransactionSupport}`");
            }

            return requestedTransportTransactionMode;
        }
    }
}