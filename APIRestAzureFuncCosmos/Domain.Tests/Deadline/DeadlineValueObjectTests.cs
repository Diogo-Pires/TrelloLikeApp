using Domain.Deadline;
using Domain.Deadline.Exceptions;
using FluentAssertions;
using Moq;
using Shared.Consts;
using Shared.Interfaces;

namespace Domain.Tests.Deadline;

public class DeadlineValueObjectTests
{
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;

    public DeadlineValueObjectTests()
    {
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
    }

    [Fact]
    public void Constructor_Should_Set_Value_When_Deadline_Is_Valid()
    {
        // Arrange
        var createdDate = new DateTime(2025, 03, 28);
        var deadlineDate = new DateTime(2025, 04, 01);
        _dateTimeProviderMock.Setup(dp => dp.GetUTCNow()).Returns(new DateTime(2025, 03, 27));

        // Act
        var deadline = new DeadlineValueObject(deadlineDate, createdDate, _dateTimeProviderMock.Object);

        // Assert
        deadline.Value.Should().Be(deadlineDate);
    }

    [Fact]
    public void Constructor_Should_Throw_Exception_When_Deadline_Is_In_The_Past()
    {
        // Arrange
        var pastDate = new DateTime(2025, 03, 01);
        var createdDate = new DateTime(2025, 03, 28);
        _dateTimeProviderMock.Setup(dp => dp.GetUTCNow()).Returns(new DateTime(2025, 03, 29));

        // Act
        var act = () => new DeadlineValueObject(pastDate, createdDate, _dateTimeProviderMock.Object);

        // Assert
        act.Should().Throw<DeadlineException>().WithMessage(Constants.VALIDATION_TASK_DEADLINE_NOT_PAST);
    }

    [Fact]
    public void Constructor_Should_Throw_Exception_When_Deadline_Is_Before_Creation_Date()
    {
        // Arrange
        var createdDate = new DateTime(2025, 03, 28);
        var invalidDeadlineDate = new DateTime(2025, 03, 27);
        _dateTimeProviderMock.Setup(dp => dp.GetUTCNow()).Returns(new DateTime(2025, 03, 26));

        // Act
        var act = () => new DeadlineValueObject(invalidDeadlineDate, createdDate, _dateTimeProviderMock.Object);

        // Assert
        act.Should().Throw<DeadlineException>().WithMessage(Constants.VALIDATION_TASK_CANNOT_BEFORE_CREATEAT);
    }

    [Fact]
    public void Constructor_Should_Allow_Null_Deadline()
    {
        // Arrange
        var createdDate = new DateTime(2025, 03, 28);

        // Act
        var deadline = new DeadlineValueObject(null, createdDate, _dateTimeProviderMock.Object);

        // Assert
        deadline.Value.Should().BeNull();
    }
}
