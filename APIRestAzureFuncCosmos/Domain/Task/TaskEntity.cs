using Domain.Deadline;
using Domain.Task.Enums;
using Domain.Task.States;
using Domain.Task.States.Interfaces;
using Domain.User;
using Newtonsoft.Json;
using Shared;
using Shared.Interfaces;

namespace Domain.Task;

public class TaskEntity
{
    [JsonProperty("id")]
    public Guid Id { get; private set; }

    [JsonProperty("title")]
    public string Title { get; private set; }

    [JsonProperty("description")]
    public string Description { get; private set; }

    [JsonProperty("status")]
    public TaskEntityStatus Status { get; private set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; private set; }

    [JsonProperty("completedAt")]
    public DateTime? CompletedAt { get; private set; }

    [JsonProperty("deadline")]
    public DateTime? Deadline { get; private set; }

    [JsonProperty("assignedUseBrEmail")]
    public string? AssignedUserEmail { get; private set; }

    [JsonProperty("_etag")]
    public string? ETag { get; private set; } = null;

    [JsonIgnore]
    public ITaskState State => TaskStateManager.GetState(Status);

    public TaskEntity(string title,
                      string description,
                      DateTime? deadline,
                      TaskEntityStatus? taskEntityStatus,
                      string? assignedUserEmail,
                      IDateTimeProvider dateTimeProvider)
    {
        var deadlineObj = new DeadlineValueObject(deadline, CreatedAt, dateTimeProvider);

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        CreatedAt = dateTimeProvider != null ? dateTimeProvider.GetUTCNow() : new DateTimeProvider().GetUTCNow();
        Deadline = deadlineObj.Value;
        Status = taskEntityStatus ?? TaskEntityStatus.Pending;
        AssignedUserEmail = assignedUserEmail;
    }

    public void SetCompletedAt(DateTime dateTime) => CompletedAt = dateTime;

    public void UpdateTask(string title, string description, DateTime? deadline, TaskEntityStatus? newTaskEntityStatus)
    {
        if (title != null && title != Title)
            Title = title.Trim();

        if (description != null && description != Description)
            Description = description.Trim();

        if (deadline.HasValue && deadline != Deadline)
            Deadline = deadline;

        UpdateTaskState(newTaskEntityStatus);
    }

    public void ChangeStatus(TaskEntityStatus newStatus)
    {
        TaskStateManager.ValidateStatusTransition(newStatus, State, Status);
        Status = newStatus;
    }

    public void AssignToUser(UserEntity user)
    {
        AssignedUserEmail = user.Id;
    }

    private void UpdateTaskState(TaskEntityStatus? newTaskEntityStatus)
    {
        if (newTaskEntityStatus != null && newTaskEntityStatus != Status)
        {
            TaskStateManager.ApplyStateTransition(this, (TaskEntityStatus)newTaskEntityStatus, State, Status);
        }
    }
}