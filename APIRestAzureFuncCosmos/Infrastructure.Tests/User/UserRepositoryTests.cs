using Moq;
using System.Net;
using Domain.User;
using Domain.User.Exceptions;
using Infrastructure.Config;
using Infrastructure.User;
using Microsoft.Azure.Cosmos;
using FluentAssertions;

namespace Infrastructure.Tests.User;

public class UserRepositoryTests
{
    private readonly Mock<Container> _containerMock;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var cosmosClientMock = new Mock<CosmosClient>();
        _containerMock = new Mock<Container>();

        var dbSettings = new CosmosDbSettings
        {
            DatabaseName = "TestDb",
            UserContainerName = "Users"
        };

        cosmosClientMock
            .Setup(c => c.GetContainer(dbSettings.DatabaseName, dbSettings.UserContainerName))
            .Returns(_containerMock.Object);

        _repository = new UserRepository(cosmosClientMock.Object, dbSettings);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByEmailAsync_Should_Return_User_If_Found()
    {
        // Arrange
        var email = "user@example.com";
        var user = new UserEntity("test", email, "test");
        var responseMock = CreateItemResponseMock(user, HttpStatusCode.OK);

        _containerMock.Setup(c => c.ReadItemAsync<UserEntity>(email, new PartitionKey(email), null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMock.Object);

        // Act
        var result = await _repository.GetByEmailAsync(email, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByEmailAsync_Should_Return_Null_If_Not_Found()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _containerMock.Setup(c => c.ReadItemAsync<UserEntity>(email, new PartitionKey(email), null, It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _repository.GetByEmailAsync(email, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_Should_Add_User_When_Not_Exists()
    {
        // Arrange
        var email = "newuser@example.com";
        var user = new UserEntity("test", email, "test");
        var responseMock = CreateItemResponseMock(user, HttpStatusCode.Created);

        _containerMock.Setup(c => c.ReadItemAsync<UserEntity>(email, new PartitionKey(email), null, It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0)); // Simulate user does not exist

        _containerMock.Setup(c => c.CreateItemAsync(user, new PartitionKey(email), null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMock.Object);

        // Act
        var result = await _repository.AddAsync(user, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_Should_Throw_Exception_When_User_Already_Exists()
    {
        // Arrange
        var email = "existinguser@example.com";
        var user = new UserEntity("test", email, "test");
        var responseMock = CreateItemResponseMock(user, HttpStatusCode.OK);

        _containerMock.Setup(c => c.ReadItemAsync<UserEntity>(email, new PartitionKey(email), null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMock.Object); // Simulate user exists

        // Act & Assert
        await Assert.ThrowsAsync<UserException>(() => _repository.AddAsync(user, CancellationToken.None));
    }

    // Helper methods
    private static Mock<ItemResponse<T>> CreateItemResponseMock<T>(T resource, HttpStatusCode statusCode)
    {
        var responseMock = new Mock<ItemResponse<T>>();
        responseMock.Setup(r => r.StatusCode).Returns(statusCode);
        responseMock.Setup(r => r.Resource).Returns(resource);
        return responseMock;
    }
}