using Domain.Entities;
using Domain.Enums;
using Domain.States;
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
    [InlineData(TaskItemStatus.Pending, typeof(PendingState))]
    [InlineData(TaskItemStatus.InProgress, typeof(InProgressState))]
    [InlineData(TaskItemStatus.Completed, typeof(CompletedState))]
    [InlineData(TaskItemStatus.Cancelled, typeof(CancelledState))]
    public void GetState_ShouldReturnCorrectState(TaskItemStatus status, Type expectedType)
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
        var invalidStatus = (TaskItemStatus)999;

        // Act & Assert
        Assert.Throws<DomainException>(() => TaskStateManager.GetState(invalidStatus));
    }

    [Fact]
    public void ValidateStatusTransition_ShouldThrowExceptionForInvalidTransition()
    {
        // Arrange
        var currentState = new PendingState();
        var invalidStatus = TaskItemStatus.Completed;

        // Act & Assert
        Assert.Throws<DomainException>(() => TaskStateManager.ValidateStatusTransition(invalidStatus, currentState, TaskItemStatus.Pending));
    }

    [Fact]
    public void ValidateStatusTransition_ShouldNotThrowForValidTransition()
    {
        // Arrange
        var currentState = new InProgressState();
        var validStatus = TaskItemStatus.Completed;

        // Act & Assert
        var exception = Record.Exception(() => TaskStateManager.ValidateStatusTransition(validStatus, currentState, TaskItemStatus.InProgress));
        Assert.Null(exception);
    }

    [Fact]
    public void ApplyStateTransition_ShouldThrowExceptionForInvalidTransition()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var task = new TaskItem("Test",
                                "Description",
                                null,
                                TaskItemStatus.Pending,
                                null,
                                _dateTimeMockService.Object);
        var invalidStatus = TaskItemStatus.Completed;

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

        var task = new TaskItem("Test",
                                "Description",
                                null,
                                TaskItemStatus.Pending,
                                null,
                                _dateTimeMockService.Object);
        var newStatus = TaskItemStatus.InProgress;

        // Act
        TaskStateManager.ApplyStateTransition(task, newStatus, task.State, task.Status);

        // Assert
        Assert.Equal(newStatus, task.Status);
    }

    [Theory]
    [InlineData(TaskItemStatus.InProgress, true)]
    [InlineData(TaskItemStatus.Cancelled, true)]
    [InlineData(TaskItemStatus.Completed, false)]
    public void PendingCanTransitionTo_ShouldReturnCorrectResult(TaskItemStatus newStatus, bool expectedResult)
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var state = new PendingState();
        var task = new TaskItem("Test Task",
                                "Test Description",
                                null,
                                TaskItemStatus.Pending,
                                null,
                                _dateTimeMockService.Object);

        // Act
        var result = state.CanTransitionTo(newStatus);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(TaskItemStatus.Pending, false)]
    [InlineData(TaskItemStatus.Cancelled, true)]
    [InlineData(TaskItemStatus.Completed, true)]
    public void InProgressCanTransitionTo_ShouldReturnCorrectResult(TaskItemStatus newStatus, bool expectedResult)
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var state = new InProgressState();
        var task = new TaskItem("Test Task",
                                "Test Description",
                                null,
                                TaskItemStatus.Pending,
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
        var task = new TaskItem("Test Task",
                                "Test Description",
                                null,
                                TaskItemStatus.Pending,
                                null,
                                _dateTimeMockService.Object);

        // Act
        state.Start(task);

        // Assert
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
    }

    [Fact]
    public void Complete_ShouldThrowException()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var state = new PendingState();
        var task = new TaskItem("Test Task",
                                "Test Description",
                                null,
                                TaskItemStatus.Pending,
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
        var task = new TaskItem("Test Task",
                                "Test Description",
                                null,
                                TaskItemStatus.Pending,
                                null,
                                _dateTimeMockService.Object);

        // Act
        state.Cancel(task);

        // Assert
        Assert.Equal(TaskItemStatus.Cancelled, task.Status);
    }
}

