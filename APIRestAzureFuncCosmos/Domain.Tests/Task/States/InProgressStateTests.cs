using Domain.Task;
using Domain.Task.Enums;
using Domain.Task.States;
using Domain.Task.States.Exceptions;
using FluentAssertions;
using Moq;
using Shared.Consts;
using Shared.Interfaces;

namespace Domain.Tests.Task.States;

public class InProgressStateTests
{
    private readonly InProgressState _inProgressState;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<TaskEntity> _taskMock;

    public InProgressStateTests()
    {
        _inProgressState = new InProgressState();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _taskMock = new Mock<TaskEntity>();
        _taskMock.Object.UpdateTask(" ", " ", null, TaskEntityStatus.InProgress);
    }

    [Fact]
    public void Status_Should_Be_InProgress()
    {
        // Assert
        _inProgressState.Status.Should().Be(TaskEntityStatus.InProgress);
    }

    [Fact]
    public void CanTransitionTo_Should_Return_True_For_Completed_Or_Cancelled()
    {
        // Act & Assert
        _inProgressState.CanTransitionTo(TaskEntityStatus.Completed).Should().BeTrue();
        _inProgressState.CanTransitionTo(TaskEntityStatus.Cancelled).Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_Should_Return_False_For_Other_Statuses()
    {
        // Act
        var result = _inProgressState.CanTransitionTo(TaskEntityStatus.Pending);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Start_Should_Throw_TaskStateException()
    {
        // Act
        var act = () => _inProgressState.Start(_taskMock.Object);

        // Assert
        act.Should().Throw<TaskStateException>().WithMessage(Constants.VALIDATION_TASK_ALREADY_PROGRESS);
    }

    [Fact]
    public void Complete_Should_Change_Status_To_Completed_And_Set_CompletedAt()
    {
        // Arrange
        var now = new DateTime(2025, 03, 28);
        _dateTimeProviderMock.Setup(dp => dp.GetUTCNow()).Returns(now);

        // Act
        _inProgressState.Complete(_taskMock.Object, _dateTimeProviderMock.Object);

        // Assert
        _taskMock.Verify(t => t.ChangeStatus(TaskEntityStatus.Completed), Times.Once);
        _taskMock.Verify(t => t.SetCompletedAt(now), Times.Once);
    }

    [Fact]
    public void Cancel_Should_Change_Status_To_Cancelled()
    {
        // Act
        _inProgressState.Cancel(_taskMock.Object);

        // Assert
        _taskMock.Verify(t => t.ChangeStatus(TaskEntityStatus.Cancelled), Times.Once);
    }
}
