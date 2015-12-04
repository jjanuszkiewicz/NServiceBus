﻿namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using MessageMutator;
    using Pipeline;
    using Pipeline.Contexts;

    class MutateIncomingMessageBehavior : Behavior<IncomingLogicalMessageContext>
    {
        public override async Task Invoke(IncomingLogicalMessageContext context, Func<Task> next)
        {
            var logicalMessage = context.Message;
            var current = logicalMessage.Instance;

            var mutatorContext = new MutateIncomingMessageContext(current, context.MessageHeaders);
            foreach (var mutator in context.Builder.BuildAll<IMutateIncomingMessages>())
            {
                await mutator.MutateIncoming(mutatorContext).ConfigureAwait(false);
            }

            if (mutatorContext.MessageChanged)
            {
                logicalMessage.UpdateMessageInstance(mutatorContext.Message);
            }

            await next().ConfigureAwait(false);
        }
    }
}