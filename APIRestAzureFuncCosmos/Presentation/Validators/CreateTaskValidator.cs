using Application.Task.DTOs;
using FluentValidation;
using Shared.Consts;
using Shared.Interfaces;

namespace Presentation.Validators;

public class CreateTaskValidator : AbstractValidator<TaskEntityDTO>
{
    public CreateTaskValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(t => t)
            .NotNull()
            .WithMessage(Constants.VALIDATION_INVALID_JSON_REQUEST);

        RuleFor(t => t.User)
            .Empty()
            .WithMessage(Constants.VALIDATION_TASK_USER_CREATION);

        RuleFor(t => t.CompletedAt)
            .Empty()
            .WithMessage(Constants.VALIDATION_TASK_COMPLETE_AT_NOT_EMPTY);

        RuleFor(t => t.CreatedAt)
            .Empty()
            .WithMessage(Constants.VALIDATION_TASK_CREATED_AT_NOT_EMPTY);

        RuleFor(t => t.Id)
            .Empty()
            .WithMessage(Constants.VALIDATION_TASK_ID_NOT_EMPTY);

        RuleFor(t => t.Status)
            .IsInEnum()
            .When(t => t.Status.HasValue);

        RuleFor(t => t.Deadline)
            .Must(deadline => deadline > dateTimeProvider.GetUTCNow())
            .WithMessage(Constants.VALIDATION_TASK_DEADLINE_NOT_PAST)
            .When(t => t.Deadline.HasValue);

        RuleFor(t => t.Title)
            .NotEmpty()
            .WithMessage(Constants.VALIDATION_TASK_TITLE_NOT_EMPTY)
            .MaximumLength(100)
            .WithMessage(Constants.VALIDATION_TASK_TITLE_LENGTH);

        RuleFor(t => t.Description)
            .NotEmpty()
            .WithMessage(Constants.VALIDATION_TASK_DESCRIPTION_NOT_EMPTY);
    }
}