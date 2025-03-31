using Domain.Task;
using Domain.Task.Enums;
using Domain.Task.States;
using Domain.Task.States.Exceptions;
using FluentAssertions;
using Moq;
using Shared.Consts;

namespace Domain.Tests.Task.States;

public class TaskStateManagerTests
{
    [Theory]
    [InlineData(TaskEntityStatus.Pending, typeof(PendingState))]
    [InlineData(TaskEntityStatus.InProgress, typeof(InProgressState))]
    [InlineData(TaskEntityStatus.Completed, typeof(CompletedState))]
    [InlineData(TaskEntityStatus.Cancelled, typeof(CancelledState))]
    public void GetState_Should_Return_Correct_State(TaskEntityStatus status, Type expectedType)
    {
        // Act
        var state = TaskStateManager.GetState(status);

        // Assert
        state.Should().BeOfType(expectedType);
    }

    [Fact]
    public void GetState_Should_Throw_Exception_For_Invalid_Status()
    {
        // Act
        var act = () => TaskStateManager.GetState((TaskEntityStatus)999);

        // Assert
        act.Should().Throw<TaskStateException>().WithMessage($"{Constants.VALIDATION_TASK_INVALID_STATUS}: 999");
    }

    [Theory]
    [InlineData(TaskEntityStatus.InProgress, TaskEntityStatus.Pending, true)]
    [InlineData(TaskEntityStatus.Completed, TaskEntityStatus.InProgress, true)]
    [InlineData(TaskEntityStatus.Cancelled, TaskEntityStatus.InProgress, true)]
    [InlineData(TaskEntityStatus.Pending, TaskEntityStatus.InProgress, false)]
    [InlineData(TaskEntityStatus.InProgress, TaskEntityStatus.InProgress, false)]
    [InlineData(TaskEntityStatus.Pending, TaskEntityStatus.Pending, false)]
    [InlineData(TaskEntityStatus.Completed, TaskEntityStatus.Cancelled, false)]
    [InlineData(TaskEntityStatus.InProgress, TaskEntityStatus.Cancelled, false)]
    [InlineData(TaskEntityStatus.Pending, TaskEntityStatus.Cancelled, false)]
    [InlineData(TaskEntityStatus.Pending, TaskEntityStatus.Completed, false)]
    [InlineData(TaskEntityStatus.Cancelled, TaskEntityStatus.Completed, false)]
    [InlineData(TaskEntityStatus.InProgress, TaskEntityStatus.Completed, false)]
    [InlineData(TaskEntityStatus.Completed, TaskEntityStatus.Completed, false)]
    public void ValidateStatusTransition_Should_Throw_Exception_If_Transition_Not_Allowed(
        TaskEntityStatus newStatus, TaskEntityStatus oldStatus, bool isValid)
    {
        // Arrange
        var state = TaskStateManager.GetState(oldStatus);

        // Act
        Action act = () => TaskStateManager.ValidateStatusTransition(newStatus, state, oldStatus);

        // Assert
        if (isValid)
        {
            act.Should().NotThrow<TaskStateException>();
        }
        else
        {
            act.Should().Throw<TaskStateException>().WithMessage($"{Constants.VALIDATION_TASK_INVALID_STATUS_TRANSITION}: {oldStatus} → {newStatus}");
        }
    }

    [Fact]
    public void ApplyStateTransition_Should_Change_State_If_Valid()
    {
        // Arrange
        var taskMock = new Mock<TaskEntity>();
        var state = TaskStateManager.GetState(TaskEntityStatus.Pending);

        // Act
        TaskStateManager.ApplyStateTransition(taskMock.Object, TaskEntityStatus.InProgress, state, TaskEntityStatus.Pending);

        // Assert
        taskMock.Verify(t => t.ChangeStatus(TaskEntityStatus.InProgress), Times.Once);
    }

    [Fact]
    public void ApplyStateTransition_Should_Throw_Exception_For_Invalid_Transition()
    {
        // Arrange
        var taskMock = new Mock<TaskEntity>();
        var state = TaskStateManager.GetState(TaskEntityStatus.Completed);

        // Act
        Action act = () => TaskStateManager.ApplyStateTransition(taskMock.Object, TaskEntityStatus.Pending, state, TaskEntityStatus.Completed);

        // Assert
        act.Should().Throw<TaskStateException>().WithMessage($"{Constants.VALIDATION_TASK_INVALID_STATUS_TRANSITION}: {TaskEntityStatus.Completed} → {TaskEntityStatus.Pending}");
    }
}