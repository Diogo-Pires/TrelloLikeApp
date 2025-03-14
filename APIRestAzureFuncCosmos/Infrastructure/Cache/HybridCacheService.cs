using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Exceptions;
using Shared.Consts;

namespace Infrastructure.Cache;

public class HybridCacheService(IMemoryCache memoryCache, IDistributedCache distributedCache) : IHybridCacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);

    public async Task<T?> GetOrSetAsync<T>(string key, string tag, Func<Task<T?>> fetchFromDb) where T : class
    {
        var version = await GetTagVersion(tag);
        var keyWithVersion = $"{key}:{version}";

        var cachedValue = await TryToGetFromCache<T>(keyWithVersion);
        if (cachedValue != null || cachedValue != default)
            return cachedValue;

        // If nothing found, go to the DB
        cachedValue = await fetchFromDb();
        if (cachedValue != null)
        {
            await SetRedisCacheKey(keyWithVersion, cachedValue);
            _memoryCache.Set(keyWithVersion, cachedValue, _cacheDuration);
        }

        return cachedValue;
    }

    public async System.Threading.Tasks.Task SetIfNotExistsAsync<T>(string key, string tag, T data) where T : class
    {
        var version = await GetTagVersion(tag);
        var keyWithVersion = $"{key}:{version}";

        var redisData = await _distributedCache.GetStringAsync(keyWithVersion);
        if (redisData == null && data != null)
        {
            await SetRedisCacheKey(keyWithVersion, data);
        }

        if (!_memoryCache.TryGetValue(keyWithVersion, out T? _))
        {
            _memoryCache.Set(keyWithVersion, data, _cacheDuration);
        }
    }

    public async System.Threading.Tasks.Task RemoveAsync(string key, string tag)
    {
        var version = await GetTagVersion(tag);
        var keyWithVersion = $"{key}:{version}";

        _memoryCache.Remove(keyWithVersion);
        await _distributedCache.RemoveAsync(keyWithVersion);
    }

    public async System.Threading.Tasks.Task IncrementVersion(string tag)
    {
        var tagVersionkey = $"tag:{tag}version";
        var version = await TryToGetFromCache<string>(tagVersionkey);
        if (string.IsNullOrWhiteSpace(version))
        {
            version = "2";
        }

        if (int.TryParse(version, out int value))
        {
            var newVersionValue = (value + 1).ToString();
            _memoryCache.Set(tagVersionkey, newVersionValue, _cacheDuration);
            await SetRedisCacheKey<string>(tagVersionkey, newVersionValue);
            return;
        }

        throw new CacheException(Constants.CACHE_VERSION_WRONG_FORMAT);
    }

    private async System.Threading.Tasks.Task SetRedisCacheKey<T>(string key, T? cachedValue) where T : class
    {
        var serializedData = JsonConvert.SerializeObject(cachedValue);
        await _distributedCache.SetStringAsync(key, serializedData, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheDuration
        });
    }

    private async Task<string> GetTagVersion(string tag)
    {
        var tagVersionkey = $"tag:{tag}version";
        var version = await TryToGetFromCache<string>(tagVersionkey);
        if (string.IsNullOrWhiteSpace(version))
        {
            version = "1";
            _memoryCache.Set(tagVersionkey, version, _cacheDuration);
            await SetRedisCacheKey<string>(tagVersionkey, version);
        }

        return version;
    }

    private async Task<T?> TryToGetFromCache<T>(string key) where T : class
    {
        T? cachedValue = null;

        // Try to get L1 cache
        if (_memoryCache.TryGetValue(key, out cachedValue))
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

        return null;
    }
}