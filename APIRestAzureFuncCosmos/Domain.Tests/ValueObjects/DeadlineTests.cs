using Domain.ValueObjects;
using Moq;
using Shared.Consts;
using Shared.Exceptions;
using Shared.Interfaces;

namespace Domain.Tests.ValueObjects;

public class DeadlineTests
{
    private readonly Mock<IDateTimeProvider> _dateTimeMockService;
    public DeadlineTests()
    {
        _dateTimeMockService = new Mock<IDateTimeProvider>();
    }

    [Fact]
    public void Constructor_ShouldSetDeadline_WhenValid()
    {
        // Arrange
        var datetime = new DateTime(2025, 1, 1);

        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(datetime);

        var createdDate = datetime;
        var deadline = datetime.AddDays(5);

        //Act
        var deadlineObj = new Deadline(deadline, createdDate, _dateTimeMockService.Object);

        //Assert
        Assert.Equal(deadline, deadlineObj.Value);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDeadlineIsInPast()
    {
        // Arrange
        var datetime = new DateTime(2025, 1, 1);

        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(datetime.AddDays(1));

        var createdDate = datetime;
        var pastDeadline = datetime.AddDays(-1);

        //Act
        var exception = Assert.Throws<DomainException>(() => new Deadline(pastDeadline, createdDate, _dateTimeMockService.Object));

        //Assert
        Assert.Equal(Constants.VALIDATION_TASK_DEADLINE_NOT_PAST, exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDeadlineIsBeforeCreatedAt()
    {
        // Arrange
        var datetime = new DateTime(2025, 1, 1);

        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(datetime);

        var createdDate = datetime.AddHours(1);
        var earlierDeadline = datetime.AddSeconds(1);

        //Act
        var exception = Assert.Throws<DomainException>(() => new Deadline(earlierDeadline, createdDate, _dateTimeMockService.Object));

        //Assert
        Assert.Equal(Constants.VALIDATION_TASK_CANNOT_BEFORE_CREATEAT, exception.Message);
    }
}
