﻿namespace NServiceBus.Routing
{
    using System;
    using NServiceBus.Pipeline;

    /// <summary>
    /// Provides context for subscription requests.
    /// </summary>
    public interface ISubscribeContext : IBehaviorContext
    {
        /// <summary>
        /// The type of the event.
        /// </summary>
        Type EventType { get; }
    }
}