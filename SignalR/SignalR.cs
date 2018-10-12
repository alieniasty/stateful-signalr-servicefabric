using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace SignalR
{
    internal sealed class SignalR : StatefulService
    {
        public SignalR(StatefulServiceContext context)
            : base(context)
        { }
        
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var subPath = nameof(SignalR).ToLowerInvariant();
            return new ServiceReplicaListener[]
            {

                new ServiceReplicaListener(serviceContext =>
                    new OwinCommunicationListener(Startup.ConfigureApp,
                        this.StateManager,
                        serviceContext,
                        ServiceEventSource.Current,
                        "ServiceEndpointHttp",
                        $"api/{subPath}"))

            };

        }
    }
}
