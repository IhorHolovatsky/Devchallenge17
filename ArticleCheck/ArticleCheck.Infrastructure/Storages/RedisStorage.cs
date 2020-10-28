using System.Threading.Tasks;
using ArticleCheck.Core.NLP.Interfaces;
using ArticleCheck.Infrastructure.Constants;
using ArticleCheck.Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace ArticleCheck.Infrastructure.Storages
{
    public class RedisStorage : IKeyValueStorage
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;

        public RedisStorage(IDistributedCache cache,
            IConnectionMultiplexer redis)
        {
            _cache = cache;
            _redis = redis;
        }

        public Task SetAsync<T>(string key, T item)
        {
            return _cache.SetAsync(key, item);
        }

        public T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }

        public Task<T> GetAsync<T>(string key)
        {
            return _cache.GetAsync<T>(key);
        }

        public Task RemoveAsync(string key)
        {
            return _redis.GetDatabase().KeyDeleteAsync($"{RedisConstants.InstanceName}{key}");
        }

        public bool Contains(string key)
        {
            return _redis.GetDatabase().KeyExists($"{RedisConstants.InstanceName}{key}");
        }

        public Task<bool> ContainsAsync(string key)
        {
            return _redis.GetDatabase().KeyExistsAsync($"{RedisConstants.InstanceName}{key}");
        }
    }
}