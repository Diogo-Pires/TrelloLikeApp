using Application.Task.DTOs;
using Application.Task.Mappers;
using Domain.Task;
using Domain.Task.Enums;
using Moq;
using Shared.Interfaces;

namespace Application.Tests.Task.Mappers;

public class TaskMapperTests
{
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;

    public TaskMapperTests()
    {
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
    }

    [Fact]
    public void ToEntity_Should_Map_Dto_To_Entity_Correctly()
    {
        // Arrange
        var dto = new TaskEntityDTO
        {
            Id = Guid.NewGuid(),
            Title = "  Task Title  ",
            Description = "  Task Description  ",
            Status = TaskEntityStatus.Pending,
            CreatedAt = DateTime.Now,
            CompletedAt = DateTime.Now.AddDays(1),
            Deadline = DateTime.Now.AddDays(5),
            AssignedUserEmail = "user@example.com"
        };

        var expectedDateTime = DateTime.Now;
        _dateTimeProviderMock.Setup(d => d.GetUTCNow()).Returns(expectedDateTime);

        // Act
        var result = TaskMapper.ToEntity(dto, _dateTimeProviderMock.Object);

        // Assert
        Assert.Equal(dto.Title.Trim(), result.Title);
        Assert.Equal(dto.Description.Trim(), result.Description);
        Assert.Equal(dto.Status, result.Status);
        Assert.Equal(dto.AssignedUserEmail, result.AssignedUserEmail);
        Assert.Equal(dto.Deadline, result.Deadline);
        Assert.Equal(expectedDateTime, result.CreatedAt);
        Assert.Null(result.CompletedAt);
    }

    [Fact]
    public void ToDTO_Should_Map_Entity_To_Dto_Correctly()
    {
        // Arrange
        var entity = new TaskEntity(
            "Task Title",
            "Task Description",
            DateTime.Now.AddDays(5),
            TaskEntityStatus.Pending,
            "user@example.com",
            _dateTimeProviderMock.Object
        );

        // Act
        var result = TaskMapper.ToDTO(entity);

        // Assert
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Title, result.Title);
        Assert.Equal(entity.Description, result.Description);
        Assert.Equal(entity.Status, result.Status);
        Assert.Equal(entity.CreatedAt, result.CreatedAt);
        Assert.Equal(entity.CompletedAt, result.CompletedAt);
        Assert.Equal(entity.Deadline, result.Deadline);
        Assert.Equal(entity.AssignedUserEmail, result.AssignedUserEmail);
    }

    [Fact]
    public void ToEntity_Should_Trim_Title_And_Description()
    {
        // Arrange
        var dto = new TaskEntityDTO
        {
            Title = "  Task Title  ",
            Description = "  Task Description  "
        };

        _dateTimeProviderMock.Setup(d => d.GetUTCNow()).Returns(DateTime.Now);

        // Act
        var result = TaskMapper.ToEntity(dto, _dateTimeProviderMock.Object);

        // Assert
        Assert.Equal("Task Title", result.Title);
        Assert.Equal("Task Description", result.Description);
    }
}
