using agentic_api.Models;

namespace agentic_api.Services;

/// <summary>
/// Repository interface for campaign data operations
/// </summary>
public interface ICampaignRepository
{
    Task<Campaign> CreateAsync(Campaign campaign, CancellationToken ct = default);
    Task<Campaign?> GetByIdAsync(string id, string sessionId, CancellationToken ct = default);
    Task<Campaign> UpdateAsync(Campaign campaign, CancellationToken ct = default);
    Task<IEnumerable<Campaign>> GetBySessionIdAsync(string sessionId, CampaignStatus? status = null, CancellationToken ct = default);
    Task DeleteAsync(string id, string sessionId, CancellationToken ct = default);
}
