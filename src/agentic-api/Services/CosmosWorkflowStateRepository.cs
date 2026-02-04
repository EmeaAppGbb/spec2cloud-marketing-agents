using agentic_api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace agentic_api.Services;

/// <summary>
/// Cosmos DB implementation of workflow state repository
/// </summary>
public class CosmosWorkflowStateRepository : IWorkflowStateRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosWorkflowStateRepository> _logger;

    public CosmosWorkflowStateRepository(CosmosClient cosmosClient, ILogger<CosmosWorkflowStateRepository> logger)
    {
        _container = cosmosClient.GetContainer("agentic-storage", "workflow-state");
        _logger = logger;
    }

    public async Task<WorkflowCheckpoint> CreateAsync(WorkflowCheckpoint checkpoint, CancellationToken ct = default)
    {
        checkpoint.CreatedAt = DateTime.UtcNow;
        checkpoint.UpdatedAt = DateTime.UtcNow;
        
        var response = await _container.CreateItemAsync(checkpoint, new PartitionKey(checkpoint.CampaignId), cancellationToken: ct);
        _logger.LogInformation("Created checkpoint {CheckpointId} for campaign {CampaignId} at step {Step}", 
            checkpoint.Id, checkpoint.CampaignId, checkpoint.CurrentStep);
        return response.Resource;
    }

    public async Task<WorkflowCheckpoint?> GetLatestByCampaignIdAsync(string campaignId, CancellationToken ct = default)
    {
        var iterator = _container.GetItemLinqQueryable<WorkflowCheckpoint>()
            .Where(c => c.CampaignId == campaignId)
            .OrderByDescending(c => c.UpdatedAt)
            .Take(1)
            .ToFeedIterator();

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<WorkflowCheckpoint> UpdateAsync(WorkflowCheckpoint checkpoint, CancellationToken ct = default)
    {
        checkpoint.UpdatedAt = DateTime.UtcNow;
        
        var response = await _container.ReplaceItemAsync(checkpoint, checkpoint.Id, new PartitionKey(checkpoint.CampaignId), cancellationToken: ct);
        _logger.LogInformation("Updated checkpoint {CheckpointId} at step {Step}", checkpoint.Id, checkpoint.CurrentStep);
        return response.Resource;
    }

    public async Task<IEnumerable<WorkflowCheckpoint>> GetByCampaignIdAsync(string campaignId, CancellationToken ct = default)
    {
        var iterator = _container.GetItemLinqQueryable<WorkflowCheckpoint>()
            .Where(c => c.CampaignId == campaignId)
            .OrderBy(c => c.CreatedAt)
            .ToFeedIterator();

        var results = new List<WorkflowCheckpoint>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            results.AddRange(response);
        }

        return results;
    }
}
