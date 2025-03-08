using Domain.Task.Enums;
using Domain.Task.States.Exceptions;
using Domain.Task.States.Interfaces;
using Shared;
using Shared.Consts;
using Shared.Interfaces;

namespace Domain.Task.States;

public class InProgressState : ITaskState
{
    public TaskEntityStatus Status => TaskEntityStatus.InProgress;

    public bool CanTransitionTo(TaskEntityStatus newStatus) =>
        newStatus == TaskEntityStatus.Completed || newStatus == TaskEntityStatus.Cancelled;

    public void Start(TaskEntity task)
    {
        throw new TaskStateException(Constants.VALIDATION_TASK_ALREADY_PROGRESS);
    }

    public void Complete(TaskEntity task, IDateTimeProvider? dateTimeProvider = null)
    {
        dateTimeProvider ??= new DateTimeProvider();

        task.ChangeStatus(TaskEntityStatus.Completed);
        task.SetCompletedAt(dateTimeProvider.GetUTCNow());
    }

    public void Cancel(TaskEntity task)
    {
        task.ChangeStatus(TaskEntityStatus.Cancelled);
    }
}
