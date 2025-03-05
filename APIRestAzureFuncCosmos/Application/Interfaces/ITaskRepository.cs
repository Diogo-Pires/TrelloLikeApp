using Domain.Entities;

namespace Application.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<TaskItem?> GetByIdAsync(Guid? id, CancellationToken cancellationToken);
    Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken);
    Task<TaskItem?> UpdateAsync(TaskItem task, CancellationToken cancellationToken);
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken);
}