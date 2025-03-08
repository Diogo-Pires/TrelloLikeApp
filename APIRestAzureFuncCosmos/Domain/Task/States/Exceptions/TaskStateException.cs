namespace Domain.Task.States.Exceptions;

[Serializable]
public class TaskStateException(string? message) : DomainException(message)
{
}