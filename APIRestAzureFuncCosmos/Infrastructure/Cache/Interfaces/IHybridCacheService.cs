namespace Infrastructure.Cache.Interfaces;

public interface IHybridCacheService
{
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> fetchFromDb) where T : class;
    System.Threading.Tasks.Task RemoveAsync(string key);
}