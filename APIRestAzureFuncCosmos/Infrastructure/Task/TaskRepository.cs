using Domain.Task;
using Domain.Task.Interfaces;
using Infrastructure.Config;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Net;

namespace Infrastructure.Task;

public class TaskRepository : ITaskRepository
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;

    public TaskRepository(CosmosClient cosmosClient, CosmosDbSettings cosmosDbSettings)
    {
        _cosmosClient = cosmosClient;
        _container = _cosmosClient.GetContainer(cosmosDbSettings.DatabaseName, cosmosDbSettings.TaskContainerName);
    }

    public async Task<List<TaskEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<TaskEntity> taskList = [];
        using (FeedIterator<TaskEntity> setIterator =
                _container.GetItemLinqQueryable<TaskEntity>()
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

    public async Task<TaskEntity?> GetByIdAsync(Guid? id, CancellationToken cancellationToken)
    {
        try
        {
            var idString = id?.ToString();
            return await _container.ReadItemAsync<TaskEntity>(idString, new PartitionKey(idString), cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<TaskEntity>> GetAssignedToAnUserAsync(string email, CancellationToken cancellationToken)
    {
        var taskList = new List<TaskEntity>();

        using (FeedIterator<TaskEntity> iterator = _container
            .GetItemLinqQueryable<TaskEntity>()
            .Where(x => x.AssignedUserEmail == email.Trim())
            .ToFeedIterator())
        {
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                taskList.AddRange(response);
            }
        }

        return taskList;
    }

    public async Task<TaskEntity> AddAsync(TaskEntity task, CancellationToken cancellationToken) =>
        await _container.CreateItemAsync(task, new PartitionKey(task.Id.ToString()), cancellationToken: cancellationToken);

    public async Task<TaskEntity?> UpdateAsync(TaskEntity task, CancellationToken cancellationToken)
    {
        var response = await _container.UpsertItemAsync(task, new PartitionKey(task.Id.ToString()), cancellationToken: cancellationToken);
        return response.StatusCode == HttpStatusCode.OK ? task : null;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var idString = id.ToString();
            var response = await _container.DeleteItemAsync<TaskEntity>(idString, new PartitionKey(idString), cancellationToken: cancellationToken);
            return response.StatusCode == HttpStatusCode.NoContent;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}