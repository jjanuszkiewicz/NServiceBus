namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Features;
    using NServiceBus.Transports;

    class Receiving : Feature
    {
        internal Receiving()
        {
            EnableByDefault();
            DependsOn<Transport>();
            Prerequisite(c => !c.Settings.GetOrDefault<bool>("Endpoint.SendOnly"), "Endpoint is configured as send-only");
            Defaults(s =>
            {
                var receiveAddress = s.Get<TransportAddresses>().GetTransportAddress(s.RootLogicalAddress());
                s.SetDefault("NServiceBus.LocalAddress", receiveAddress);
            });
        }

        /// <summary>
        /// <see cref="Feature.Setup"/>.
        /// </summary>
        protected internal override void Setup(FeatureConfigurationContext context)
        {
            var inboundTransport = context.Settings.Get<InboundTransport>();

            context.Settings.Get<QueueBindings>().BindReceiving(context.Settings.LocalAddress());

            context.Container.RegisterSingleton(inboundTransport.Definition);

            var lazyReceiveConfigResult = new Lazy<TransportReceivingConfigurationResult>(()=> inboundTransport.Configure(context.Settings));
            context.Container.ConfigureComponent(b => lazyReceiveConfigResult.Value.MessagePumpFactory(), DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureComponent(b => lazyReceiveConfigResult.Value.QueueCreatorFactory(), DependencyLifecycle.SingleInstance);

            context.RegisterStartupTask(new PrepareForReceiving(lazyReceiveConfigResult));
        }
        
        class PrepareForReceiving : FeatureStartupTask
        {
            private readonly Lazy<TransportReceivingConfigurationResult> lazy;

            public PrepareForReceiving(Lazy<TransportReceivingConfigurationResult> lazy)
            {
                this.lazy = lazy;
            }

            protected override async Task OnStart(IBusSession session)
            {
                var result = await lazy.Value.PreStartupCheck().ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    throw new Exception($"Pre start-up check failed: {result.ErrorMessage}");
                }
            }

            protected override Task OnStop(IBusSession session)
            {
                return TaskEx.Completed;
            }
        }
    }
}
