using Application.Task.DTOs;
using FluentValidation.TestHelper;
using Moq;
using PresentationRestAPI.Task.Validators;
using Shared.Consts;
using Shared.Interfaces;

namespace PresentationRestAPI.Tests.Task.Validators;

public class TaskCreateValidatorTests
{
    private readonly TaskCreateValidator _validator;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;

    public TaskCreateValidatorTests()
    {
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(m => m.GetUTCNow()).Returns(new DateTime(2025, 3, 25));

        _validator = new TaskCreateValidator(_dateTimeProviderMock.Object);
    }

    [Fact]
    public void Should_Have_Error_When_AssignedUserEmail_Is_Not_Empty()
    {
        //Arrange
        var task = new TaskEntityDTO { AssignedUserEmail = "user@example.com" };

        //Act
        var result = _validator.TestValidate(task);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.AssignedUserEmail)
            .WithErrorMessage(Constants.VALIDATION_TASK_USER_CREATION);
    }

    [Fact]
    public void Should_Have_Error_When_CompletedAt_Is_Not_Empty()
    {
        //Arrange
        var task = new TaskEntityDTO { CompletedAt = DateTime.UtcNow };

        //Act
        var result = _validator.TestValidate(task);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.CompletedAt)
            .WithErrorMessage(Constants.VALIDATION_TASK_COMPLETE_AT_NOT_EMPTY);
    }

    [Fact]
    public void Should_Have_Error_When_CreatedAt_Is_Not_Empty()
    {
        //Arrange
        var task = new TaskEntityDTO { CreatedAt = DateTime.UtcNow };

        //Act
        var result = _validator.TestValidate(task);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.CreatedAt)
            .WithErrorMessage(Constants.VALIDATION_TASK_CREATED_AT_NOT_EMPTY);
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Not_Empty()
    {
        //Arrange
        var task = new TaskEntityDTO { Id = Guid.NewGuid() };

        //Act
        var result = _validator.TestValidate(task);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage(Constants.VALIDATION_TASK_ID_NOT_EMPTY);
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        //Arrange
        var task = new TaskEntityDTO { Title = "" };

        //Act
        var result = _validator.TestValidate(task);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage(Constants.VALIDATION_TASK_TITLE_NOT_EMPTY);
    }

    [Fact]
    public void Should_Have_Error_When_Title_Exceeds_Max_Length()
    {
        //Arrange
        var task = new TaskEntityDTO { Title = new string('A', 101) };

        //Act
        var result = _validator.TestValidate(task);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage(Constants.VALIDATION_TASK_TITLE_LENGTH);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Empty()
    {
        //Arrange
        var task = new TaskEntityDTO { Description = "" };

        //Act
        var result = _validator.TestValidate(task);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(Constants.VALIDATION_TASK_DESCRIPTION_NOT_EMPTY);
    }

    [Fact]
    public void Should_Have_Error_When_Deadline_Is_In_The_Past()
    {
        //Arrange
        var task = new TaskEntityDTO { Deadline = new DateTime(2024, 1, 1) };

        //Act
        var result = _validator.TestValidate(task);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Deadline)
            .WithErrorMessage(Constants.VALIDATION_TASK_DEADLINE_NOT_PAST);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Task_Is_Valid()
    {
        //Arrange
        var task = new TaskEntityDTO
        {
            Title = "Valid Task",
            Description = "Valid Description",
            Deadline = new DateTime(2025, 5, 1)
        };

        //Act
        var result = _validator.TestValidate(task);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}