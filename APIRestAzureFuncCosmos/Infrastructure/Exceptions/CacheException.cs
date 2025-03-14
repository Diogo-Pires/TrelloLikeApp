namespace Infrastructure.Exceptions;

[Serializable]
public class CacheException(string? message) : Exception(message)
{
}