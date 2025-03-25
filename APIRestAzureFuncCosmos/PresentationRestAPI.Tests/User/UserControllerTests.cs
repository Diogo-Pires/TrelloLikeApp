using Application.User.DTOs;
using Application.User.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PresentationRestAPI.User;

namespace PresentationRestAPI.Tests.User;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _controller = new UsersController(_mockUserService.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllUsers_ReturnsOkResult_WithUserList()
    {
        // Arrange
        var users = new List<UserEntityDTO> { new() { Id = "test@example.com" } };
        _mockUserService.Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnUsers = Assert.IsType<List<UserEntityDTO>>(okResult.Value);
        Assert.Single(returnUsers);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetUserByEmail_ReturnsOkResult_WhenUserExists()
    {
        // Arrange
        var user = new UserEntityDTO { Id = "test@example.com" };
        _mockUserService.Setup(service => service.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserByEmail("test@example.com", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnUser = Assert.IsType<UserEntityDTO>(okResult.Value);
        Assert.Equal("test@example.com", returnUser.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetUserByEmail_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserService
            .Setup(service => service.GetByEmailAsync("unknown@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntityDTO)null);

        // Act
        var result = await _controller.GetUserByEmail("unknown@example.com", CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetUserByEmail_ReturnsBadRequest_WhenEmailIsEmpty()
    {
        // Act
        var result = await _controller.GetUserByEmail("", CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task ClearAllCaches_ReturnsNoContent()
    {
        // Arrange
        _mockUserService
            .Setup(service => service.DeleteAllCacheAsync(It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _controller.ClearAllCaches(CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}