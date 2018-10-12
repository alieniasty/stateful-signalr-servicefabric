using System;
using System.Collections.Generic;
using System.Fabric;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Owin;

namespace SignalR
{
    internal class OwinCommunicationListener : ICommunicationListener
    {
        readonly ServiceEventSource _eventSource;
        readonly Action<IAppBuilder, IReliableStateManager> _startup;
        readonly StatefulServiceContext _serviceContext;
        readonly string _endpointName;
        readonly string _appRoot;

        IDisposable _webApp;
        string _publishAddress;
        string _listeningAddress;
        readonly IReliableStateManager _stateManager;

        public OwinCommunicationListener(
            Action<IAppBuilder, IReliableStateManager> startup, 
            IReliableStateManager stateManager, 
            StatefulServiceContext serviceContext, 
            ServiceEventSource eventSource, 
            string endpointName)
            : this(startup, stateManager, serviceContext, eventSource, endpointName, null)
        {
        }

        public OwinCommunicationListener(
            Action<IAppBuilder, IReliableStateManager> startup, 
            IReliableStateManager stateManager, 
            StatefulServiceContext serviceContext, 
            ServiceEventSource eventSource, 
            string endpointName, 
            string appRoot)
        {
            if (startup == null) throw new ArgumentNullException(nameof(startup));

            if (serviceContext == null) throw new ArgumentNullException(nameof(serviceContext));

            if (endpointName == null) throw new ArgumentNullException(nameof(endpointName));

            if (eventSource == null) throw new ArgumentNullException(nameof(eventSource));

            this._startup = startup;
            this._serviceContext = serviceContext;
            this._endpointName = endpointName;
            this._eventSource = eventSource;
            this._appRoot = appRoot;
            this._stateManager = stateManager;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serviceEndpoint = _serviceContext.CodePackageActivationContext.GetEndpoint(_endpointName);
            var port = serviceEndpoint.Port;
            var protocol = serviceEndpoint.Protocol;

            if (_serviceContext != null)
            {
                var statefulServiceContext = _serviceContext;

                _listeningAddress = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}://+:{1}/{2}{3}/{4}/{5}",
                    protocol,
                    port,
                    string.IsNullOrWhiteSpace(_appRoot)
                        ? string.Empty
                        : _appRoot.TrimEnd('/') + '/',
                    statefulServiceContext.PartitionId,
                    statefulServiceContext.ReplicaId,
                    Guid.NewGuid());
            }

            _publishAddress = _listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            try
            {
                _eventSource.ServiceMessage(_serviceContext, "Starting web server on " + _listeningAddress);

                _webApp = WebApp.Start(_listeningAddress, appBuilder => _startup.Invoke(appBuilder, _stateManager));

                _eventSource.ServiceMessage(_serviceContext, "Listening on " + _publishAddress);

                return Task.FromResult(_publishAddress);
            }
            catch (Exception ex)
            {
                _eventSource.ServiceMessage(_serviceContext, "Web server failed to open. " + ex);

                StopWebServer();
                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _eventSource.ServiceMessage(_serviceContext, "Closing web server");

            StopWebServer();

            return Task.FromResult(true);
        }

        public void Abort()
        {
            _eventSource.ServiceMessage(_serviceContext, "Aborting web server");

            StopWebServer();
        }

        void StopWebServer()
        {
            if (_webApp == null) return;
            try
            {
                _webApp.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // no-op
            }
        }
    }

}
