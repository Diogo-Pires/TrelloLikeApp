namespace Domain.User.Exceptions;

[Serializable]
public class UserException(string? message) : DomainException(message)
{
}