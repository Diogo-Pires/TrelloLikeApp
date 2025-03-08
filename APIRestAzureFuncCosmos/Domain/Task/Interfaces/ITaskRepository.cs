namespace Domain.Task.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<TaskEntity?> GetByIdAsync(Guid? id, CancellationToken cancellationToken);
    Task<TaskEntity> AddAsync(TaskEntity task, CancellationToken cancellationToken);
    Task<TaskEntity?> UpdateAsync(TaskEntity task, CancellationToken cancellationToken);
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken);
}