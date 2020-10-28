using System.Collections.Generic;
using System.Threading.Tasks;
using ArticleCheck.Core.NLP.Interfaces;

namespace ArticleCheck.UnitTests.Implementations
{
    public class InMemoryKeyValueStorage : IKeyValueStorage
    {
        private Dictionary<string, object> Cache { get; set; } = new Dictionary<string, object>();

        public Task SetAsync<T>(string key, T item)
        {
            Cache[key] = item;

            return Task.CompletedTask;
        }

        public T Get<T>(string key)
        {
            if (!Cache.ContainsKey(key))
            {
                return default;
            }

            return (T)Cache[key];
        }

        public Task<T> GetAsync<T>(string key)
        {
            return Task.FromResult(Get<T>(key));
        }

        public Task RemoveAsync(string key)
        {
            Cache.Remove(key);

            return Task.CompletedTask;
        }

        public bool Contains(string key)
        {
            return Cache.ContainsKey(key);
        }

        public Task<bool> ContainsAsync(string key)
        {
            return Task.FromResult(Contains(key));
        }
    }
}