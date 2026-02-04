using System.Text.Json.Serialization;

namespace agentic_api.Models;

/// <summary>
/// Asset metadata stored in Cosmos DB (actual binary in Blob Storage)
/// </summary>
public class AssetMetadata
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AssetType Type { get; set; }

    [JsonPropertyName("blobUrl")]
    public required string BlobUrl { get; set; }

    [JsonPropertyName("originalUrl")]
    public string? OriginalUrl { get; set; }

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }

    [JsonPropertyName("hashtags")]
    public List<string> Hashtags { get; set; } = [];

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("approvalStatus")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("previousVersionId")]
    public string? PreviousVersionId { get; set; }

    [JsonPropertyName("market")]
    public string? Market { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum AssetType
{
    Image,
    Video
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}
