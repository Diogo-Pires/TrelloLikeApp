using Domain.User.Interfaces;
using Infrastructure.Config;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Net;

namespace Infrastructure.User;

public class UserRepository : IUserRepository
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;

    public UserRepository(CosmosClient cosmosClient, CosmosDbSettings cosmosDbSettings)
    {
        _cosmosClient = cosmosClient;
        _container = _cosmosClient.GetContainer(cosmosDbSettings.DatabaseName, cosmosDbSettings.UserContainerName);
    }

    public async Task<List<Domain.User.UserEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<Domain.User.UserEntity> userList = [];
        using (FeedIterator<Domain.User.UserEntity> setIterator =
                _container.GetItemLinqQueryable<Domain.User.UserEntity>()
                    .ToFeedIterator())
        {
            while (setIterator.HasMoreResults)
            {
                foreach (var item in await setIterator.ReadNextAsync(cancellationToken))
                {
                    userList.Add(item);
                }
            }
        }

        return userList;
    }

    public async Task<Domain.User.UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        try
        {
            return await _container.ReadItemAsync<Domain.User.UserEntity>(email, new PartitionKey(email), cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Domain.User.UserEntity> AddAsync(Domain.User.UserEntity user, CancellationToken cancellationToken) =>
        await _container.CreateItemAsync(user, new PartitionKey(user.Id), cancellationToken: cancellationToken);
}