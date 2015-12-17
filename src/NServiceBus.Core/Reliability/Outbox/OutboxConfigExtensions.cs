﻿namespace NServiceBus
{
    using Outbox;

    /// <summary>
    /// Config methods for the outbox.
    /// </summary>
    public static class OutboxConfigExtensions
    {
        /// <summary>
        /// Enables the outbox feature.
        /// </summary>
        /// <param name="config">The <see cref="BusConfiguration"/> instance to apply the settings to.</param>
        public static OutboxSettings EnableOutbox(this BusConfiguration config)
        {
            Guard.AgainstNull(nameof(config), config);

            var outboxSettings = new OutboxSettings(config.Settings);

            config.Settings.SetDefault<TransportTransactionMode>(TransportTransactionMode.ReceiveOnly);
            config.EnableFeature<Features.Outbox>();
            return outboxSettings;
        }
    }
}