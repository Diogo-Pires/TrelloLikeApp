namespace Domain.Task.Exceptions;

[Serializable]
public class TaskUpdateConcurrenctException(string taskId) : DomainException($"Concurrency conflict detected! Someone else modified the document.: {taskId}")
{
}