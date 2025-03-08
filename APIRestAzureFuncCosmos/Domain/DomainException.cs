namespace Domain;

[Serializable]
public class DomainException(string? message) : Exception(message)
{
}