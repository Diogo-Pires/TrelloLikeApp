using Application.Task.DTOs;
using FluentValidation;
using Shared.Consts;
using Shared.Interfaces;

namespace PresentationRestAPI.Validators;

public class UpdateTaskValidator : AbstractValidator<TaskEntityDTO>
{
    public UpdateTaskValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(t => t)
            .NotNull()
            .WithMessage(Constants.VALIDATION_INVALID_JSON_REQUEST);

        RuleFor(t => t.User)
            .Empty()
            .WithMessage(Constants.VALIDATION_TASK_USER_CREATION);

        RuleFor(t => t.CompletedAt)
            .Must((task, completeAt) => completeAt > task.CreatedAt)
            .WithMessage(Constants.VALIDATION_TASK_CREATEAT_COMPLETEAT)
            .When(t => t.CompletedAt.HasValue);

        RuleFor(t => t.CreatedAt)
            .Empty()
            .WithMessage(Constants.VALIDATION_TASK_CREATED_AT_NOT_EMPTY);

        RuleFor(t => t.Id)
            .NotEmpty()
            .WithMessage(Constants.VALIDATION_TASK_ID_EMPTY);

        RuleFor(t => t.Status)
            .IsInEnum()
            .When(t => t.Status.HasValue);

        RuleFor(t => t.Deadline)
            .Must(deadline => deadline > dateTimeProvider.GetUTCNow())
            .WithMessage(Constants.VALIDATION_TASK_DEADLINE_NOT_PAST)
            .When(t => t.Deadline.HasValue);

        RuleFor(t => t.Title)
            .MaximumLength(100)
            .WithMessage(Constants.VALIDATION_TASK_TITLE_LENGTH)
            .When(t => t.Title != null);
    }
}