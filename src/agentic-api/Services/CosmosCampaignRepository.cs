using agentic_api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace agentic_api.Services;

/// <summary>
/// Cosmos DB implementation of campaign repository
/// </summary>
public class CosmosCampaignRepository : ICampaignRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosCampaignRepository> _logger;

    public CosmosCampaignRepository(CosmosClient cosmosClient, ILogger<CosmosCampaignRepository> logger)
    {
        _container = cosmosClient.GetContainer("agentic-storage", "campaigns");
        _logger = logger;
    }

    public async Task<Campaign> CreateAsync(Campaign campaign, CancellationToken ct = default)
    {
        campaign.CreatedAt = DateTime.UtcNow;
        campaign.UpdatedAt = DateTime.UtcNow;
        
        var response = await _container.CreateItemAsync(campaign, new PartitionKey(campaign.SessionId), cancellationToken: ct);
        _logger.LogInformation("Created campaign {CampaignId} for session {SessionId}", campaign.Id, campaign.SessionId);
        return response.Resource;
    }

    public async Task<Campaign?> GetByIdAsync(string id, string sessionId, CancellationToken ct = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<Campaign>(id, new PartitionKey(sessionId), cancellationToken: ct);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Campaign> UpdateAsync(Campaign campaign, CancellationToken ct = default)
    {
        campaign.UpdatedAt = DateTime.UtcNow;
        
        var response = await _container.ReplaceItemAsync(campaign, campaign.Id, new PartitionKey(campaign.SessionId), cancellationToken: ct);
        _logger.LogInformation("Updated campaign {CampaignId}", campaign.Id);
        return response.Resource;
    }

    public async Task<IEnumerable<Campaign>> GetBySessionIdAsync(string sessionId, CampaignStatus? status = null, CancellationToken ct = default)
    {
        var queryable = _container.GetItemLinqQueryable<Campaign>()
            .Where(c => c.SessionId == sessionId);

        if (status.HasValue)
        {
            queryable = queryable.Where(c => c.Status == status.Value);
        }

        var iterator = queryable.OrderByDescending(c => c.UpdatedAt).ToFeedIterator();
        var results = new List<Campaign>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            results.AddRange(response);
        }

        return results;
    }

    public async Task DeleteAsync(string id, string sessionId, CancellationToken ct = default)
    {
        await _container.DeleteItemAsync<Campaign>(id, new PartitionKey(sessionId), cancellationToken: ct);
        _logger.LogInformation("Deleted campaign {CampaignId}", id);
    }
}
