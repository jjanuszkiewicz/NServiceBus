﻿namespace NServiceBus
{
    using System.Collections.Generic;
    using NServiceBus.Pipeline;

    class OutgoingReplyContext : OutgoingContext, IOutgoingReplyContext
    {
        public OutgoingReplyContext(OutgoingLogicalMessage message, ReplyOptions options, IBehaviorContext parentContext)
            : base(options.MessageId, new Dictionary<string, string>(options.OutgoingHeaders), parentContext)
        {
            Message = message;
            Guard.AgainstNull(nameof(parentContext), parentContext);
            Guard.AgainstNull(nameof(message), message);
            Guard.AgainstNull(nameof(options), options);

            parentContext.Extensions.Merge(options.Context);
        }

        public OutgoingLogicalMessage Message { get; }
    }
}