using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Shared.Consts;
using Shared.Exceptions;
using Shared.Interfaces;

namespace Domain.States;

public class CompletedState : ITaskState
{
    public TaskItemStatus Status => TaskItemStatus.Completed;

    public bool CanTransitionTo(TaskItemStatus newStatus) =>
        false;

    public void Start(TaskItem task)
    {
        throw new DomainException(Constants.VALIDATION_TASK_RESTART_COMPLETED_TASK);
    }

    public void Complete(TaskItem task, IDateTimeProvider? dateTimeProvider = null)
    {
        throw new DomainException(Constants.VALIDATION_TASK_ALREADY_COMPLETED);
    }

    public void Cancel(TaskItem task)
    {
        throw new DomainException(Constants.VALIDATION_TASK_CANNOT_CANCEL_COMPLETE);
    }
}
