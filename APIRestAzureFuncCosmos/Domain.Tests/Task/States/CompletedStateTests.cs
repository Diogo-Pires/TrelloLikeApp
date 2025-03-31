using Domain.Task;
using Domain.Task.Enums;
using Domain.Task.States;
using Domain.Task.States.Exceptions;
using FluentAssertions;
using Moq;
using Shared.Consts;
using Shared.Interfaces;

namespace Domain.Tests.Task.States;

public class CompletedStateTests
{
    private readonly CompletedState _completedState;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<TaskEntity> _taskMock;

    public CompletedStateTests()
    {
        _completedState = new CompletedState();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _taskMock = new Mock<TaskEntity>();
    }

    [Fact]
    public void Status_Should_Be_Completed()
    {
        // Assert
        _completedState.Status.Should().Be(TaskEntityStatus.Completed);
    }

    [Fact]
    public void CanTransitionTo_Should_Always_Return_False()
    {
        // Act
        var result = _completedState.CanTransitionTo(TaskEntityStatus.Pending);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Start_Should_Throw_TaskStateException()
    {
        // Act
        var act = () => _completedState.Start(_taskMock.Object);

        // Assert
        act.Should().Throw<TaskStateException>().WithMessage(Constants.VALIDATION_TASK_RESTART_COMPLETED_TASK);
    }

    [Fact]
    public void Complete_Should_Throw_TaskStateException()
    {
        // Act
        var act = () => _completedState.Complete(_taskMock.Object, _dateTimeProviderMock.Object);

        // Assert
        act.Should().Throw<TaskStateException>().WithMessage(Constants.VALIDATION_TASK_ALREADY_COMPLETED);
    }

    [Fact]
    public void Cancel_Should_Throw_TaskStateException()
    {
        // Act
        var act = () => _completedState.Cancel(_taskMock.Object);

        // Assert
        act.Should().Throw<TaskStateException>().WithMessage(Constants.VALIDATION_TASK_CANNOT_CANCEL_COMPLETE);
    }
}
