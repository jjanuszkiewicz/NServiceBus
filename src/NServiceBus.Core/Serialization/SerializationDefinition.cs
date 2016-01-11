﻿namespace NServiceBus.Serialization
{
    using System;
    using NServiceBus.MessageInterfaces;
    using NServiceBus.Settings;

    /// <summary>
    /// Implemented by serializers to provide their capabilities.
    /// </summary>
    public abstract class SerializationDefinition
    {
        /// <summary>
        /// Provides a factory method for building a message serializer.
        /// </summary>
        protected internal abstract Func<IMessageMapper, IMessageSerializer> Configure(ReadOnlySettings settings);
    }
}