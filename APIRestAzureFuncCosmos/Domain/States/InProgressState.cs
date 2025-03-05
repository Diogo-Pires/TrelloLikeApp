using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Shared;
using Shared.Consts;
using Shared.Exceptions;
using Shared.Interfaces;

namespace Domain.States;

public class InProgressState : ITaskState
{
    public TaskItemStatus Status => TaskItemStatus.InProgress;

    public bool CanTransitionTo(TaskItemStatus newStatus) =>
        newStatus == TaskItemStatus.Completed || newStatus == TaskItemStatus.Cancelled;

    public void Start(TaskItem task)
    {
        throw new DomainException(Constants.VALIDATION_TASK_ALREADY_PROGRESS);
    }

    public void Complete(TaskItem task, IDateTimeProvider? dateTimeProvider = null)
    {
        dateTimeProvider ??= new DateTimeProvider();

        task.ChangeStatus(TaskItemStatus.Completed);
        task.SetCompletedAt(dateTimeProvider.GetUTCNow());
    }

    public void Cancel(TaskItem task)
    {
        task.ChangeStatus(TaskItemStatus.Cancelled);
    }
}
