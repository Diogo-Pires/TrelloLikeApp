using Application.DTOs;
using Application.Mappers;
using Domain.Entities;
using FluentValidation;
using Shared.Exceptions;

namespace Application.Validators;

public class CreateTaskValidator : AbstractValidator<TaskDTO>
{
    public CreateTaskValidator()
    {
        RuleFor(t => t)
            .Custom((task, context) =>
            {
                try
                {
                    TaskItem.ValidateCreation(task.Id,
                                              task.Title,
                                              task.Description,
                                              task.Deadline,
                                              task.CreatedAt,
                                              task.CompletedAt);

                    if(task.User != null)
                    {
                        //User.
                    }
                }
                catch (DomainException ex)
                {
                    context.AddFailure(ex.Message);
                }
            });
    }
}