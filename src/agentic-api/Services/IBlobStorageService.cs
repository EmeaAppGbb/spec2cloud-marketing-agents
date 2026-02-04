namespace agentic_api.Services;

/// <summary>
/// Service interface for blob storage operations
/// </summary>
public interface IBlobStorageService
{
    Task<string> UploadAsync(string campaignId, string fileName, Stream content, string contentType, CancellationToken ct = default);
    Task<string> UploadFromUrlAsync(string campaignId, string fileName, string sourceUrl, CancellationToken ct = default);
    Task<Stream?> DownloadAsync(string blobUrl, CancellationToken ct = default);
    Task DeleteAsync(string blobUrl, CancellationToken ct = default);
    Task DeleteByCampaignIdAsync(string campaignId, CancellationToken ct = default);
}
