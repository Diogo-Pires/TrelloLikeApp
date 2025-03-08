using Application.Cache.Interfaces;
using Application.User.DTOs;
using Application.User.Mappers;
using Application.User.Services;
using Domain.Entities;
using Domain.User.Interfaces;
using FluentValidation;
using Moq;

namespace Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<UserEntityDTO>> _validatorMock;
    private readonly Mock<IHybridCacheService> _cacheServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _validatorMock = new Mock<IValidator<UserEntityDTO>>();
        _cacheServiceMock = new Mock<IHybridCacheService>();
        _userService = new UserService(_userRepositoryMock.Object, _validatorMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsersFromCacheOrRepository()
    {
        // Arrange
        var email = "test@example.com";
        var users = new List<UserEntityDTO> { new("Test User", email) };
        _cacheServiceMock
            .Setup(c => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<List<UserEntityDTO>?>>>()))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(email, result.First().Id);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUserFromCacheOrRepository()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User("Test User", email);
        _cacheServiceMock
            .Setup(c => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<User?>>>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetByEmailAsync(email, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var user = new UserEntityDTO("test@example.com", "Test User");
        var validationResult = new FluentValidation.Results.ValidationResult(
        [
            new FluentValidation.Results.ValidationFailure("Email", "Email is required")
        ]);

        _validatorMock
            .Setup(v => v.ValidateAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _userService.CreateAsync(user, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Email is required", result.Errors.Select(e => e.Message));
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenValidationPasses()
    {
        // Arrange
        var user = new UserEntityDTO("test@example.com", "Test User");
        var userEntity = UserMapper.ToEntity(user);
        _validatorMock
            .Setup(v => v.ValidateAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);

        // Act
        var result = await _userService.CreateAsync(user, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.Id);
    }
}