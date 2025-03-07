namespace Shared.Consts;

public struct Constants
{
    public const string VALIDATION_INVALID_JSON_REQUEST = "Invalid JSON request.";
    public const string VALIDATION_TASK_ID_NOT_EMPTY = "Task's id cannot by provided during creation.";
    public const string VALIDATION_TASK_COMPLETE_AT_NOT_EMPTY = "Task's completeAt cannot by provided during creation.";
    public const string VALIDATION_TASK_CREATED_AT_NOT_EMPTY = "Task's createdAt cannot by provided during creation/update.";
    public const string VALIDATION_TASK_TITLE_NOT_EMPTY = "Task's title cannot be empty.";
    public const string VALIDATION_TASK_TITLE_LENGTH = "Task's title cannot exceed 100 characters.";
    public const string VALIDATION_TASK_DESCRIPTION_NOT_EMPTY = "Task's description cannot be empty.";
    public const string VALIDATION_TASK_DEADLINE_NOT_PAST = "Task's deadline cannot be in the past.";
    public const string VALIDATION_TASK_CANNOT_BEFORE_CREATEAT = "Deadline cannot be before createAt date.";
    public const string VALIDATION_TASK_CANNOT_START_CANCELLED = "Cannot start a cancelled task.";
    public const string VALIDATION_TASK_CANNOT_COMPLETE_CANCELLED = "Cannot complete a cancelled task.";
    public const string VALIDATION_TASK_ALREADY_CANCELLED = "Task is already cancelled.";
    public const string VALIDATION_TASK_RESTART_COMPLETED_TASK = "Cannot restart a completed task.";
    public const string VALIDATION_TASK_ALREADY_COMPLETED = "Task is already completed.";
    public const string VALIDATION_TASK_CANNOT_CANCEL_COMPLETE = "Cannot cancel a completed task.";
    public const string VALIDATION_TASK_ALREADY_PROGRESS = "Task is already in progress.";
    public const string VALIDATION_TASK_MUST_BE_STARTED = "Task cannot be completed before being started.";
    public const string VALIDATION_TASK_INVALID_STATUS = "Invalid task status.";
    public const string VALIDATION_TASK_INVALID_STATUS_TRANSITION = "Invalid status transition.";
    public const string VALIDATION_TASK_USER_CREATION = "An user cannot be provided during task's create/update";
    public const string VALIDATION_TASK_CREATEAT_COMPLETEAT = "CompleteAt cannot be greater than createAt";
    public const string VALIDATION_USER_NAME_NOT_EMPTY = "User's name cannot be empty.";
    public const string VALIDATION_USER_EMAIL_NOT_EMPTY = "User's email cannot be empty.";
    public const string VALIDATION_USER_EMAIL_NOT_VALID = "User's email is not valid.";
    public const string VALIDATION_TASK_ID_EMPTY = "Task's id must be not empty";
    public const string VALIDATION_TASK_NOT_FOUND = "Task was not found.";
    public const string VALIDATION_USER_NOT_FOUND = "User was not found.";
}