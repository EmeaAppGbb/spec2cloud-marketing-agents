using agentic_api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace agentic_api.Services;

/// <summary>
/// Cosmos DB implementation of asset repository
/// </summary>
public class CosmosAssetRepository : IAssetRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosAssetRepository> _logger;

    public CosmosAssetRepository(CosmosClient cosmosClient, ILogger<CosmosAssetRepository> logger)
    {
        _container = cosmosClient.GetContainer("agentic-storage", "assets");
        _logger = logger;
    }

    public async Task<AssetMetadata> CreateAsync(AssetMetadata asset, CancellationToken ct = default)
    {
        asset.CreatedAt = DateTime.UtcNow;
        asset.UpdatedAt = DateTime.UtcNow;
        
        var response = await _container.CreateItemAsync(asset, new PartitionKey(asset.CampaignId), cancellationToken: ct);
        _logger.LogInformation("Created asset {AssetId} for campaign {CampaignId}", asset.Id, asset.CampaignId);
        return response.Resource;
    }

    public async Task<AssetMetadata?> GetByIdAsync(string id, string campaignId, CancellationToken ct = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<AssetMetadata>(id, new PartitionKey(campaignId), cancellationToken: ct);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<AssetMetadata> UpdateAsync(AssetMetadata asset, CancellationToken ct = default)
    {
        asset.UpdatedAt = DateTime.UtcNow;
        
        var response = await _container.ReplaceItemAsync(asset, asset.Id, new PartitionKey(asset.CampaignId), cancellationToken: ct);
        _logger.LogInformation("Updated asset {AssetId}", asset.Id);
        return response.Resource;
    }

    public async Task<IEnumerable<AssetMetadata>> GetByCampaignIdAsync(string campaignId, CancellationToken ct = default)
    {
        _logger.LogInformation("Querying assets for campaignId: {CampaignId}", campaignId);
        
        try
        {
            // Use partition key for efficient query
            var queryOptions = new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(campaignId)
            };
            
            var iterator = _container.GetItemLinqQueryable<AssetMetadata>(requestOptions: queryOptions)
                .Where(a => a.CampaignId == campaignId)
                .OrderByDescending(a => a.CreatedAt)
                .ToFeedIterator();

            var results = new List<AssetMetadata>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(ct);
                _logger.LogInformation("Query returned {Count} items in this batch for campaignId: {CampaignId}", response.Count, campaignId);
                results.AddRange(response);
            }

            _logger.LogInformation("Total {Count} assets found for campaignId: {CampaignId}", results.Count, campaignId);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying assets for campaignId: {CampaignId}", campaignId);
            return [];
        }
    }

    public async Task DeleteAsync(string id, string campaignId, CancellationToken ct = default)
    {
        await _container.DeleteItemAsync<AssetMetadata>(id, new PartitionKey(campaignId), cancellationToken: ct);
        _logger.LogInformation("Deleted asset {AssetId}", id);
    }
}
