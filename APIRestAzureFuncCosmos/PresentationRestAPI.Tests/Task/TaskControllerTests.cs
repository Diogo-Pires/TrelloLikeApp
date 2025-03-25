using Application.Task.DTOs;
using Application.Task.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PresentationRestAPI.Task;

namespace PresentationRestAPI.Tests.Task;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _controller = new TasksController(_taskServiceMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllTasks_ReturnsOk_WithTaskList()
    {
        // Arrange
        var tasks = new List<TaskEntityDTO> { new TaskEntityDTO { Id = Guid.NewGuid(), Title = "Test Task" } };
        _taskServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetAllTasks(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTasks = Assert.IsType<List<TaskEntityDTO>>(okResult.Value);
        Assert.Single(returnedTasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _taskServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TaskEntityDTO)null);

        // Act
        var result = await _controller.GetTaskById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskById_ReturnsOk_WhenTaskExists()
    {
        // Arrange
        var task = new TaskEntityDTO { Id = Guid.NewGuid(), Title = "Existing Task" };
        _taskServiceMock.Setup(s => s.GetByIdAsync((Guid)task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        // Act
        var result = await _controller.GetTaskById((Guid)task.Id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTask = Assert.IsType<TaskEntityDTO>(okResult.Value);
        Assert.Equal(task.Id, returnedTask.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTask_ReturnsNoContent_WhenTaskDeleted()
    {
        // Arrange
        _taskServiceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTask(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _taskServiceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTask(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}