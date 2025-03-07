using System.Text;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Presentation.Interfaces;

namespace Presentation.Tests;

public class TaskFunctionTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<IExceptionHandler> _exceptionHandlerMock; 
    private readonly Mock<IValidator<TaskDTO>> _createValidatorMock;
    private readonly Mock<IValidator<TaskDTO>> _updateValidatorMock;
    private readonly TaskFunction _taskFunction;

    public TaskFunctionTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _exceptionHandlerMock = new Mock<IExceptionHandler>();
        _createValidatorMock = new Mock<IValidator<TaskDTO>>();
        _updateValidatorMock = new Mock<IValidator<TaskDTO>>();
        _taskFunction = new TaskFunction(_taskServiceMock.Object,
                                         _exceptionHandlerMock.Object,
                                         _createValidatorMock.Object,
                                         _updateValidatorMock.Object);
    }

    [Fact]
    public async Task GetAllTasks_ReturnsOk()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;
        var taskDto = new TaskDTO(Guid.NewGuid(), "Test Task", "Test Description", TaskItemStatus.Pending, dateTime, dateTime, dateTime, null);
        var tasks = new List<TaskDTO> { taskDto };
        _taskServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tasks);
        var req = new Mock<HttpRequest>();

        // Act
        var result = await _taskFunction.GetAllTasks(req.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(tasks, okResult.Value);
    }

    [Fact]
    public async Task GetTaskById_ReturnsTask_WhenFound()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;
        var taskId = Guid.NewGuid();
        var taskDto = new TaskDTO(taskId, "Test Task", "Test Description", TaskItemStatus.Pending, dateTime, dateTime, dateTime, null);
        _taskServiceMock
            .Setup(s => s.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskDto);
        var req = new Mock<HttpRequest>();

        // Act
        var result = await _taskFunction.GetTaskById(req.Object, taskId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(taskDto, okResult.Value);
    }

    [Fact]
    public async Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskServiceMock
            .Setup(s => s.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskDTO?)null);
        var req = new Mock<HttpRequest>();

        // Act
        var result = await _taskFunction.GetTaskById(req.Object, taskId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateTask_ReturnsCreated_WhenValid()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;
        var taskDto = new TaskDTO("Test Task", "Test Description", TaskItemStatus.Pending, null, null);
        var response = Result.Ok(taskDto);

        _taskServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<TaskDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var req = new Mock<HttpRequest>();
        var json = JsonConvert.SerializeObject(taskDto);
        req.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(json)));

        // Act
        var result = await _taskFunction.CreateTask(req.Object, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task UpdateTask_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;
        var taskDto = new TaskDTO(Guid.NewGuid(), "Test Task", "Test Description", TaskItemStatus.Pending, dateTime, dateTime, dateTime, null);
        var response = Result.Ok((TaskDTO?)taskDto);
        _taskServiceMock
            .Setup(s => s.UpdateAsync(It.IsAny<TaskDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var req = new Mock<HttpRequest>();
        var json = JsonConvert.SerializeObject(taskDto);
        req.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(json)));

        // Act
        var result = await _taskFunction.UpdateTask(req.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(taskDto, okResult.Value);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskServiceMock.Setup(s => s.DeleteAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var req = new Mock<HttpRequest>();

        // Act
        var result = await _taskFunction.DeleteTask(req.Object, taskId, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskServiceMock.Setup(s => s.DeleteAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var req = new Mock<HttpRequest>();

        // Act
        var result = await _taskFunction.DeleteTask(req.Object, taskId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}