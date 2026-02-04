using agentic_api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace agentic_api.Services;

/// <summary>
/// Cosmos DB implementation of conversation repository
/// </summary>
public class CosmosConversationRepository : IConversationRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosConversationRepository> _logger;

    public CosmosConversationRepository(CosmosClient cosmosClient, ILogger<CosmosConversationRepository> logger)
    {
        _container = cosmosClient.GetContainer("agentic-storage", "conversations");
        _logger = logger;
    }

    public async Task<ConversationMessage> CreateAsync(ConversationMessage message, CancellationToken ct = default)
    {
        message.Timestamp = DateTime.UtcNow;
        
        var response = await _container.CreateItemAsync(message, new PartitionKey(message.CampaignId), cancellationToken: ct);
        _logger.LogDebug("Created message {MessageId} for campaign {CampaignId}", message.Id, message.CampaignId);
        return response.Resource;
    }

    public async Task<IEnumerable<ConversationMessage>> GetByCampaignIdAsync(string campaignId, int? limit = null, CancellationToken ct = default)
    {
        var queryable = _container.GetItemLinqQueryable<ConversationMessage>()
            .Where(m => m.CampaignId == campaignId)
            .OrderBy(m => m.Timestamp);

        if (limit.HasValue)
        {
            queryable = (IOrderedQueryable<ConversationMessage>)queryable.Take(limit.Value);
        }

        var iterator = queryable.ToFeedIterator();
        var results = new List<ConversationMessage>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            results.AddRange(response);
        }

        return results;
    }

    public async Task DeleteByCampaignIdAsync(string campaignId, CancellationToken ct = default)
    {
        var messages = await GetByCampaignIdAsync(campaignId, ct: ct);
        
        foreach (var message in messages)
        {
            await _container.DeleteItemAsync<ConversationMessage>(message.Id, new PartitionKey(campaignId), cancellationToken: ct);
        }
        
        _logger.LogInformation("Deleted all messages for campaign {CampaignId}", campaignId);
    }
}
