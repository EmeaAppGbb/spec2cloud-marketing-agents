using System.Text.Json.Serialization;

namespace agentic_api.Models;

/// <summary>
/// Campaign entity stored in Cosmos DB
/// </summary>
public class Campaign
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("sessionId")]
    public required string SessionId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("brief")]
    public string? Brief { get; set; }

    [JsonPropertyName("status")]
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

    [JsonPropertyName("currentStep")]
    public string? CurrentStep { get; set; }

    [JsonPropertyName("plan")]
    public CampaignPlanData? Plan { get; set; }

    [JsonPropertyName("schedule")]
    public ScheduleData? Schedule { get; set; }

    [JsonPropertyName("publishResult")]
    public PublishResultData? PublishResult { get; set; }

    [JsonPropertyName("localizedContent")]
    public Dictionary<string, LocalizedContentData> LocalizedContent { get; set; } = [];

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum CampaignStatus
{
    Draft,
    InProgress,
    Completed,
    Published
}

/// <summary>
/// Campaign plan data (mirrors CampaignPlan from workflow)
/// </summary>
public class CampaignPlanData
{
    [JsonPropertyName("objectives")]
    public string Objectives { get; set; } = string.Empty;

    [JsonPropertyName("targetAudience")]
    public string TargetAudience { get; set; } = string.Empty;

    [JsonPropertyName("platforms")]
    public List<string> Platforms { get; set; } = ["Instagram", "TikTok"];

    [JsonPropertyName("timeline")]
    public string Timeline { get; set; } = "2 weeks";

    [JsonPropertyName("budget")]
    public string? Budget { get; set; }

    [JsonPropertyName("toneStyle")]
    public string ToneStyle { get; set; } = string.Empty;

    [JsonPropertyName("keyMessages")]
    public List<string> KeyMessages { get; set; } = [];
}

/// <summary>
/// Localized content for a specific market
/// </summary>
public class LocalizedContentData
{
    [JsonPropertyName("market")]
    public required string Market { get; set; }

    [JsonPropertyName("caption")]
    public required string Caption { get; set; }

    [JsonPropertyName("hashtags")]
    public List<string> Hashtags { get; set; } = [];
}

/// <summary>
/// Publishing schedule data
/// </summary>
public class ScheduleData
{
    [JsonPropertyName("publishDate")]
    public DateTime? PublishDate { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("frequency")]
    public string? Frequency { get; set; }
}

/// <summary>
/// Instagram publish result
/// </summary>
public class PublishResultData
{
    [JsonPropertyName("postId")]
    public string? PostId { get; set; }

    [JsonPropertyName("postUrl")]
    public string? PostUrl { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTime? PublishedAt { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
