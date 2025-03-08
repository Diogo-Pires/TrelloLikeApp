using Application.Task.DTOs;
using Application.User.DTOs;
using FluentValidation;

namespace PresentationRestAPI.User.Interfaces;

public interface IUserCreatorValidator : IValidator<UserEntityDTO> { }
