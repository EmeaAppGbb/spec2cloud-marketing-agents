using System.Text.Json.Serialization;

namespace agentic_api.Models;

/// <summary>
/// Workflow state checkpoint for resume functionality
/// </summary>
public class WorkflowCheckpoint
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; set; }

    [JsonPropertyName("workflowName")]
    public required string WorkflowName { get; set; }

    [JsonPropertyName("currentStep")]
    public required string CurrentStep { get; set; }

    [JsonPropertyName("completedSteps")]
    public List<string> CompletedSteps { get; set; } = [];

    [JsonPropertyName("stateData")]
    public string? StateData { get; set; }

    [JsonPropertyName("humanGateDecisions")]
    public List<HumanGateDecision> HumanGateDecisions { get; set; } = [];

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Record of a human-in-the-loop decision
/// </summary>
public class HumanGateDecision
{
    [JsonPropertyName("gateName")]
    public required string GateName { get; set; }

    [JsonPropertyName("decision")]
    public required string Decision { get; set; }

    [JsonPropertyName("feedback")]
    public string? Feedback { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
