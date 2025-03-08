using Application.User.DTOs;
using FluentValidation;
using PresentationRestAPI.User.Interfaces;
using Shared.Consts;

namespace PresentationRestAPI.User.Validators;

public class UserCreateValidator : AbstractValidator<UserEntityDTO>, IUserCreatorValidator
{
    public UserCreateValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .WithMessage(Constants.VALIDATION_INVALID_JSON_REQUEST);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Constants.VALIDATION_USER_NAME_NOT_EMPTY);

        RuleFor(x => x.Id)
            .Matches(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")
            .WithMessage(Constants.VALIDATION_USER_EMAIL_NOT_VALID)
            .NotEmpty()
            .WithMessage(Constants.VALIDATION_USER_EMAIL_NOT_EMPTY);
    }
}