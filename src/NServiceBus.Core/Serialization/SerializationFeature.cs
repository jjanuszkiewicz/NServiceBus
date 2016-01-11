namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.Features;
    using NServiceBus.MessageInterfaces;
    using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
    using NServiceBus.Serialization;

    class SerializationFeature : Feature
    {
        internal SerializationFeature()
        {
            EnableByDefault();
            Defaults(s =>
            {
                s.SetDefault("AdditionalDeserializers", new List<SerializationDefinition>());
                s.SetDefault<SerializationDefinition>(new XmlSerializer());
            });
        }

        /// <summary>
        /// Called when the features is activated.
        /// </summary>
        protected internal sealed override void Setup(FeatureConfigurationContext context)
        {
            var mapper = new MessageMapper();
            var settings = context.Settings;
            var conventions = settings.Get<Conventions>();
            var messageTypes = settings.GetAvailableTypes().Where(conventions.IsMessageType);
            mapper.Initialize(messageTypes);

            var defaultSerializerDefinition = context.Settings.GetOrDefault<SerializationDefinition>();
            var defaultSerializer = CreateMessageSerializer(defaultSerializerDefinition, mapper, context);
            context.Container.ConfigureComponent(b => defaultSerializer, DependencyLifecycle.SingleInstance);

            var additionalDeserializerDefinitions = context.Settings.Get<List<SerializationDefinition>>("AdditionalDeserializers");
            var additionalDeserializers = additionalDeserializerDefinitions.Select(d => CreateMessageSerializer(d, mapper, context)).ToArray();

            context.Container.ConfigureComponent(_ => defaultSerializer, DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent(_ => new MessageDeserializerResolver(defaultSerializer, additionalDeserializers), DependencyLifecycle.SingleInstance); 
            context.Container.ConfigureComponent(_ => mapper,  DependencyLifecycle.SingleInstance);
        }

        static IMessageSerializer CreateMessageSerializer(SerializationDefinition definition, IMessageMapper mapper, FeatureConfigurationContext context)
        {
            var serializerFactory = definition.Configure(context.Settings);
            var serializer = serializerFactory(mapper);
            return serializer;
        }
    }
}