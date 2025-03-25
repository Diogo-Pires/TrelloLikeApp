using Application.User.DTOs;
using FluentValidation.TestHelper;
using PresentationRestAPI.User.Validators;
using Shared.Consts;

namespace PresentationRestAPI.Tests.User.Validators;

public class UserCreateValidatorTests
{
    private readonly UserCreateValidator _validator;

    public UserCreateValidatorTests()
    {
        _validator = new UserCreateValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        //Arrange
        var user = new UserEntityDTO { Name = "" };

        //Act
        var result = _validator.TestValidate(user);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(Constants.VALIDATION_USER_NAME_NOT_EMPTY);
    }

    [Fact]
    public void Should_Have_Error_When_GoogleId_Is_Empty()
    {
        //Arrange
        var user = new UserEntityDTO { GoogleId = "" };

        //Act
        var result = _validator.TestValidate(user);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.GoogleId)
            .WithErrorMessage(Constants.VALIDATION_USER_GOOGLE_NOT_EMPTY);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        //Arrange
        var user = new UserEntityDTO { Id = "" };

        //Act
        var result = _validator.TestValidate(user);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage(Constants.VALIDATION_USER_EMAIL_NOT_EMPTY);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        //Arrange
        var user = new UserEntityDTO { Id = "invalid-email" };

        //Act
        var result = _validator.TestValidate(user);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage(Constants.VALIDATION_USER_EMAIL_NOT_VALID);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_User_Is_Valid()
    {
        //Arrange
        var user = new UserEntityDTO
        {
            Name = "John Doe",
            GoogleId = "123456",
            Id = "john.doe@example.com"
        };

        //Act
        var result = _validator.TestValidate(user);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}