using Domain.Task.Enums;
using Domain.Task.States.Exceptions;
using Domain.Task.States.Interfaces;
using Shared.Consts;

namespace Domain.Task.States;

public static class TaskStateManager
{
    private static readonly IReadOnlyDictionary<TaskEntityStatus, Lazy<ITaskState>> _statusToStateDictionary =
        new Dictionary<TaskEntityStatus, Lazy<ITaskState>>()
        {
        { TaskEntityStatus.Pending, new Lazy<ITaskState>(() => new PendingState()) },
        { TaskEntityStatus.InProgress, new Lazy<ITaskState>(() => new InProgressState()) },
        { TaskEntityStatus.Completed, new Lazy<ITaskState>(() => new CompletedState()) },
        { TaskEntityStatus.Cancelled, new Lazy<ITaskState>(() => new CancelledState()) }
    };

    private static readonly IReadOnlyDictionary<TaskEntityStatus, Action<TaskEntity>> _statusToActionsDictionary =
        new Dictionary<TaskEntityStatus, Action<TaskEntity>>()
        {
        { TaskEntityStatus.InProgress, task => task.State.Start(task) },
        { TaskEntityStatus.Completed, task => task.State.Complete(task) },
        { TaskEntityStatus.Cancelled, task => task.State.Cancel(task) }
    };

    public static ITaskState GetState(TaskEntityStatus status)
    {
        if (_statusToStateDictionary.TryGetValue(status, out var state))
            return state.Value;

        throw new TaskStateException($"${Constants.VALIDATION_TASK_INVALID_STATUS}: {status}");
    }

    public static void ValidateStatusTransition(TaskEntityStatus newStatus,
                                                ITaskState currentState,
                                                TaskEntityStatus oldStatus)
    {
        if (!currentState.CanTransitionTo(newStatus))
        {
            throw new TaskStateException($"{Constants.VALIDATION_TASK_INVALID_STATUS_TRANSITION}: {oldStatus} → {newStatus}");
        }
    }

    public static void ApplyStateTransition(TaskEntity task,
                                            TaskEntityStatus newStatus,
                                            ITaskState currentState,
                                            TaskEntityStatus oldStatus)
    {
        ValidateStatusTransition(newStatus, currentState, oldStatus);

        if (_statusToActionsDictionary.ContainsKey(newStatus))
        {
            _statusToActionsDictionary[newStatus](task);
        }
        else
        {
            throw new TaskStateException($"{Constants.VALIDATION_TASK_INVALID_STATUS_TRANSITION}: {newStatus}");
        }
    }
}