namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using System.Transactions;
    using NServiceBus.Features;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    using NServiceBus.Transports.Msmq;
    using NServiceBus.Transports.Msmq.Config;
    using TransactionSettings = NServiceBus.Unicast.Transport.TransactionSettings;

    /// <summary>
    /// Transport definition for MSMQ.
    /// </summary>
    public class MsmqTransport : TransportDefinition
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MsmqTransport"/>.
        /// </summary>
        public MsmqTransport()
        {
            RequireOutboxConsent = true;
        }

        /// <summary>
        /// Returns the discriminator for this endpoint instance.
        /// </summary>
        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance, ReadOnlySettings settings) => instance.AtMachine(Environment.MachineName);

        /// <summary>
        /// Converts a given logical address to the transport address.
        /// </summary>
        /// <param name="logicalAddress">The logical address.</param>
        /// <returns>The transport address.</returns>
        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            string machine;
            if (!logicalAddress.EndpointInstance.Properties.TryGetValue("Machine", out machine))
            {
                machine = Environment.MachineName;
            }

            var queue = new StringBuilder(logicalAddress.EndpointInstance.Endpoint.ToString());
            if (logicalAddress.EndpointInstance.Discriminator != null)
            {
                queue.Append("-" + logicalAddress.EndpointInstance.Discriminator);
            }
            if (logicalAddress.Qualifier != null)
            {
                queue.Append("." + logicalAddress.Qualifier);
            }
            return queue + "@" + machine;
        }

        /// <summary>
        /// Returns the canonical for of the given transport address so various transport addresses can be effectively compared and deduplicated.
        /// </summary>
        /// <param name="transportAddress">A transport address.</param>
        public override string MakeCanonicalForm(string transportAddress)
        {
            return MsmqAddress.Parse(transportAddress).ToString();
        }

        /// <summary>
        /// Foo.
        /// </summary>
        /// <param name="settings">Foo.</param>
        /// <returns>Foo.</returns>
        public override SupportedByTransport Initialize(SettingsHolder settings)
        {
            return new SupportedByTransport(TransportTransactionMode.TransactionScope, new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Unicast, OutboundRoutingType.Unicast),
                connectionString =>
                {
                    new CheckMachineNameForComplianceWithDtcLimitation().Check();

                    Func<IReadOnlyDictionary<string, string>, string> getMessageLabel;
                    settings.TryGet("Msmq.GetMessageLabel", out getMessageLabel);
                    var builder = new MsmqConnectionStringBuilder(connectionString).RetrieveSettings();

                    MsmqLabelGenerator messageLabelGenerator;
                    if (!settings.TryGet(out messageLabelGenerator))
                    {
                        messageLabelGenerator = headers => string.Empty;
                    }
                    return new TransportSendingConfigurationResult(
                        () => new MsmqMessageSender(builder, messageLabelGenerator),
                        () =>
                        {
                            var bindings = settings.Get<QueueBindings>();
                            new QueuePermissionChecker().CheckQueuePermissions(bindings.SendingAddresses);
                            var result = new MsmqTimeToBeReceivedOverrideCheck(settings).CheckTimeToBeReceivedOverrides();
                            return Task.FromResult(result);
                        });
                },
                connectionString =>
                {
                    new CheckMachineNameForComplianceWithDtcLimitation().Check();

                    var builder = connectionString != null
                        ? new MsmqConnectionStringBuilder(connectionString).RetrieveSettings()
                        : new MsmqSettings();

                    var transactionSettings = new TransactionSettings(settings);
                    var transactionOptions = new TransactionOptions
                    {
                        IsolationLevel = transactionSettings.IsolationLevel,
                        Timeout = transactionSettings.TransactionTimeout
                    };

                    return new TransportReceivingConfigurationResult(
                        () => new MessagePump(guarantee => SelectReceiveStrategy(guarantee, transactionOptions)),
                        () => new QueueCreator(builder),
                        () =>
                        {
                            var bindings = settings.Get<QueueBindings>();
                            new QueuePermissionChecker().CheckQueuePermissions(bindings.ReceivingAddresses);
                            return Task.FromResult(StartupCheckResult.Success);
                        });
                }
,
                new[]
            {
                typeof(DiscardIfNotReceivedBefore)
            });
        }

        ReceiveStrategy SelectReceiveStrategy(TransportTransactionMode minimumConsistencyGuarantee, TransactionOptions transactionOptions)
        {
            if (minimumConsistencyGuarantee == TransportTransactionMode.TransactionScope)
            {
                return new ReceiveWithTransactionScope(transactionOptions);
            }

            if (minimumConsistencyGuarantee == TransportTransactionMode.None)
            {
                return new ReceiveWithNoTransaction();
            }

            return new ReceiveWithNativeTransaction();
        }

        /// <summary>
        /// Gets an example connection string to use when reporting lack of configured connection string to the user.
        /// </summary>
        public override string ExampleConnectionStringForErrorMessage => "cacheSendConnection=true;journal=false;deadLetter=true";

        /// <summary>
        /// Used by implementations to control if a connection string is necessary.
        /// </summary>
        public override bool RequiresConnectionString => false;
    }
}