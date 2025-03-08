using Domain.Task.Enums;
using Domain.Task.States.Exceptions;
using Domain.Task.States.Interfaces;
using Shared.Consts;
using Shared.Interfaces;

namespace Domain.Task.States;

public class CancelledState : ITaskState
{
    public TaskEntityStatus Status => TaskEntityStatus.Pending;

    public bool CanTransitionTo(TaskEntityStatus newStatus) =>
        false;

    public void Start(TaskEntity task)
    {
        throw new TaskStateException(Constants.VALIDATION_TASK_CANNOT_START_CANCELLED);
    }

    public void Complete(TaskEntity task, IDateTimeProvider? dateTimeProvider = null)
    {
        throw new TaskStateException(Constants.VALIDATION_TASK_CANNOT_COMPLETE_CANCELLED);
    }

    public void Cancel(TaskEntity task)
    {
        throw new TaskStateException(Constants.VALIDATION_TASK_ALREADY_CANCELLED);
    }
}
