using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Shared.Consts;
using Shared.Exceptions;
using Shared.Interfaces;

namespace Domain.States;

public class PendingState : ITaskState
{
    public TaskItemStatus Status => TaskItemStatus.Pending;

    public bool CanTransitionTo(TaskItemStatus newStatus) =>
        newStatus == TaskItemStatus.InProgress || newStatus == TaskItemStatus.Cancelled;

    public void Start(TaskItem task)
    {
        task.ChangeStatus(TaskItemStatus.InProgress);
    }

    public void Complete(TaskItem task, IDateTimeProvider? dateTimeProvider = null)
    {
        throw new DomainException(Constants.VALIDATION_TASK_MUST_BE_STARTED);
    }

    public void Cancel(TaskItem task)
    {
        task.ChangeStatus(TaskItemStatus.Cancelled);
    }
}