using System.Text.Json.Serialization;

namespace agentic_api.Models;

/// <summary>
/// Conversation message for chat history persistence
/// </summary>
public class ConversationMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; set; }

    [JsonPropertyName("role")]
    public required MessageRole Role { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = [];

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum MessageRole
{
    User,
    Assistant,
    System
}
