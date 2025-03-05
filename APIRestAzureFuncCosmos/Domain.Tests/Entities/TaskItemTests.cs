using Domain.Enums;
using Shared.Exceptions;
using Domain.Entities;
using Moq;
using Shared.Interfaces;
using Shared.Consts;

namespace Domain.Tests.Entities;

public class TaskItemTests
{
    private readonly Mock<IDateTimeProvider> _dateTimeMockService;

    public TaskItemTests()
    {
        _dateTimeMockService = new Mock<IDateTimeProvider>();
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var title = "Test Task";
        var description = "Test Description";
        var deadline = DateTime.UtcNow.AddDays(1);

        // Act
        var task = new TaskItem(title, description, deadline, null, null, _dateTimeMockService.Object);

        // Assert
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(title, task.Title);
        Assert.Equal(description, task.Description);
        Assert.Equal(TaskItemStatus.Pending, task.Status);
        Assert.Null(task.AssignedUser);
    }

    [Fact]
    public void ValidateCreation_ShouldThrowException_WhenTitleIsEmpty()
    {
        //Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TaskItem.ValidateCreation(null,
                                      string.Empty,
                                      "Valid Description",
                                      DateTime.UtcNow.AddDays(1),
                                      null,
                                      null,
                                      _dateTimeMockService.Object));

        Assert.Equal(Constants.VALIDATION_TASK_TITLE_NOT_EMPTY, exception.Message);
    }

    [Fact]
    public void ValidateCreation_ShouldThrowException_WhenTitleIsTooLong()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var longTitle = new string('A', 101);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TaskItem.ValidateCreation(null,
                                      longTitle,
                                      "Valid Description",
                                      DateTime.UtcNow.AddDays(1),
                                      null,
                                      null,
                                      _dateTimeMockService.Object));

        Assert.Equal(Constants.VALIDATION_TASK_TITLE_LENGTH, exception.Message);
    }

    [Fact]
    public void ValidateCreation_ShouldThrowException_WhenCreateAtIsProvided()
    {
        //Arrange
        var datetime = new DateTime(2025, 1, 1);

        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(datetime.AddDays(1));

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TaskItem.ValidateCreation(null, 
                                      "Valid Title",
                                      "Valid Description",
                                      datetime.AddDays(1),
                                      DateTime.UtcNow,
                                      null,
                                      _dateTimeMockService.Object));

        Assert.Equal(Constants.VALIDATION_TASK_CREATED_AT_NOT_EMPTY, exception.Message);
    }
    [Fact]
    public void ValidateCreation_ShouldThrowException_WhenDeadlineIsInPast()
    {
        //Arrange
        var datetime = new DateTime(2025, 1, 1);

        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(datetime.AddDays(1));

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TaskItem.ValidateCreation(null,
                                      "Valid Title",
                                      "Valid Description",
                                      datetime,
                                      null,
                                      null,
                                      _dateTimeMockService.Object));

        Assert.Equal(Constants.VALIDATION_TASK_DEADLINE_NOT_PAST, exception.Message);
    }


    [Fact]
    public void UpdateTask_ShouldUpdateFieldsCorrectly()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var task = new TaskItem("Title",
                                "Description",
                                DateTime.UtcNow.AddDays(1),
                                null,
                                null,
                                _dateTimeMockService.Object);

        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newDeadline = DateTime.UtcNow.AddDays(2);

        // Act
        task.UpdateTask(newTitle, newDescription, newDeadline, TaskItemStatus.InProgress);

        // Assert
        Assert.Equal(newTitle, task.Title);
        Assert.Equal(newDescription, task.Description);
        Assert.Equal(newDeadline, task.Deadline);
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
    }

    [Fact]
    public void ChangeStatus_ShouldChangeStatus_WhenValid()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var task = new TaskItem("Task",
                                "Description",
                                DateTime.UtcNow.AddDays(1),
                                TaskItemStatus.Pending,
                                null,
                                _dateTimeMockService.Object);

        // Act
        task.ChangeStatus(TaskItemStatus.InProgress);

        // Assert
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
    }

    [Fact]
    public void AssignToUser_ShouldSetAssignedUser()
    {
        // Arrange
        _dateTimeMockService
            .Setup(m => m.GetUTCNow())
            .Returns(new DateTime(2025, 1, 1));

        var task = new TaskItem("Task",
                                "Description",
                                DateTime.UtcNow.AddDays(1),
                                null,
                                null,
                                _dateTimeMockService.Object);
        var user = new User("user@example.com", "User Name");

        // Act
        task.AssignToUser(user);

        // Assert
        Assert.Equal(user.Id, task.AssignedUserEmail);
        Assert.Equal(user, task.AssignedUser);
    }
}
