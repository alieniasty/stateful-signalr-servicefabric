using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.ServiceFabric.Data;

namespace SignalR.Hubs
{
    public class TestNotificationHub : BaseHub
    {
        readonly IHubContext _context;

        public TestNotificationHub(IReliableStateManager stateManager) : base(stateManager)
        {
            _context = GlobalHost.ConnectionManager.GetHubContext(this.GetType().Name);
        }

        public void Broadcast(string connectionId, object data)
        {
            _context.Clients.Client(connectionId).broadcast(data);
        }
    }
}
