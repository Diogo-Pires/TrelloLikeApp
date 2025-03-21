using Application.Task.DTOs;
using Application.Task.Interfaces;
using Application.Task.Mappers;
using Domain;
using Domain.Task;
using Domain.Task.Interfaces;
using Domain.User.Interfaces;
using FluentResults;
using Infrastructure.Cache;
using Infrastructure.Cache.Interfaces;
using Shared.Consts;
using Shared.Interfaces;

namespace Application.Task.Services;

public class TaskService(ITaskRepository taskRepository,
                         IUserRepository userRepository,
                         IHybridCacheService hybridCacheService,
                         IDateTimeProvider dateTimeProvider) : BaseHybridCacheService, ITaskService
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IHybridCacheService _cacheService = hybridCacheService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public override string CacheKey { get => "tasks:"; }

    public async Task<List<TaskEntityDTO>> GetAllAsync(CancellationToken cancellationToken)
    {
        var cachekey = $"{CacheKey}{BASE_CACHEKEY_ALL}";
        return await _cacheService
            .GetOrSetAsync(cachekey, CacheKey, async () =>
                (await _taskRepository.GetAllAsync(cancellationToken))
                        .Select(TaskMapper.ToDTO)
                        .ToList()
            ) ?? [];
    }

    public async Task<TaskEntityDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var cachekey = $"{CacheKey}{id}";
        var task = await _cacheService
            .GetOrSetAsync(cachekey, CacheKey, async () =>
                await _taskRepository.GetByIdAsync(id, cancellationToken)
            );

        if (task == null)
        {
            return null;
        }

        return TaskMapper.ToDTO(task);
    }

    public async Task<List<TaskEntityDTO>> GetAssignedToAnUserAsync(string email, CancellationToken cancellationToken)
    {
        var cachekey = $"{CacheKey}{email}:{BASE_CACHEKEY_ALL}";
        return await _cacheService
            .GetOrSetAsync(cachekey, CacheKey, async () =>
                (await _taskRepository.GetAssignedToAnUserAsync(email, cancellationToken))
                        .Select(TaskMapper.ToDTO)
                        .ToList()
            ) ?? [];
    }

    public async Task<Result<TaskEntityDTO>> CreateAsync(TaskEntityDTO createTaskDto,
                                                         CancellationToken cancellationToken)
    {
        var taskEntity = TaskMapper.ToEntity(createTaskDto, _dateTimeProvider);
        var createdTask = await _taskRepository.AddAsync(taskEntity, cancellationToken);

        await _cacheService.SetIfNotExistsAsync($"{CacheKey}{createdTask.Id}", CacheKey, createdTask);
        await ClearAllRequestFromCacheAsync(_cacheService);

        return Result.Ok(TaskMapper.ToDTO(createdTask));
    }

    public async Task<Result<TaskEntityDTO?>> UpdateAsync(TaskEntityDTO updateTaskDto,
                                                          CancellationToken cancellationToken)
    {
        var existingTask = await _taskRepository.GetByIdAsync(updateTaskDto.Id, cancellationToken);
        if (existingTask == null)
        {
            return Result.Fail(new Error(Constants.VALIDATION_TASK_NOT_FOUND));
        }

        TaskEntity? updatedTask;
        try
        {
            existingTask.UpdateTask(updateTaskDto.Title,
                                    updateTaskDto.Description,
                                    updateTaskDto.Deadline,
                                    updateTaskDto.Status);

            updatedTask = await _taskRepository.UpdateAsync(existingTask, cancellationToken);
            if (updatedTask == null)
            {
                return Result.Fail(new Error(Constants.VALIDATION_TASK_NOT_FOUND));
            }
        }
        catch (DomainException ex)
        {
            return Result.Fail(new Error(ex.Message));
        }

        await _cacheService.RemoveAsync($"{CacheKey}{updatedTask.Id}", CacheKey);
        await _cacheService.SetIfNotExistsAsync($"{CacheKey}{updatedTask.Id}", CacheKey, updatedTask);

        await ClearAllRequestFromCacheAsync(_cacheService);

        return TaskMapper.ToDTO(updatedTask);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taskRepository.DeleteByIdAsync(id, cancellationToken);

        if (result)
        {
            await _cacheService.RemoveAsync($"{CacheKey}{id}", CacheKey);
            await ClearAllRequestFromCacheAsync(_cacheService);
        }

        return result;
    }

    public async Task<Result> AssignTaskToUserAsync(Guid taskId, string email, CancellationToken cancellationToken)
    {
        var existingTask = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (existingTask == null)
        {
            return Result.Fail(new Error(Constants.VALIDATION_TASK_NOT_FOUND));
        }

        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser == null)
        {
            return Result.Fail(new Error(Constants.VALIDATION_USER_NOT_FOUND));
        }

        try
        {
            existingTask.AssignToUser(existingUser);
            await _taskRepository.UpdateAsync(existingTask, cancellationToken);
        }
        catch (DomainException ex)
        {
            return Result.Fail(new Error(ex.Message));
        }

        await _cacheService.RemoveAsync($"{CacheKey}{taskId}", CacheKey);
        await _cacheService.SetIfNotExistsAsync($"{CacheKey}{existingTask.Id}", CacheKey, existingTask);
        await ClearAllRequestFromCacheAsync(_cacheService);

        return Result.Ok();
    }

    public async System.Threading.Tasks.Task DeleteAllCacheAsync(CancellationToken cancellationToken) =>
        await _cacheService.IncrementVersion(CacheKey);
}