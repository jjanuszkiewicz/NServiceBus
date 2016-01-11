namespace NServiceBus
{
    using System;
    using System.Text;
    using NServiceBus.MessageInterfaces;
    using NServiceBus.Settings;
    using Serialization;

    /// <summary>
    /// Defines the capabilities of the JSON serializer.
    /// </summary>
    public class JsonSerializer : SerializationDefinition
    {
        /// <summary>
        /// Provides a factory method for building a message serializer.
        /// </summary>
        protected internal override Func<IMessageMapper, IMessageSerializer> Configure(ReadOnlySettings settings)
        {
            var encoding = settings.GetOrDefault<Encoding>("Serialization.Json.Encoding") ?? Encoding.UTF8;
            return mapper => new JsonMessageSerializer(mapper, encoding);
        }
    }
}