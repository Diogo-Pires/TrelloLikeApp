using Application.User.DTOs;
using Application.User.Mappers;
using Application.User.Services;
using Domain.User;
using Domain.User.Interfaces;
using FluentAssertions;
using Infrastructure.Cache.Interfaces;
using Moq;

namespace Application.Tests.User.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IHybridCacheService> _cacheServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _cacheServiceMock = new Mock<IHybridCacheService>();
        _userService = new UserService(_userRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_Should_Return_All_Users_From_Cache_Or_Repository()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new("John Doe", "123", "google-xyz"),
            new("Jane Doe", "456", "google-abc")
        };
        var userDtos = users.Select(UserMapper.ToDTO).ToList();
        _cacheServiceMock
            .Setup(cs => cs.GetOrSetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<Task<List<UserEntityDTO>?>>>()))
            .ReturnsAsync(userDtos);

        // Act
        var result = await _userService.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(userDtos);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByEmailAsync_Should_Return_User_From_Cache_Or_Repository()
    {
        // Arrange
        var email = "test@example.com";
        var user = new UserEntity("John Doe", email, "google-xyz");
        _cacheServiceMock
            .Setup(cs => cs.GetOrSetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<Task<UserEntity?>>>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetByEmailAsync(email, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(UserMapper.ToDTO(user));
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_Should_Add_User_And_Clear_Cache()
    {
        // Arrange
        var userDto = new UserEntityDTO { Name = "John Doe", Id = "123", GoogleId = "google-xyz" };
        var userEntity = UserMapper.ToEntity(userDto);
        _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);

        // Act
        var result = await _userService.CreateAsync(userDto, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(UserMapper.ToDTO(userEntity));
        _cacheServiceMock.Verify(cs => cs.SetIfNotExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserEntity>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAllCacheAsync_Should_Increment_Cache_Version()
    {
        // Act
        await _userService.DeleteAllCacheAsync(CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(cs => cs.IncrementVersion(It.IsAny<string>()), Times.Once);
    }
}
