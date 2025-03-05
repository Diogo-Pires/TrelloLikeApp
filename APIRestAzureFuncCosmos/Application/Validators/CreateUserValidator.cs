using Application.DTOs;
using FluentValidation;
using Shared.Consts;

namespace Application.Validators;

public class CreateUserValidator : AbstractValidator<UserDTO>
{
    public CreateUserValidator()
    {
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