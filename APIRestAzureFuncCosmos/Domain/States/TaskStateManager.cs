using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Shared.Consts;
using Shared.Exceptions;

namespace Domain.States;

public static class TaskStateManager
{
    private static readonly IReadOnlyDictionary<TaskItemStatus, Lazy<ITaskState>> _statusToStateDictionary =
        new Dictionary<TaskItemStatus, Lazy<ITaskState>>()
        {
        { TaskItemStatus.Pending, new Lazy<ITaskState>(() => new PendingState()) },
        { TaskItemStatus.InProgress, new Lazy<ITaskState>(() => new InProgressState()) },
        { TaskItemStatus.Completed, new Lazy<ITaskState>(() => new CompletedState()) },
        { TaskItemStatus.Cancelled, new Lazy<ITaskState>(() => new CancelledState()) }
    };

    private static readonly IReadOnlyDictionary<TaskItemStatus, Action<TaskItem>> _statusToActionsDictionary =
        new Dictionary<TaskItemStatus, Action<TaskItem>>()
        {
        { TaskItemStatus.InProgress, task => task.State.Start(task) },
        { TaskItemStatus.Completed, task => task.State.Complete(task) },
        { TaskItemStatus.Cancelled, task => task.State.Cancel(task) }
    };

    public static ITaskState GetState(TaskItemStatus status)
    {
        if (_statusToStateDictionary.TryGetValue(status, out var state))
            return state.Value;

        throw new DomainException($"${Constants.VALIDATION_TASK_INVALID_STATUS}: {status}");
    }

    public static void ValidateStatusTransition(TaskItemStatus newStatus,
                                                 ITaskState currentState,
                                                 TaskItemStatus oldStatus)
    {
        if (!currentState.CanTransitionTo(newStatus))
        {
            throw new DomainException($"{Constants.VALIDATION_TASK_INVALID_STATUS_TRANSITION}: {oldStatus} → {newStatus}");
        }
    }

    public static void ApplyStateTransition(TaskItem task,
                                            TaskItemStatus newStatus,
                                            ITaskState currentState,
                                            TaskItemStatus oldStatus)
    {
        ValidateStatusTransition(newStatus, currentState, oldStatus);

        if (_statusToActionsDictionary.ContainsKey(newStatus))
        {
            _statusToActionsDictionary[newStatus](task);
        }
        else
        {
            throw new DomainException($"{Constants.VALIDATION_TASK_INVALID_STATUS_TRANSITION}: {newStatus}");
        }
    }
}