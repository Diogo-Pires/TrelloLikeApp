using Domain.Task.Enums;
using Shared.Interfaces;

namespace Domain.Task.States.Interfaces;

public interface ITaskState
{
    TaskEntityStatus Status { get; }
    bool CanTransitionTo(TaskEntityStatus newStatus);
    void Start(TaskEntity task);
    void Complete(TaskEntity task, IDateTimeProvider? dateTimeProvider = null);
    void Cancel(TaskEntity task);
}
