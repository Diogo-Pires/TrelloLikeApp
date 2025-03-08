using Application.DTOs;
using Application.Task.Mappers;
using Domain.Entities;
using Domain.Task.Enums;
using Moq;
using Shared.Interfaces;

namespace Application.Tests.Mappers;

public class TaskMapperTests
{
    [Fact]
    public void ToEntity_ShouldMapCorrectly()
    {
        //Arrange
        var dateTime = DateTime.UtcNow;
        var title = "Test Task";
        var description = "Test Description";
        var taskDto = new TaskDTO(Guid.NewGuid(), title, description, TaskEntityStatus.Pending, dateTime, dateTime, dateTime, null);
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();

        //Act
        var entity = TaskMapper.ToEntity(taskDto, dateTimeProviderMock.Object);

        //Assert
        Assert.Equal(title, entity.Title);
        Assert.Equal(description, entity.Description);
    }

    [Fact]
    public void ToDTO_ShouldMapCorrectly()
    {
        //Arrange
        var _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        var dateTime = DateTime.UtcNow;
        var entity = new Domain.Entities.TaskEntity("Test Task", "Description", dateTime, TaskEntityStatus.Pending, null, _dateTimeProviderMock.Object);

        //Act
        var dto = TaskMapper.ToDTO(entity);

        //Assert
        Assert.Equal(entity.Title, dto.Title);
        Assert.Equal(entity.Description, dto.Description);
    }
}
