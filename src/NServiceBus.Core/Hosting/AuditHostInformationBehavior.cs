﻿namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Hosting;
    using NServiceBus.Pipeline;
    using NServiceBus.Routing;
    using NServiceBus.Support;

    class AuditHostInformationBehavior : Behavior<IAuditContext>
    {
        public AuditHostInformationBehavior(HostInformation hostInfo, EndpointName endpoint)
        {
            this.hostInfo = hostInfo;
            this.endpoint = endpoint;
        }

        public override Task Invoke(IAuditContext context, Func<Task> next)
        {
            context.AddAuditData(Headers.HostId, hostInfo.HostId.ToString("N"));
            context.AddAuditData(Headers.HostDisplayName, hostInfo.DisplayName);

            context.AddAuditData(Headers.ProcessingMachine, RuntimeEnvironment.MachineName);
            context.AddAuditData(Headers.ProcessingEndpoint, endpoint.ToString());

            return next();
        }

        HostInformation hostInfo;
        EndpointName endpoint;
    }
}