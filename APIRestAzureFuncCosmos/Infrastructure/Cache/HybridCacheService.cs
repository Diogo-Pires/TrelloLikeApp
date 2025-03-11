using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Infrastructure.Cache.Interfaces;

namespace Infrastructure.Cache;

public class HybridCacheService(IMemoryCache memoryCache, IDistributedCache distributedCache) : IHybridCacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> fetchFromDb) where T : class
    {
        // Try to get L1 cache
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue;
        }

        // Try to get L2 cache(Redis)
        var redisData = await _distributedCache.GetStringAsync(key);
        if (redisData != null)
        {
            cachedValue = JsonConvert.DeserializeObject<T>(redisData);
            _memoryCache.Set(key, cachedValue, _cacheDuration);

            return cachedValue;
        }

        // If nothing found, go to the DB
        cachedValue = await fetchFromDb();
        if (cachedValue != null)
        {
            await SetRedisCacheKey(key, cachedValue);
            _memoryCache.Set(key, cachedValue, _cacheDuration);
        }

        return cachedValue;
    }

    public async System.Threading.Tasks.Task SetIfNotExistsAsync<T>(string key, T data) where T : class
    {
        var redisData = await _distributedCache.GetStringAsync(key);
        if (redisData == null && data != null)
        {
            await SetRedisCacheKey(key, data);
        }

        if (!_memoryCache.TryGetValue(key, out T? _))
        {
            _memoryCache.Set(key, data, _cacheDuration);
        }
    }

    public async System.Threading.Tasks.Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        await _distributedCache.RemoveAsync(key);
    }

    private async System.Threading.Tasks.Task SetRedisCacheKey<T>(string key, T? cachedValue) where T : class
    {
        var serializedData = JsonConvert.SerializeObject(cachedValue);
        await _distributedCache.SetStringAsync(key, serializedData, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheDuration
        });
    }
}