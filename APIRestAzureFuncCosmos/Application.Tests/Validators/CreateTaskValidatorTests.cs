using Application.DTOs;
using Application.Validators;
using Domain.Enums;

namespace Application.Tests.Validators;

public class CreateTaskValidatorTests
{
    private readonly CreateTaskValidator _validator;

    public CreateTaskValidatorTests()
    {
        _validator = new CreateTaskValidator();
    }

    [Fact]
    public void Validate_ShouldPass_WhenTaskIsValid()
    {
        //Arrange
        var dateTime = DateTime.UtcNow;
        var taskDto = new TaskDTO("Test Task", "Test Description", TaskItemStatus.Pending, null, null);
       
        //Act
        var result = _validator.Validate(taskDto);

        //Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsEmpty()
    {
        //Arrange
        var dateTime = DateTime.UtcNow;
        var taskDto = new TaskDTO("", "Test Description", TaskItemStatus.Pending, dateTime, null);

        //Act
        var result = _validator.Validate(taskDto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("title"));
    }

    [Fact]
    public void Validate_ShouldFail_WhenDeadlineIsInPast()
    {
        //Arrange
        var dateTime = DateTime.UtcNow;
        var taskDto = new TaskDTO("Test Task", "Test Description", TaskItemStatus.Pending, dateTime,  null);

        //Act
        var result = _validator.Validate(taskDto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("deadline"));
    }
}

