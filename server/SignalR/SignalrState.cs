using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace SignalR
{
    public class SignalrState<TKey, TValue> : IReliableState where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        public Uri Name { get; }

        readonly ConcurrentDictionary<TKey, TValue> _dict = new ConcurrentDictionary<TKey, TValue>();

        public SignalrState(string name)
        {
            Name = ToUri(name);
        }

        static Uri ToUri(string name)
        {
            return new Uri("frag://" + name, UriKind.Absolute);
        }

        public Task SetAsync(ITransaction tx, TKey key, TValue value)
        {
            _dict[key] = value;
            return Task.FromResult(0);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key)
        {
            TValue value;
            bool result = _dict.TryGetValue(key, out value);

            return Task.FromResult(new ConditionalValue<TValue>(result, value));
        }

        public Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key)
        {
            TValue outValue;
            return Task.FromResult(new ConditionalValue<TValue>(this._dict.TryRemove(key, out outValue), outValue));
        }
    }
}
