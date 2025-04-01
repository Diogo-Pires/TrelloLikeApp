using Domain.Task;
using Domain.Task.Enums;
using Domain.Task.States.Exceptions;
using Domain.User;
using FluentAssertions;
using Moq;
using Shared.Interfaces;

namespace Domain.Tests.Task;

public class TaskEntityTests
{
    [Fact]
    public void Constructor_Should_Initialize_Properties_Correctly()
    {
        // Arrange
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        var now = DateTime.UtcNow;
        dateTimeProviderMock.Setup(p => p.GetUTCNow()).Returns(now);

        // Act
        var task = new TaskEntity("Test Title", "Test Description", now.AddDays(1), TaskEntityStatus.Pending, "user@example.com", dateTimeProviderMock.Object);

        // Assert
        task.Id.Should().NotBeEmpty();
        task.Title.Should().Be("Test Title");
        task.Description.Should().Be("Test Description");
        task.CreatedAt.Should().Be(now);
        task.Deadline.Should().Be(now.AddDays(1));
        task.Status.Should().Be(TaskEntityStatus.Pending);
        task.AssignedUserEmail.Should().Be("user@example.com");
    }

    [Fact]
    public void ChangeStatus_Should_Update_Status_When_Valid()
    {
        // Arrange
        var task = new TaskEntity("Test Title", "Test Description", null, TaskEntityStatus.Pending, null, new Mock<IDateTimeProvider>().Object);

        // Act
        task.ChangeStatus(TaskEntityStatus.InProgress);

        // Assert
        task.Status.Should().Be(TaskEntityStatus.InProgress);
    }

    [Fact]
    public void ChangeStatus_Should_Throw_Exception_When_Invalid()
    {
        // Arrange
        var task = new TaskEntity("Test Title", "Test Description", null, TaskEntityStatus.Completed, null, new Mock<IDateTimeProvider>().Object);

        // Act
        Action act = () => task.ChangeStatus(TaskEntityStatus.Pending);

        // Assert
        act.Should().Throw<TaskStateException>();
    }

    [Fact]
    public void UpdateTask_Should_Update_Title_Description_And_Deadline()
    {
        // Arrange
        var task = new TaskEntity("Old Title", "Old Description", null, TaskEntityStatus.Pending, null, new Mock<IDateTimeProvider>().Object);
        var newDeadline = DateTime.UtcNow.AddDays(5);

        // Act
        task.UpdateTask("New Title", "New Description", newDeadline, null);

        // Assert
        task.Title.Should().Be("New Title");
        task.Description.Should().Be("New Description");
        task.Deadline.Should().Be(newDeadline);
    }

    [Fact]
    public void AssignToUser_Should_Set_AssignedUserEmail()
    {
        // Arrange
        var task = new TaskEntity("Test Title", "Test Description", null, TaskEntityStatus.Pending, null, new Mock<IDateTimeProvider>().Object);
        var user = new UserEntity("test", "user@example.com", "test");

        // Act
        task.AssignToUser(user);

        // Assert
        task.AssignedUserEmail.Should().Be("user@example.com");
    }
}
