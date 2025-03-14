namespace Infrastructure.Cache.Interfaces;

public interface IHybridCacheService
{
    Task<T?> GetOrSetAsync<T>(string key, string tag, Func<Task<T?>> fetchFromDb) where T : class;
    System.Threading.Tasks.Task SetIfNotExistsAsync<T>(string key, string tag, T data) where T : class;
    System.Threading.Tasks.Task RemoveAsync(string key, string tag);
    System.Threading.Tasks.Task IncrementVersion(string tag);
}