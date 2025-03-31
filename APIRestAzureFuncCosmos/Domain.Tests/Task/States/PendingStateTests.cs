using Domain.Task;
using Domain.Task.Enums;
using Domain.Task.States;
using Domain.Task.States.Exceptions;
using FluentAssertions;
using Moq;
using Shared.Consts;

namespace Domain.Tests.Task.States;

public class PendingStateTests
{
    private readonly PendingState _pendingState;
    private readonly Mock<TaskEntity> _taskMock;

    public PendingStateTests()
    {
        _pendingState = new PendingState();
        _taskMock = new Mock<TaskEntity>();
    }

    [Fact]
    public void Status_Should_Be_Pending()
    {
        // Assert
        _pendingState.Status.Should().Be(TaskEntityStatus.Pending);
    }

    [Fact]
    public void CanTransitionTo_Should_Return_True_For_InProgress_Or_Cancelled()
    {
        // Act & Assert
        _pendingState.CanTransitionTo(TaskEntityStatus.InProgress).Should().BeTrue();
        _pendingState.CanTransitionTo(TaskEntityStatus.Cancelled).Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_Should_Return_False_For_Other_Statuses()
    {
        // Act
        var result = _pendingState.CanTransitionTo(TaskEntityStatus.Completed);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Start_Should_Change_Status_To_InProgress()
    {
        // Act
        _pendingState.Start(_taskMock.Object);

        // Assert
        _taskMock.Verify(t => t.ChangeStatus(TaskEntityStatus.InProgress), Times.Once);
    }

    [Fact]
    public void Complete_Should_Throw_TaskStateException()
    {
        // Act
        var act = () => _pendingState.Complete(_taskMock.Object);

        // Assert
        act.Should().Throw<TaskStateException>().WithMessage(Constants.VALIDATION_TASK_MUST_BE_STARTED);
    }

    [Fact]
    public void Cancel_Should_Change_Status_To_Cancelled()
    {
        // Act
        _pendingState.Cancel(_taskMock.Object);

        // Assert
        _taskMock.Verify(t => t.ChangeStatus(TaskEntityStatus.Cancelled), Times.Once);
    }
}