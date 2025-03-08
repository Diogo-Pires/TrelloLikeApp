namespace Domain.Deadline.Exceptions;

[Serializable]
public class DeadlineException(string? message) : DomainException(message)
{
}