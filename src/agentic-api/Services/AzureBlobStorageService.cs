using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace agentic_api.Services;

/// <summary>
/// Azure Blob Storage implementation for asset binary storage
/// </summary>
public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly HttpClient _httpClient;
    private bool _containerInitialized;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient, ILogger<AzureBlobStorageService> logger, HttpClient httpClient)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient("campaign-assets");
        _logger = logger;
        _httpClient = httpClient;
    }

    private async Task EnsureContainerExistsAsync(CancellationToken ct = default)
    {
        if (_containerInitialized) return;
        
        try
        {
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);
            _containerInitialized = true;
            _logger.LogInformation("Blob container 'campaign-assets' is ready");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ensure container exists - continuing anyway");
        }
    }

    public async Task<string> UploadAsync(string campaignId, string fileName, Stream content, string contentType, CancellationToken ct = default)
    {
        await EnsureContainerExistsAsync(ct);
        
        var blobName = $"{campaignId}/{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(content, options, ct);
        
        _logger.LogInformation("Uploaded blob {BlobName} for campaign {CampaignId}", blobName, campaignId);
        return blobClient.Uri.ToString();
    }

    public async Task<string> UploadFromUrlAsync(string campaignId, string fileName, string sourceUrl, CancellationToken ct = default)
    {
        // Download from source URL
        using var response = await _httpClient.GetAsync(sourceUrl, ct);
        response.EnsureSuccessStatusCode();
        
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        
        return await UploadAsync(campaignId, fileName, stream, contentType, ct);
    }

    public async Task<Stream?> DownloadAsync(string blobUrl, CancellationToken ct = default)
    {
        try
        {
            var blobName = GetBlobNameFromUrl(blobUrl);
            var blobClient = _containerClient.GetBlobClient(blobName);
            
            var response = await blobClient.DownloadStreamingAsync(cancellationToken: ct);
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download blob from {BlobUrl}", blobUrl);
            return null;
        }
    }

    public async Task DeleteAsync(string blobUrl, CancellationToken ct = default)
    {
        var blobName = GetBlobNameFromUrl(blobUrl);
        var blobClient = _containerClient.GetBlobClient(blobName);
        
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        _logger.LogInformation("Deleted blob {BlobName}", blobName);
    }

    public async Task DeleteByCampaignIdAsync(string campaignId, CancellationToken ct = default)
    {
        var prefix = $"{campaignId}/";
        
        await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: ct))
        {
            var blobClient = _containerClient.GetBlobClient(blobItem.Name);
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        }
        
        _logger.LogInformation("Deleted all blobs for campaign {CampaignId}", campaignId);
    }

    private static string GetBlobNameFromUrl(string blobUrl)
    {
        var uri = new Uri(blobUrl);
        // Remove container name from path (e.g., /campaign-assets/campaignId/file.png -> campaignId/file.png)
        var path = uri.AbsolutePath;
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("/", parts.Skip(1)); // Skip container name
    }
}
