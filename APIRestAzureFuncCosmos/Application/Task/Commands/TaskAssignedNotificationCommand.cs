using MediatR;
using Shared.Task.Model;

namespace Application.Task.Commands;

public record TaskAssignedNotificationCommand : TaskAssignedMessage, IRequest
{
    public TaskAssignedNotificationCommand(Guid taskId, string email) : base(taskId, email)
    {
    }
}