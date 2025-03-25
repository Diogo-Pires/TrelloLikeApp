using Infrastructure.Cache.Interfaces;

namespace Infrastructure.Cache;

public abstract class BaseHybridCacheService
{
    public const string BASE_CACHEKEY_ALL = "all";

    public abstract string CacheKey { get; }

    protected async System.Threading.Tasks.Task ClearAllRequestFromCacheAsync(IHybridCacheService hybridCacheService) =>
        await hybridCacheService.RemoveAsync($"{CacheKey}{BASE_CACHEKEY_ALL}", CacheKey);
}