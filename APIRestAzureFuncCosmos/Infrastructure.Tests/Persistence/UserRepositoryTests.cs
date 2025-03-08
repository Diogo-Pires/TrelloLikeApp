using System.Net;
using Infrastructure.Config;
using Infrastructure.User;
using Microsoft.Azure.Cosmos;
using Moq;

namespace Infrastructure.Tests.Persistence;

public class UserRepositoryTests
{
    private readonly Mock<Container> _containerMock;
    private readonly Mock<CosmosClient> _cosmosClientMock;
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        _containerMock = new Mock<Container>();
        _cosmosClientMock = new Mock<CosmosClient>();

        var cosmosDbSettings = new CosmosDbSettings
        {
            DatabaseName = "TestDB",
            UserContainerName = "Users"
        };

        _cosmosClientMock
            .Setup(c => c.GetContainer(cosmosDbSettings.DatabaseName, cosmosDbSettings.UserContainerName))
            .Returns(_containerMock.Object);

        _userRepository = new UserRepository(_cosmosClientMock.Object, cosmosDbSettings);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new Domain.Entities.User("test@example.com", "Test User");

        _containerMock
            .Setup(c => c.ReadItemAsync<Domain.User.UserEntity>(user.Id, new PartitionKey(user.Id), null, default))
            .ReturnsAsync(Mock.Of<ItemResponse<Domain.User.UserEntity>>(r => r.Resource == user));

        // Act
        var result = await _userRepository.GetByEmailAsync(user.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        _containerMock
            .Setup(c => c.ReadItemAsync<Domain.User.UserEntity>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _userRepository.GetByEmailAsync("nonexistent@example.com", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnUser_WhenUserIsCreated()
    {
        // Arrange
        var user = new Domain.Entities.User("test@example.com", "Test User");

        _containerMock
            .Setup(c => c.CreateItemAsync(user, new PartitionKey(user.Id), null, default))
            .ReturnsAsync(Mock.Of<ItemResponse<Domain.User.UserEntity>>(r => r.Resource == user));

        // Act
        var result = await _userRepository.AddAsync(user, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }
}