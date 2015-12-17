﻿namespace NServiceBus
{
    using NServiceBus.Extensibility;
    using NServiceBus.OutgoingPipeline;
    using NServiceBus.Routing;

    /// <summary>
    /// Provides options for disabling the best practice enforcement.
    /// </summary>
    public static class BestPracticesOptionExtensions
    {
        /// <summary>
        /// Turns off the best practice enforcement for the given message.
        /// </summary>
        public static void DoNotEnforceBestPractices(this ExtendableOptions options)
        {
            options.Context.SetDoNotEnforceBestPractices();
        }

        /// <summary>
        /// Turns off the best practice enforcement for the given context.
        /// </summary>
        public static void DoNotEnforceBestPractices(this IOutgoingReplyContext context)
        {
            context.Extensions.SetDoNotEnforceBestPractices();
        }

        /// <summary>
        /// Turns off the best practice enforcement for the given context.
        /// </summary>
        public static void DoNotEnforceBestPractices(this IOutgoingSendContext context)
        {
            context.Extensions.SetDoNotEnforceBestPractices();
        }

        /// <summary>
        /// Turns off the best practice enforcement for the given context.
        /// </summary>
        public static void DoNotEnforceBestPractices(this ISubscribeContext context)
        {
            context.Extensions.SetDoNotEnforceBestPractices();
        }

        /// <summary>
        /// Turns off the best practice enforcement for the given context.
        /// </summary>
        public static void DoNotEnforceBestPractices(this IOutgoingPublishContext context)
        {
            context.Extensions.SetDoNotEnforceBestPractices();
        }

        /// <summary>
        /// Turns off the best practice enforcement for the given context.
        /// </summary>
        public static void DoNotEnforceBestPractices(this IUnsubscribeContext context)
        {
            context.Extensions.SetDoNotEnforceBestPractices();
        }

        static void SetDoNotEnforceBestPractices(this ContextBag context)
        {
            var bestPracticesOptions = new EnforceBestPracticesOptions
            {
                Enabled = false
            };
            context.Set(bestPracticesOptions);
        }
    }
}