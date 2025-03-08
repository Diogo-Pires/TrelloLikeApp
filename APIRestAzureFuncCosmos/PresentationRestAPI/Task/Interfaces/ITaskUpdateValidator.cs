using Application.Task.DTOs;
using FluentValidation;

namespace PresentationRestAPI.Task.Interfaces;

public interface ITaskUpdateValidator : IValidator<TaskEntityDTO> { }