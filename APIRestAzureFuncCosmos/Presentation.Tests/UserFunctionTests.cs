using Application.DTOs;
using Application.Interfaces;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Presentation.Interfaces;
using System.Text;

namespace Presentation.Tests;

public class UserFunctionTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IExceptionHandler> _mockExceptionHandler;
    private readonly Mock<IValidator<UserDTO>> _createValidatorMock;
    private readonly UserFunction _userFunction;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public UserFunctionTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockExceptionHandler = new Mock<IExceptionHandler>();
        _createValidatorMock = new Mock<IValidator<UserDTO>>();
        _userFunction = new UserFunction(_mockUserService.Object,
                                         _createValidatorMock.Object,
                                         _mockExceptionHandler.Object);
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnOk_WhenUsersExist()
    {
        //Arrange
        var users = new List<UserDTO> { new("name", "test@example.com") };
        _mockUserService.Setup(s => s.GetAllAsync(_cancellationToken)).ReturnsAsync(users);

        //Act
        var result = await _userFunction.GetAllUsers(Mock.Of<HttpRequest>(), _cancellationToken);

        //Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(users, okResult.Value);
    }

    [Fact]
    public async Task GetUserByEmail_ShouldReturnOk_WhenUserExists()
    {
        //Arrange
        var email = "test@example.com";
        var user = new UserDTO("name", "test@example.com");
        _mockUserService.Setup(s => s.GetByEmailAsync(email, _cancellationToken)).ReturnsAsync(user);

        //Act
        var result = await _userFunction.GetUserByEmail(Mock.Of<HttpRequest>(), email, _cancellationToken);

        //Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(user, okResult.Value);
    }

    [Fact]
    public async Task GetUserByEmail_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        //Arrange
        _mockUserService
            .Setup(s => s.GetByEmailAsync(It.IsAny<string>(), _cancellationToken))
            .ReturnsAsync((UserDTO?)null);

        //Act
        var result = await _userFunction.GetUserByEmail(Mock.Of<HttpRequest>(), "notfound@example.com", _cancellationToken);

        //Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnCreated_WhenUserIsCreated()
    {
        //Arrange
        var userDto = new UserDTO("name", "test@example.com");
        var requestBody = JsonConvert.SerializeObject(userDto);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        var createdResult = Result.Ok(userDto);
        _mockUserService.Setup(s => s.CreateAsync(It.IsAny<UserDTO>(), _cancellationToken)).ReturnsAsync(createdResult);

        //Act
        var result = await _userFunction.CreateUser(request, _cancellationToken);

        //Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdAtResult.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenUserCreationFails()
    {
        //Arrange
        var userDto = new UserDTO("name", "invalid@example.com");
        var requestBody = JsonConvert.SerializeObject(userDto);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        var failedResult = Result.Fail("User creation failed");
        _mockUserService.Setup(s => s.CreateAsync(It.IsAny<UserDTO>(), _cancellationToken)).ReturnsAsync(failedResult);

        //Act
        var result = await _userFunction.CreateUser(request, _cancellationToken);

        //Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
}