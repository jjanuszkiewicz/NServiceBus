namespace NServiceBus.Pipeline
{
    using System;

    /// <summary>
    /// Allows steps to be registered in order.
    /// </summary>
    public class StepRegistrationSequence
    {
        Action<RegisterStep> addStep;

        internal StepRegistrationSequence(Action<RegisterStep> addStep)
        {
            this.addStep = addStep;
        }

        /// <summary>
        /// Register a new step into the pipeline.
        /// </summary>
        /// <param name="stepId">The identifier of the new step to add.</param>
        /// <param name="behavior">The <see cref="Behavior{TContext}"/> to execute.</param>
        /// <param name="description">The description of the behavior.</param>
        public StepRegistrationSequence Register(string stepId, Type behavior, string description)
        {
            BehaviorTypeChecker.ThrowIfInvalid(behavior, "behavior");

            Guard.AgainstNullAndEmpty(nameof(stepId), stepId);
            Guard.AgainstNullAndEmpty(nameof(description), description);

            var step = RegisterStep.Create(stepId, behavior, description);
            addStep(step);
            return this;
        }


        /// <summary>
        /// <see cref="Register(string,System.Type,string)"/>.
        /// </summary>
        /// <param name="wellKnownStep">The identifier of the step to add.</param>
        /// <param name="behavior">The <see cref="Behavior{TContext}"/> to execute.</param>
        /// <param name="description">The description of the behavior.</param>
        public StepRegistrationSequence Register(WellKnownStep wellKnownStep, Type behavior, string description)
        {
            Guard.AgainstNull(nameof(wellKnownStep), wellKnownStep);

            Register((string)wellKnownStep, behavior, description);
            return this;
        }
    }
}