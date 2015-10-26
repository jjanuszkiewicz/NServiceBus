namespace NServiceBus.Transports.FileBased
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.Routing;

    class Dispatcher : IDispatchMessages
    {
        public Task Dispatch(IEnumerable<TransportOperation> outgoingMessages, ContextBag context)
        {
            foreach (var transportOperation in outgoingMessages)
            {
                var addressTag = transportOperation.DispatchOptions.AddressTag as UnicastAddressTag;

                if (addressTag == null)
                {
                    throw new InvalidOperationException("The filebased transport only support unicast addressing");
                }

                var basePath = Path.Combine("c:\\bus", addressTag.Destination);
                var bodyPath = Path.Combine(basePath, ".bodies", transportOperation.Message.MessageId) + ".xml"; //TODO: pick the correct ending based on the serialized type

                //todo: this is needed since handleCurrentMessageLater does a send local with the same msg id
                if (!File.Exists(bodyPath))
                {
                    File.WriteAllBytes(bodyPath, transportOperation.Message.Body);
                }

                var messageContents = new List<string>
                {
                    bodyPath,
                    HeaderSerializer.ToXml(transportOperation.Message.Headers)
                };

                DirectoryBasedTransaction transaction;

                var messagePath = Path.Combine(basePath, transportOperation.Message.MessageId) + ".txt";

                if (transportOperation.DispatchOptions.RequiredDispatchConsistency != DispatchConsistency.Isolated &&
                    context.TryGet(out transaction))
                {               
                    transaction.Enlist(messagePath, messageContents);
                }
                else
                {
                    var tempFile = Path.GetTempFileName();

                    //write to temp file first so we can do a atomic move 
                    //this avoids the file being locked when the receiver tries to process it
                    File.WriteAllLines(tempFile, messageContents);
                    File.Move(tempFile, messagePath);
                }
            }

            return TaskEx.Completed;
        }
    }
}