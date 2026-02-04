using agentic_api.Models;

namespace agentic_api.Services;

/// <summary>
/// Repository interface for asset metadata operations
/// </summary>
public interface IAssetRepository
{
    Task<AssetMetadata> CreateAsync(AssetMetadata asset, CancellationToken ct = default);
    Task<AssetMetadata?> GetByIdAsync(string id, string campaignId, CancellationToken ct = default);
    Task<AssetMetadata> UpdateAsync(AssetMetadata asset, CancellationToken ct = default);
    Task<IEnumerable<AssetMetadata>> GetByCampaignIdAsync(string campaignId, CancellationToken ct = default);
    Task DeleteAsync(string id, string campaignId, CancellationToken ct = default);
}
