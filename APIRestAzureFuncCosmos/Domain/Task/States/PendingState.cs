using Domain.Task.Enums;
using Domain.Task.States.Exceptions;
using Domain.Task.States.Interfaces;
using Shared.Consts;
using Shared.Interfaces;

namespace Domain.Task.States;

public class PendingState : ITaskState
{
    public TaskEntityStatus Status => TaskEntityStatus.Pending;

    public bool CanTransitionTo(TaskEntityStatus newStatus) =>
        newStatus == TaskEntityStatus.InProgress || newStatus == TaskEntityStatus.Cancelled;

    public void Start(TaskEntity task)
    {
        task.ChangeStatus(TaskEntityStatus.InProgress);
    }

    public void Complete(TaskEntity task, IDateTimeProvider? dateTimeProvider = null)
    {
        throw new TaskStateException(Constants.VALIDATION_TASK_MUST_BE_STARTED);
    }

    public void Cancel(TaskEntity task)
    {
        task.ChangeStatus(TaskEntityStatus.Cancelled);
    }
}