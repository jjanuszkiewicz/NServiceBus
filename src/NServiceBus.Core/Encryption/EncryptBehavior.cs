namespace NServiceBus
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline.OutgoingPipeline;
    using Pipeline;

    class EncryptBehavior : Behavior<IOutgoingLogicalMessageContext>
    {
        EncryptionInspector messageInspector;
        IEncryptionService encryptionService;

        public EncryptBehavior(EncryptionInspector messageInspector, IEncryptionService encryptionService)
        {
            this.messageInspector = messageInspector;
            this.encryptionService = encryptionService;
        }

        public override Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
        {
            var currentMessageToSend = context.Message.Instance;

            foreach (var item in messageInspector.ScanObject(currentMessageToSend))
            {
                EncryptMember(item.Item1, item.Item2, context);
            }

            context.UpdateMessageInstance(currentMessageToSend);

            return next();
        }

        void EncryptMember(object message, MemberInfo member, IOutgoingLogicalMessageContext context)
        {
            var valueToEncrypt = member.GetValue(message);

            var wireEncryptedString = valueToEncrypt as WireEncryptedString;
            if (wireEncryptedString != null)
            {
                encryptionService.EncryptValue(wireEncryptedString, context);
                return;
            }

            var stringToEncrypt = valueToEncrypt as string;
            if (stringToEncrypt != null)
            {
                encryptionService.EncryptValue(ref stringToEncrypt, context);

                member.SetValue(message, stringToEncrypt);
                return;
            }

            throw new Exception("Only string properties is supported for convention based encryption, please check your convention");
        }

        public class EncryptRegistration : RegisterStep
        {
            public EncryptRegistration()
                : base("InvokeEncryption", typeof(EncryptBehavior), "Invokes the encryption logic")
            {
                InsertAfter(WellKnownStep.MutateOutgoingMessages);
            }

        }
    }
}