using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ArticleCheck.Infrastructure.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken token = default)
        {
            var json = await cache.GetStringAsync(key, token);

            return JsonConvert.DeserializeObject<T>(json ?? string.Empty);
        }

        public static T Get<T>(this IDistributedCache cache, string key)
        {
            var json = cache.GetString(key);

            return JsonConvert.DeserializeObject<T>(json ?? string.Empty);
        }

        public static Task SetAsync<T>(this IDistributedCache cache,
            string key,
            T value,
            CancellationToken token = default)
        {
            var json = JsonConvert.SerializeObject(value);

            return cache.SetStringAsync(key, json, token);
        }

        public static Task SetAsync<T>(this IDistributedCache cache,
            string key,
            T value,
            DistributedCacheEntryOptions options,
            CancellationToken token = default)
        {
            var json = JsonConvert.SerializeObject(value);

            return cache.SetStringAsync(key, json, options, token);
        }
    }

}