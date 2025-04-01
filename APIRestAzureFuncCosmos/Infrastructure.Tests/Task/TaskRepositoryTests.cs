using Moq;
using System.Net;
using Domain.Task;
using Infrastructure.Config;
using Infrastructure.Task;
using Microsoft.Azure.Cosmos;
using FluentAssertions;
using Shared.Interfaces;

namespace Infrastructure.Tests.Task;

public class TaskRepositoryTests
{
    private readonly Mock<Container> _containerMock;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        var cosmosClientMock = new Mock<CosmosClient>();
        _containerMock = new Mock<Container>();

        var dbSettings = new CosmosDbSettings
        {
            DatabaseName = "TestDb",
            TaskContainerName = "Tasks"
        };

        cosmosClientMock
            .Setup(c => c.GetContainer(dbSettings.DatabaseName, dbSettings.TaskContainerName))
            .Returns(_containerMock.Object);

        _repository = new TaskRepository(cosmosClientMock.Object, dbSettings);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_Should_Return_Task_If_Found()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var datetime = DateTime.UtcNow;
        var datetimeProviderMock = new Mock<IDateTimeProvider>();

        var task = new TaskEntity("test", "test", datetime.AddDays(1), Domain.Task.Enums.TaskEntityStatus.Pending, null, datetimeProviderMock.Object);
        var responseMock = CreateItemResponseMock(task, HttpStatusCode.OK);

        _containerMock.Setup(c => c.ReadItemAsync<TaskEntity>(taskId.ToString(), new PartitionKey(taskId.ToString()), null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMock.Object);

        // Act
        var result = await _repository.GetByIdAsync(taskId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(task);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_Should_Return_Null_If_Not_Found()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _containerMock.Setup(c => c.ReadItemAsync<TaskEntity>(taskId.ToString(), new PartitionKey(taskId.ToString()), null, It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _repository.GetByIdAsync(taskId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_Should_Add_Task()
    {
        // Arrange
        var datetime = DateTime.UtcNow;
        var datetimeProviderMock = new Mock<IDateTimeProvider>();

        var task = new TaskEntity("test", "test", datetime.AddDays(1), Domain.Task.Enums.TaskEntityStatus.Pending, null, datetimeProviderMock.Object);
        var responseMock = CreateItemResponseMock(task, HttpStatusCode.Created);

        _containerMock.Setup(c => c.CreateItemAsync(task, new PartitionKey(task.Id.ToString()), null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMock.Object);

        // Act
        var result = await _repository.AddAsync(task, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(task);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAsync_Should_Update_Task()
    {
        // Arrange
        var datetime = DateTime.UtcNow;
        var datetimeProviderMock = new Mock<IDateTimeProvider>();

        var task = new TaskEntity("test", "test", datetime.AddDays(1), Domain.Task.Enums.TaskEntityStatus.Pending, null, datetimeProviderMock.Object);
        var responseMock = CreateItemResponseMock(task, HttpStatusCode.OK);

        _containerMock.Setup(c => c.ReplaceItemAsync(task, task.Id.ToString(), new PartitionKey(task.Id.ToString()), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMock.Object);

        // Act
        var result = await _repository.UpdateAsync(task, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(task);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteByIdAsync_Should_Return_True_When_Success()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var responseMock = CreateItemResponseMock<object>(null, HttpStatusCode.NoContent);

        _containerMock.Setup(c => c.DeleteItemAsync<object>(taskId.ToString(), new PartitionKey(taskId.ToString()), null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMock.Object);

        // Act
        var result = await _repository.DeleteByIdAsync(taskId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteByIdAsync_Should_Return_False_When_Not_Found()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _containerMock.Setup(c => c.DeleteItemAsync<object>(taskId.ToString(), new PartitionKey(taskId.ToString()), null, It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _repository.DeleteByIdAsync(taskId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    // Mock helper methods
    private static Mock<ItemResponse<T>> CreateItemResponseMock<T>(T resource, HttpStatusCode statusCode)
    {
        var responseMock = new Mock<ItemResponse<T>>();
        responseMock.Setup(r => r.StatusCode).Returns(statusCode);
        responseMock.Setup(r => r.Resource).Returns(resource);
        return responseMock;
    }
}