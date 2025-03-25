using Application.User.DTOs;
using Application.User.Interfaces;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using PresentationRestAPI.User.Interfaces;
using PresentationRestAPI.User.Middleswares;
using System.Security.Claims;
using FluentResults;

namespace PresentationRestAPI.Tests.User.Middlewares;

public class UserValidationMiddlewareTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IUserCreatorValidator> _validatorMock;
    private readonly RequestDelegate _nextMock;
    private readonly UserValidationMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public UserValidationMiddlewareTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _validatorMock = new Mock<IUserCreatorValidator>();
        _nextMock = new Mock<RequestDelegate>().Object;

        _middleware = new UserValidationMiddleware(_nextMock);
        _httpContext = new DefaultHttpContext();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Call_Next_When_User_Is_Not_Authenticated()
    {
        // Arrange
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); 

        // Act
        await _middleware.Invoke(_httpContext, _userServiceMock.Object, _validatorMock.Object);

        // Assert
        Assert.True(_httpContext.Response.StatusCode == StatusCodes.Status200OK);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Call_Next_When_Email_Name_Or_GoogleId_Is_Missing()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, "user@example.com"),
            new(ClaimTypes.NameIdentifier, "") 
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        // Act
        await _middleware.Invoke(_httpContext, _userServiceMock.Object, _validatorMock.Object);

        // Assert
        Assert.True(_httpContext.Response.StatusCode == StatusCodes.Status200OK);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Call_Next_When_User_Already_Exists()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, "user@example.com"),
            new(ClaimTypes.NameIdentifier, "google-123"),
            new("name", "Test User")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        _userServiceMock
            .Setup(s => s.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserEntityDTO("Test User", "user@example.com", "google-123"));

        // Act
        await _middleware.Invoke(_httpContext, _userServiceMock.Object, _validatorMock.Object);

        // Assert
        Assert.True(_httpContext.Response.StatusCode == StatusCodes.Status200OK);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Call_Next_When_Validation_Fails()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, "user@example.com"),
            new(ClaimTypes.NameIdentifier, "google-123"),
            new("name", "Test User")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        _userServiceMock
            .Setup(s => s.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntityDTO)null);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UserEntityDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
            {
            new ValidationFailure("Email", "Invalid email format")
            }));

        // Act
        await _middleware.Invoke(_httpContext, _userServiceMock.Object, _validatorMock.Object);

        // Assert
        Assert.True(_httpContext.Response.StatusCode == StatusCodes.Status200OK);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Call_Next_When_User_Creation_Fails()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, "user@example.com"),
            new(ClaimTypes.NameIdentifier, "google-123"),
            new("name", "Test User")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        _userServiceMock
            .Setup(s => s.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntityDTO)null);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UserEntityDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _userServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<UserEntityDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Error creating user"));

        // Act
        await _middleware.Invoke(_httpContext, _userServiceMock.Object, _validatorMock.Object);

        // Assert
        Assert.True(_httpContext.Response.StatusCode == StatusCodes.Status200OK);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Create_User_When_Valid()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, "user@example.com"),
            new(ClaimTypes.NameIdentifier, "google-123"),
            new("name", "Test User")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        _userServiceMock
            .Setup(s => s.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntityDTO)null);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UserEntityDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _userServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<UserEntityDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        await _middleware.Invoke(_httpContext, _userServiceMock.Object, _validatorMock.Object);

        // Assert
        Assert.True(_httpContext.Response.StatusCode == StatusCodes.Status200OK);
    }
}