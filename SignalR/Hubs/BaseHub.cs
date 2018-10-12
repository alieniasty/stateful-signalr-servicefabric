using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.ServiceFabric.Data;

namespace SignalR.Hubs
{
    public abstract class BaseHub : Hub
    {
        Guid _testUserId = Guid.Parse("9f5bc8eb-5d25-47db-b76f-731f9308774c");
        protected readonly IReliableStateManager StateManager;

        protected BaseHub(IReliableStateManager stateManager)
        {
            StateManager = stateManager;
        }

        public override async Task OnConnected()
        {
            var map = await StateManager.GetOrAddAsync<SignalrState<Guid, List<string>>>("UserConnectionMap");

            using (var tx = StateManager.CreateTransaction())
            {
                var connections = await map.TryGetValueAsync(tx, _testUserId);

                if (ConnectionAlreadySaved(Context.ConnectionId, ref connections))
                    return;

                if (UserHasAnyConnections(connections.Value))
                {
                    var list = connections.Value.ToList();
                    list.Add(Context.ConnectionId);
                    await map.SetAsync(tx, _testUserId, list);
                    await tx.CommitAsync();
                }
                else
                {
                    await map.SetAsync(tx, _testUserId, new List<string> { Context.ConnectionId });
                    await tx.CommitAsync();
                }
            }

            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            var map = await StateManager.GetOrAddAsync<SignalrState<Guid, string>>("UserConnectionMap");

            using (var tx = StateManager.CreateTransaction())
            {
                await map.TryRemoveAsync(tx, _testUserId);
                await tx.CommitAsync();
            }

            await base.OnDisconnected(stopCalled);
        }

        static bool UserHasAnyConnections(IEnumerable<string> connectionIds)
        {
            return connectionIds != null && connectionIds.Any();
        }

        static bool ConnectionAlreadySaved(string connectionId, ref ConditionalValue<List<string>> connections)
        {
            return connections.HasValue && connections.Value != null && connections.Value.Contains(connectionId);
        }
    }

}
