using Application.DTOs;
using Application.Interfaces;
using Application.Mappers;
using Domain.Entities;
using FluentResults;
using FluentValidation;
using Shared.Consts;
using Shared.Exceptions;
using Shared.Interfaces;

namespace Application.Services;

public class TaskService(ITaskRepository taskRepository,
                         IUserRepository userRepository,
                         IValidator<TaskDTO> createValidator,
                         IValidator<TaskItem> updateValidator,
                         IHybridCacheService hybridCacheService,
                         IDateTimeProvider dateTimeProvider) : BaseHybridCacheService, ITaskService
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IHybridCacheService _cacheService = hybridCacheService;
    private readonly IValidator<TaskDTO> _createValidator = createValidator;
    private readonly IValidator<TaskItem> _updateValidator = updateValidator;    
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    protected override string CacheKey { get => "task:"; }

    public async Task<List<TaskDTO>> GetAllAsync(CancellationToken cancellationToken)
    {
        var cachekey = $"{CacheKey}{BASE_CACHEKEY_ALL}";
        return await _cacheService
            .GetOrSetAsync(cachekey, async () => 
                (await _taskRepository.GetAllAsync(cancellationToken))
                        .Select(TaskMapper.ToDTO)
                        .ToList()
            ) ?? [];
    }

    public async Task<TaskDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var cachekey = $"{CacheKey}{id}";
        var task = await _cacheService
            .GetOrSetAsync(cachekey, async () =>
                await _taskRepository.GetByIdAsync(id, cancellationToken)
            );

        if (task == null)
        {
            return null;
        }

        return TaskMapper.ToDTO(task);
    }

    public async Task<Result<TaskDTO>> CreateAsync(TaskDTO createTaskDto,
                                                   CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(createTaskDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(errors);
        }

        var taskEntity = TaskMapper.ToEntity(createTaskDto, _dateTimeProvider);
        var createdTask = await _taskRepository.AddAsync(taskEntity, cancellationToken);
        
        await ClearAllRequestFromCacheAsync(_cacheService);

        return Result.Ok(TaskMapper.ToDTO(createdTask));
    }

    public async Task<Result<TaskDTO?>> UpdateAsync(TaskDTO updateTaskDto, CancellationToken cancellationToken)
    {
        var existingTask = await _taskRepository.GetByIdAsync(updateTaskDto.Id, cancellationToken);
        if (existingTask == null)
        {
            return Result.Fail(new Error(Constants.VALIDATION_TASK_NOT_FOUND));
        }

        try
        {
            existingTask.UpdateTask(updateTaskDto.Title,
                                    updateTaskDto.Description,
                                    updateTaskDto.Deadline,
                                    updateTaskDto.Status);
        }
        catch (DomainException ex)
        {
            return Result.Fail(new Error(ex.Message));
        }

        var validationResult = await _updateValidator.ValidateAsync(existingTask, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(errors);
        }

        var updatedTask = await _taskRepository.UpdateAsync(existingTask, cancellationToken);
        if (updatedTask == null)
        {
            return Result.Fail(new Error(Constants.VALIDATION_TASK_NOT_FOUND));
        }

        await _cacheService.RemoveAsync($"{CacheKey}{updatedTask.Id}");
        await ClearAllRequestFromCacheAsync(_cacheService);

        return TaskMapper.ToDTO(updatedTask);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taskRepository.DeleteByIdAsync(id, cancellationToken);

        if (result)
        {
            await _cacheService.RemoveAsync($"{CacheKey}{id}");
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

        existingTask.AssignToUser(existingUser);

        await _taskRepository.UpdateAsync(existingTask, cancellationToken);
        await _cacheService.RemoveAsync($"{CacheKey}{taskId}");

        return Result.Ok();
    }
}