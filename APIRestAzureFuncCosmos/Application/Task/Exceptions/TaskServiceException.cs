namespace Application.Task.Exceptions;

[Serializable]
internal class TaskServiceException(string? message, Exception? innerException) : Exception(message, innerException)
{
}