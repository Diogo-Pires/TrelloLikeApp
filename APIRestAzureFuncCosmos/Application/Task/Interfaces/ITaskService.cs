using Application.Task.DTOs;
using FluentResults;

namespace Application.Task.Interfaces;
public interface ITaskService
{
    Task<List<TaskEntityDTO>> GetAllAsync(CancellationToken cancellationToken);
    Task<TaskEntityDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<TaskEntityDTO>> CreateAsync(TaskEntityDTO taskDto, CancellationToken cancellationToken);
    Task<Result<TaskEntityDTO?>> UpdateAsync(TaskEntityDTO taskDto, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> AssignTaskToUserAsync(Guid taskId, string email, CancellationToken cancellationToken);
    Task<List<TaskEntityDTO>> GetAssignedToAnUserAsync(string email, CancellationToken cancellationToken);
}