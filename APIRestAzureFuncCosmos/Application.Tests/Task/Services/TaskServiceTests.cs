using Application.Task.DTOs;
using Application.Task.Mappers;
using Application.Task.Services;
using Domain.Task;
using Domain.Task.Interfaces;
using Domain.User.Interfaces;
using Domain.User;
using Infrastructure.Cache.Interfaces;
using MediatR;
using Moq;
using Shared.Interfaces;
using Domain.Task.Enums;

namespace Application.Tests.Task.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IHybridCacheService> _cacheServiceMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _cacheServiceMock = new Mock<IHybridCacheService>();
        _mediatorMock = new Mock<IMediator>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            _userRepositoryMock.Object,
            _cacheServiceMock.Object,
            _mediatorMock.Object,
            _dateTimeProviderMock.Object
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_Should_Return_Cached_Or_Repository_Data()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new("Task 1", "Description 1", DateTime.Now.AddDays(1), TaskEntityStatus.Pending, "user@example.com", _dateTimeProviderMock.Object),
            new("Task 2", "Description 2", DateTime.Now.AddDays(2), TaskEntityStatus.Pending, "user@example.com", _dateTimeProviderMock.Object)
        };

        var taskDtos = tasks.Select(TaskMapper.ToDTO).ToList();
        var cacheKey = "tasks:" + TaskService.BASE_CACHEKEY_ALL;

        _cacheServiceMock
             .Setup(c => c.GetOrSetAsync(
                 It.IsAny<string>(),
                 It.IsAny<string>(),
                 It.IsAny<Func<Task<List<TaskEntityDTO>?>>>()
             ))
             .ReturnsAsync(taskDtos);

        // Act
        var result = await _taskService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        _cacheServiceMock.Verify(c => c.GetOrSetAsync(cacheKey, "tasks:", It.IsAny<Func<Task<List<TaskEntityDTO>?>>>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_Should_Return_Correct_Task()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskEntity("Task 1", "Description 1", DateTime.Now.AddDays(1), TaskEntityStatus.Pending, "user@example.com", _dateTimeProviderMock.Object);
        var taskDto = TaskMapper.ToDTO(task);
        var cacheKey = $"tasks:{taskId}";

        _cacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(), 
                It.IsAny<Func<Task<TaskEntity?>>>()
            ))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.GetByIdAsync(taskId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskDto.Id, result?.Id);
        _cacheServiceMock.Verify(c => c.GetOrSetAsync(cacheKey, "tasks:", It.IsAny<Func<Task<TaskEntity?>>>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_Should_Create_Task_And_Update_Cache()
    {
        // Arrange
        var taskDto = new TaskEntityDTO
        {
            Title = "New Task",
            Description = "Description for new task",
            Deadline = DateTime.Now.AddDays(3),
            Status = TaskEntityStatus.Pending,
            AssignedUserEmail = "user@example.com"
        };

        var taskEntity = TaskMapper.ToEntity(taskDto, _dateTimeProviderMock.Object);
        var createdTask = new TaskEntity("New Task", "Description for new task", DateTime.Now.AddDays(3), TaskEntityStatus.Pending, "user@example.com", _dateTimeProviderMock.Object);

        _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TaskEntity>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdTask);

        // Act
        var result = await _taskService.CreateAsync(taskDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.SetIfNotExistsAsync(It.IsAny<string>(), "tasks:", createdTask), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_Should_Remove_Task_From_Cache()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepositoryMock.Setup(r => r.DeleteByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _taskService.DeleteAsync(taskId, CancellationToken.None);

        // Assert
        Assert.True(result);
        _cacheServiceMock.Verify(c => c.RemoveAsync($"tasks:{taskId}", "tasks:"), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task AssignTaskToUserAsync_Should_Validate_And_Assign_Task()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var email = "user@example.com";
        var task = new TaskEntity("Task 1", "Description", DateTime.Now.AddDays(1), TaskEntityStatus.Pending, email, _dateTimeProviderMock.Object);
        var user = new UserEntity("name", email, "googleid");

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TaskEntity>(), It.IsAny<CancellationToken>())).ReturnsAsync(task);

        // Act
        var result = await _taskService.AssignTaskToUserAsync(taskId, email, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mediatorMock.Verify(m => m.Send(It.IsAny<Application.Task.Commands.TaskAssignedNotificationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
