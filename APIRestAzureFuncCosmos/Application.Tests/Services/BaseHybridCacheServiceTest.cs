using Application.Cache.Interfaces;
using Application.DTOs;
using Application.Task.Mappers;
using Application.Task.Services;
using Domain.Entities;
using Domain.Task.Enums;
using Domain.Task.Interfaces;
using Domain.User.Interfaces;
using FluentValidation;
using Moq;
using Shared.Interfaces;

namespace Application.Tests.Services;

public class BaseHybridCacheServiceTest
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IHybridCacheService> _cacheServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly TaskService _taskService;

    public BaseHybridCacheServiceTest()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _cacheServiceMock = new Mock<IHybridCacheService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            _userRepositoryMock.Object,
            _cacheServiceMock.Object,
            _dateTimeProviderMock.Object
        );
    }

    [Fact]
    public async Task ClearAllRequestFromCacheAsync_ShouldRemoveCacheKey()
    {
        //Arrange
        var dateTime = DateTime.UtcNow;
        var taskDto = new TaskDTO(Guid.NewGuid(), "Test Task", "Test Description", TaskEntityStatus.Pending, dateTime, dateTime, dateTime, null);
        var taskEntity = TaskMapper.ToEntity(taskDto, _dateTimeProviderMock.Object);

        _taskRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<TaskEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskEntity);

        //Act
        await _taskService.CreateAsync(taskDto, CancellationToken.None);

        //Assert
        _cacheServiceMock.Verify(cache => cache.RemoveAsync("task:all"), Times.Once);
    }
}