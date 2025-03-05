using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Config;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Net;

namespace Infrastructure.Persistence;

public class TaskRepository : ITaskRepository
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;

    public TaskRepository(CosmosClient cosmosClient, CosmosDbSettings cosmosDbSettings)
    {
        _cosmosClient = cosmosClient;
        _container = _cosmosClient.GetContainer(cosmosDbSettings.DatabaseName, cosmosDbSettings.TaskContainerName);
    }

    public async Task<List<TaskItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<TaskItem> taskList = [];
        using (FeedIterator<TaskItem> setIterator =
                _container.GetItemLinqQueryable<TaskItem>()
                    .ToFeedIterator())
        {
            while (setIterator.HasMoreResults)
            {
                foreach (var item in await setIterator.ReadNextAsync(cancellationToken))
                {
                    taskList.Add(item);
                }
            }
        }

        return taskList;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid? id, CancellationToken cancellationToken)
    {
        try
        {
            var idString = id?.ToString();
            return await _container.ReadItemAsync<TaskItem>(idString, new PartitionKey(idString), cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken) =>
        await _container.CreateItemAsync(task, new PartitionKey(task.Id.ToString()), cancellationToken: cancellationToken);

    public async Task<TaskItem?> UpdateAsync(TaskItem task, CancellationToken cancellationToken)
    {
        var response = await _container.UpsertItemAsync(task, new PartitionKey(task.Id.ToString()), cancellationToken: cancellationToken);
        return response.StatusCode == HttpStatusCode.OK ? task : null;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var idString = id.ToString();
            var response = await _container.DeleteItemAsync<TaskItem>(idString, new PartitionKey(idString), cancellationToken: cancellationToken);
            return response.StatusCode == HttpStatusCode.NoContent;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}