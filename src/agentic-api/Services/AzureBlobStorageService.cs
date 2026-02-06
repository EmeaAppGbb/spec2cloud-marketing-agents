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
            // Container is provisioned by infrastructure (Bicep) - just verify it exists.
            // Avoid CreateIfNotExistsAsync which issues a PUT that may require elevated permissions.
            if (await _containerClient.ExistsAsync(ct))
            {
                _containerInitialized = true;
                return;
            }

            _logger.LogError("Blob container 'campaign-assets' does not exist. Ensure infrastructure has been provisioned (azd provision).");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to verify container existence - continuing anyway");
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
        // Handle data: URIs (base64-encoded content from AI image generation)
        if (sourceUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return await UploadFromDataUriAsync(campaignId, fileName, sourceUrl, ct);
        }

        // Download from HTTP/HTTPS source URL
        using var response = await _httpClient.GetAsync(sourceUrl, ct);
        response.EnsureSuccessStatusCode();
        
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        
        return await UploadAsync(campaignId, fileName, stream, contentType, ct);
    }

    private async Task<string> UploadFromDataUriAsync(string campaignId, string fileName, string dataUri, CancellationToken ct)
    {
        // Parse data:[<mediatype>][;base64],<data>
        var commaIndex = dataUri.IndexOf(',');
        if (commaIndex < 0)
            throw new ArgumentException("Invalid data URI: missing comma separator.");

        var header = dataUri[..commaIndex]; // e.g. "data:image/png;base64"
        var base64Data = dataUri[(commaIndex + 1)..];

        var contentType = "application/octet-stream";
        if (header.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var meta = header[5..]; // strip "data:"
            var semiIndex = meta.IndexOf(';');
            if (semiIndex > 0)
                contentType = meta[..semiIndex];
            else if (meta.Length > 0)
                contentType = meta;
        }

        var bytes = Convert.FromBase64String(base64Data);
        await using var stream = new MemoryStream(bytes);

        _logger.LogInformation("Uploading data URI asset ({ContentType}, {Size} bytes) for campaign {CampaignId}",
            contentType, bytes.Length, campaignId);

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
