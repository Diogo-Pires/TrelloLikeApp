using Domain.Task.Enums;
using Domain.Task.States.Exceptions;
using Domain.Task.States.Interfaces;
using Shared.Consts;
using Shared.Interfaces;

namespace Domain.Task.States;

public class CompletedState : ITaskState
{
    public TaskEntityStatus Status => TaskEntityStatus.Completed;

    public bool CanTransitionTo(TaskEntityStatus newStatus) =>
        false;

    public void Start(TaskEntity task)
    {
        throw new TaskStateException(Constants.VALIDATION_TASK_RESTART_COMPLETED_TASK);
    }

    public void Complete(TaskEntity task, IDateTimeProvider? dateTimeProvider = null)
    {
        throw new TaskStateException(Constants.VALIDATION_TASK_ALREADY_COMPLETED);
    }

    public void Cancel(TaskEntity task)
    {
        throw new TaskStateException(Constants.VALIDATION_TASK_CANNOT_CANCEL_COMPLETE);
    }
}
