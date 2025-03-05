using Domain.Enums;
using Domain.Interfaces;
using Domain.States;
using Domain.ValueObjects;
using Newtonsoft.Json;
using Shared;
using Shared.Consts;
using Shared.Exceptions;
using Shared.Interfaces;

namespace Domain.Entities;

public class TaskItem
{
    [JsonProperty("id")]
    public Guid Id { get; private set; }

    [JsonProperty("title")]
    public string Title { get; private set; }

    [JsonProperty("description")]
    public string Description { get; private set; }

    [JsonProperty("status")]
    public TaskItemStatus Status { get; private set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; private set; }

    [JsonProperty("completedAt")]
    public DateTime? CompletedAt { get; private set; }

    [JsonProperty("deadline")]
    public DateTime? Deadline { get; private set; }

    [JsonProperty("assignedUserEmail")]
    public string? AssignedUserEmail { get; private set; }

    [JsonIgnore]
    public ITaskState State => TaskStateManager.GetState(Status);

    [JsonIgnore]
    public User? AssignedUser { get; private set; }

    public TaskItem(string title,
                    string description,
                    DateTime? deadline,
                    TaskItemStatus? taskItemStatus,
                    User? assignedUser,
                    IDateTimeProvider dateTimeProvider)
    {
        var deadlineObj = new Deadline(deadline, CreatedAt, dateTimeProvider);
        
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        CreatedAt = dateTimeProvider != null ? dateTimeProvider.GetUTCNow() : new DateTimeProvider().GetUTCNow();
        Deadline = deadlineObj.Value;
        Status = taskItemStatus ?? TaskItemStatus.Pending;
        AssignedUser = assignedUser;
    }

    public void SetCompletedAt(DateTime dateTime) => CompletedAt = dateTime;

    public static void ValidateCreation(Guid? id,
                                        string title,
                                        string description,
                                        DateTime? deadline,
                                        DateTime? createdAt,
                                        DateTime? completedAt,
                                        IDateTimeProvider? dateTimeProvider = null)
    {
        BasicValidation(title, description, deadline, createdAt, completedAt, dateTimeProvider);

        if (id != null && id != Guid.Empty)
            throw new DomainException(Constants.VALIDATION_TASK_ID_NOT_EMPTY);

        if (createdAt != null && createdAt.HasValue)
            throw new DomainException(Constants.VALIDATION_TASK_CREATED_AT_NOT_EMPTY);

    }

    public void ValidateUpdate(IDateTimeProvider? dateTimeProvider = null)
    {
        BasicValidation(Title, Description, Deadline, CreatedAt, CompletedAt, dateTimeProvider);

        if (Id == Guid.Empty)
            throw new DomainException(Constants.VALIDATION_TASK_ID_EMPTY);
    }

    public void UpdateTask(string title, string description, DateTime? deadline, TaskItemStatus? newTaskItemStatus)
    {
        if (title != null && title != Title)
            Title = title.Trim();

        if (description != null && description != Description)
            Description = description.Trim();

        if (deadline.HasValue && deadline != Deadline)
            Deadline = deadline;

        UpdateTaskState(newTaskItemStatus);
    }

    public void ChangeStatus(TaskItemStatus newStatus)
    {
        TaskStateManager.ValidateStatusTransition(newStatus, State, Status);
        Status = newStatus;
    }

    public void AssignToUser(User user)
    {
        AssignedUserEmail = user.Id;
        AssignedUser = user;
    }

    private void UpdateTaskState(TaskItemStatus? newTaskItemStatus)
    {
        if (newTaskItemStatus != null && newTaskItemStatus != Status)
        {
            TaskStateManager.ApplyStateTransition(this, (TaskItemStatus)newTaskItemStatus, State, Status);
        }
    }

    private static void BasicValidation(string title,
                                         string description,
                                         DateTime? deadline,
                                         DateTime? createdAt,
                                         DateTime? completedAt,
                                         IDateTimeProvider? dateTimeProvider = null)
    {
        dateTimeProvider ??= new DateTimeProvider();

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(Constants.VALIDATION_TASK_TITLE_NOT_EMPTY);

        if (title.Length > 100)
            throw new DomainException(Constants.VALIDATION_TASK_TITLE_LENGTH);

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(Constants.VALIDATION_TASK_DESCRIPTION_NOT_EMPTY);

        if (deadline != null && deadline.HasValue && deadline.Value < dateTimeProvider.GetUTCNow())
            throw new DomainException(Constants.VALIDATION_TASK_DEADLINE_NOT_PAST);

        if (completedAt != null && completedAt.HasValue)
            throw new DomainException(Constants.VALIDATION_TASK_COMPLETE_AT_NOT_EMPTY);
    }
}
