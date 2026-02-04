using agentic_api.Models;

namespace agentic_api.Services;

/// <summary>
/// Repository interface for conversation history operations
/// </summary>
public interface IConversationRepository
{
    Task<ConversationMessage> CreateAsync(ConversationMessage message, CancellationToken ct = default);
    Task<IEnumerable<ConversationMessage>> GetByCampaignIdAsync(string campaignId, int? limit = null, CancellationToken ct = default);
    Task DeleteByCampaignIdAsync(string campaignId, CancellationToken ct = default);
}
