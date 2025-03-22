namespace Shared.Task.Model;

public record TaskAssignedMessage
{
    public  Guid TaskId { get; set; }
    public  string Email { get; set; }

    public TaskAssignedMessage(Guid taskId, string email)
    {
        TaskId = taskId;
        Email = email;
    }
}