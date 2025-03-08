using Domain.Task.Enums;
using Domain.TaskEntity;
using Infrastructure.Config;
using Infrastructure.Task;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Moq;
using Shared.Interfaces;
using System.Net;

namespace Infrastructure.Tests.Persistence;

public class TaskRepositoryTests
{
    private readonly Mock<CosmosClient> _cosmosClientMock;
    private readonly Mock<Container> _containerMock;
    private readonly TaskRepository _taskRepository;

    public TaskRepositoryTests()
    {
        _cosmosClientMock = new Mock<CosmosClient>();
        _containerMock = new Mock<Container>();

        var cosmosDbSettings = new CosmosDbSettings { DatabaseName = "TestDB", TaskContainerName = "Tasks" };

        _cosmosClientMock
            .Setup(client => client.GetContainer(cosmosDbSettings.DatabaseName, cosmosDbSettings.TaskContainerName))
            .Returns(_containerMock.Object);

        _taskRepository = new TaskRepository(_cosmosClientMock.Object, cosmosDbSettings);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTask_WhenExists()
    {
        //Arrange
        var datetime = new DateTime(2025, 1, 1);
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock
            .Setup(m => m.GetUTCNow())
            .Returns(datetime);

        var taskId = Guid.NewGuid().ToString();
        var task = new TaskEntity("Test Task", "Description", datetime.AddDays(1), TaskEntityStatus.Pending, null, dateTimeProviderMock.Object);

        _containerMock
            .Setup(c => c.ReadItemAsync<TaskEntity>(taskId, new PartitionKey(taskId), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<TaskEntity>>(r => r.Resource == task));

        var result = await _taskRepository.GetByIdAsync(Guid.Parse(taskId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(task.Title, result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        //Arrange
        var taskId = Guid.NewGuid().ToString();

        _containerMock
            .Setup(c => c.ReadItemAsync<TaskEntity>(taskId, new PartitionKey(taskId), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, string.Empty, 0));

        //Act
        var result = await _taskRepository.GetByIdAsync(Guid.Parse(taskId), CancellationToken.None);

        //Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnTrue_WhenItemExists()
    {
        //Arrange
        var taskId = Guid.NewGuid().ToString();

        _containerMock
            .Setup(c => c.DeleteItemAsync<TaskEntity>(taskId, new PartitionKey(taskId), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<TaskEntity>>(r => r.StatusCode == HttpStatusCode.NoContent));

        //Act
        var result = await _taskRepository.DeleteByIdAsync(Guid.Parse(taskId), CancellationToken.None);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        //Arrange
        var taskId = Guid.NewGuid().ToString();

        _containerMock
            .Setup(c => c.DeleteItemAsync<TaskEntity>(taskId, new PartitionKey(taskId), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, string.Empty, 0));

        //Act
        var result = await _taskRepository.DeleteByIdAsync(Guid.Parse(taskId), CancellationToken.None);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnTask_WhenSuccessful()
    {
        //Arrange
        var datetime = new DateTime(2025, 1, 1);
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock
            .Setup(m => m.GetUTCNow())
            .Returns(datetime);

        var task = new TaskEntity("Test Task", "Description", datetime.AddDays(1), TaskEntityStatus.Pending, null, dateTimeProviderMock.Object);

        _containerMock
            .Setup(c => c.CreateItemAsync(task, new PartitionKey(task.Id.ToString()), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<TaskEntity>>(r => r.Resource == task));

        //Act
        var result = await _taskRepository.AddAsync(task, CancellationToken.None);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(task.Title, result.Title);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnTask_WhenSuccessful()
    {
        //Arrange
        var datetime = new DateTime(2025, 1, 1);
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock
            .Setup(m => m.GetUTCNow())
            .Returns(datetime);

        var task = new TaskEntity("Test Task", "Description", datetime.AddDays(1), TaskEntityStatus.Pending, null, dateTimeProviderMock.Object);

        _containerMock
            .Setup(c => c.UpsertItemAsync(task, new PartitionKey(task.Id.ToString()), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<TaskEntity>>(r => r.StatusCode == HttpStatusCode.OK));

        //Act
        var result = await _taskRepository.UpdateAsync(task, CancellationToken.None);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(task.Title, result.Title);
    }
}
