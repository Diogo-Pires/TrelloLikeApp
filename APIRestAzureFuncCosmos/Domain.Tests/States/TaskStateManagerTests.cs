using Domain.Entities;
using Domain.Task.Enums;
using Domain.Task.States;
using Moq;
using Shared.Consts;
using Shared.Exceptions;
using Shared.Interfaces;

namespace Domain.Tests.States;

public class TaskStateManagerTests
{
    private readonly Mock<IDateTimeProvider> _dateTimeMockService;

    public TaskStateManagerTests()
    {
        _dateTimeMockService = new Mock<IDateTimeProvider>();
    }

    [Theory]
    [InlineData(TaskEntityStatus.Pending, typeof(PendingState))]
    [InlineData(TaskEntityStatus.InProgress, typeof(InProgressState))]
    [InlineData(TaskEntityStatus.Completed, typeof(CompletedState))]
    [InlineData(TaskEntityStatus.Cancelled, typeof(CancelledState))]
    public void GetState_ShouldReturnCorrectState(TaskEntityStatus status, Type expectedType)
    {
        // Act
        var state = TaskStateManager.GetState(status);

        // Assert
        Assert.IsType(expectedType, state);
    }

    [Fact]
    public void GetState_ShouldThrowExceptionForInvalidStatus()
    {
        // Arrange
        var invalidStatus = (TaskEntityStatus)999;

        // Act & Assert
        Assert.Throws<DomainException>(() => TaskStateManager.GetState(invalidStatus));
    }

    [Fact]
    public void ValidateStatusTransition_ShouldThrowExceptionForInvalidTransition()
    {
        // Arrange
        var currentState = new PendingState();
        var invalidStatus = TaskEntityStatus.Completed;

        // Act & Assert
        Assert.Throws<DomainException>(() => TaskStateManager.ValidateStatusTransition(invalidStatus, currentState, TaskEntityStatus.Pending));
    }

    [Fact]
    public void ValidateStatusTransition_ShouldNotThrowForValidTransition()
    {
        // Arrange
        var currentState = new InProgressState();
        var validStatus = TaskEntityStatus.Completed;

        // Act & Assert
        var exception = Record.Exception(() => TaskStateManager.ValidateStatusTransition(validStatus, currentState, TaskEntityStatus.InProgress));
        Assert.Null(exception);
    }

    [Fact]
    public void ApplyStateTransition_ShouldThrowExceptionForInvalidTransition()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var task = new TaskEntity("Test",
                                "Description",
                                null,
                                TaskEntityStatus.Pending,
                                null,
                                _dateTimeMockService.Object);
        var invalidStatus = TaskEntityStatus.Completed;

        // Act & Assert
        Assert.Throws<DomainException>(() => TaskStateManager.ApplyStateTransition(task, invalidStatus, task.State, task.Status));
    }

    [Fact]
    public void ApplyStateTransition_ShouldInvokeCorrectStateMethod()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var task = new TaskEntity("Test",
                                "Description",
                                null,
                                TaskEntityStatus.Pending,
                                null,
                                _dateTimeMockService.Object);
        var newStatus = TaskEntityStatus.InProgress;

        // Act
        TaskStateManager.ApplyStateTransition(task, newStatus, task.State, task.Status);

        // Assert
        Assert.Equal(newStatus, task.Status);
    }

    [Theory]
    [InlineData(TaskEntityStatus.InProgress, true)]
    [InlineData(TaskEntityStatus.Cancelled, true)]
    [InlineData(TaskEntityStatus.Completed, false)]
    public void PendingCanTransitionTo_ShouldReturnCorrectResult(TaskEntityStatus newStatus, bool expectedResult)
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var state = new PendingState();
        var task = new TaskEntity("Test Task",
                                "Test Description",
                                null,
                                TaskEntityStatus.Pending,
                                null,
                                _dateTimeMockService.Object);

        // Act
        var result = state.CanTransitionTo(newStatus);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(TaskEntityStatus.Pending, false)]
    [InlineData(TaskEntityStatus.Cancelled, true)]
    [InlineData(TaskEntityStatus.Completed, true)]
    public void InProgressCanTransitionTo_ShouldReturnCorrectResult(TaskEntityStatus newStatus, bool expectedResult)
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var state = new InProgressState();
        var task = new TaskEntity("Test Task",
                                "Test Description",
                                null,
                                TaskEntityStatus.Pending,
                                null,
                                _dateTimeMockService.Object);

        // Act
        var result = state.CanTransitionTo(newStatus);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void Start_ShouldChangeStatusToInProgress()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var state = new PendingState();
        var task = new TaskEntity("Test Task",
                                "Test Description",
                                null,
                                TaskEntityStatus.Pending,
                                null,
                                _dateTimeMockService.Object);

        // Act
        state.Start(task);

        // Assert
        Assert.Equal(TaskEntityStatus.InProgress, task.Status);
    }

    [Fact]
    public void Complete_ShouldThrowException()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var state = new PendingState();
        var task = new TaskEntity("Test Task",
                                "Test Description",
                                null,
                                TaskEntityStatus.Pending,
                                null,
                                _dateTimeMockService.Object);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => state.Complete(task));
        Assert.Equal(Constants.VALIDATION_TASK_MUST_BE_STARTED, exception.Message);
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var state = new PendingState();
        var task = new TaskEntity("Test Task",
                                "Test Description",
                                null,
                                TaskEntityStatus.Pending,
                                null,
                                _dateTimeMockService.Object);

        // Act
        state.Cancel(task);

        // Assert
        Assert.Equal(TaskEntityStatus.Cancelled, task.Status);
    }
}

