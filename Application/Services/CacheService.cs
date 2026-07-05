using Microsoft.Extensions.Caching.Distributed;
using scripture_hub_server.Application.Interfaces;
using System.Text.Json;

namespace scripture_hub_server.Application.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }


        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedValue = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedValue))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(24)
            };

            await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

    }
}
