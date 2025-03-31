using Domain.Task;
using Domain.Task.Enums;
using Domain.Task.States;
using Domain.Task.States.Exceptions;
using FluentAssertions;
using Moq;
using Shared.Consts;
using Shared.Interfaces;

namespace Domain.Tests.Task.States;

public class CancelledStateTests
{
    private readonly CancelledState _cancelledState;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<TaskEntity> _taskMock;

    public CancelledStateTests()
    {
        _cancelledState = new CancelledState();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _taskMock = new Mock<TaskEntity>();
    }

    [Fact]
    public void Status_Should_Be_Pending()
    {
        // Assert
        _cancelledState.Status.Should().Be(TaskEntityStatus.Pending);
    }

    [Fact]
    public void CanTransitionTo_Should_Always_Return_False()
    {
        // Act
        var result = _cancelledState.CanTransitionTo(TaskEntityStatus.Completed);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Start_Should_Throw_TaskStateException()
    {
        // Act
        var act = () => _cancelledState.Start(_taskMock.Object);

        // Assert
        act.Should().Throw<TaskStateException>().WithMessage(Constants.VALIDATION_TASK_CANNOT_START_CANCELLED);
    }

    [Fact]
    public void Complete_Should_Throw_TaskStateException()
    {
        // Act
        var act = () => _cancelledState.Complete(_taskMock.Object, _dateTimeProviderMock.Object);

        // Assert
        act.Should().Throw<TaskStateException>().WithMessage(Constants.VALIDATION_TASK_CANNOT_COMPLETE_CANCELLED);
    }

    [Fact]
    public void Cancel_Should_Throw_TaskStateException()
    {
        // Act
        var act = () => _cancelledState.Cancel(_taskMock.Object);

        // Assert
        act.Should().Throw<TaskStateException>().WithMessage(Constants.VALIDATION_TASK_ALREADY_CANCELLED);
    }
}
