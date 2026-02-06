using agentic_api.Models;
using agentic_api.Workflows;
using System.Text.Json;

namespace agentic_api.Services;

/// <summary>
/// High-level service for campaign persistence operations.
/// Provides a simplified API for workflow executors.
/// </summary>
public interface ICampaignPersistenceService
{
    // Campaign operations
    Task<Campaign> CreateCampaignAsync(string sessionId, string? brief = null, string? campaignId = null, CancellationToken ct = default);
    Task<Campaign?> GetCampaignAsync(string campaignId, string sessionId, CancellationToken ct = default);
    Task<Campaign?> GetActiveCampaignAsync(string sessionId, CancellationToken ct = default);
    Task<Campaign> UpdateCampaignAsync(Campaign campaign, CancellationToken ct = default);
    Task<IEnumerable<Campaign>> GetCampaignsBySessionAsync(string sessionId, CampaignStatus? status = null, CancellationToken ct = default);
    
    // Workflow state operations
    Task SaveCheckpointAsync(string campaignId, string step, string? stateJson, CancellationToken ct = default);
    Task<WorkflowCheckpoint?> GetLatestCheckpointAsync(string campaignId, CancellationToken ct = default);
    Task RecordHumanDecisionAsync(string campaignId, string gateName, string decision, string? feedback = null, CancellationToken ct = default);
    
    // Asset operations
    Task<AssetMetadata> SaveAssetAsync(string campaignId, string sourceUrl, AssetType type, string? caption = null, List<string>? hashtags = null, CancellationToken ct = default);
    Task<IEnumerable<AssetMetadata>> GetAssetsAsync(string campaignId, CancellationToken ct = default);
    Task UpdateAssetApprovalAsync(string assetId, string campaignId, ApprovalStatus status, CancellationToken ct = default);
    
    // Conversation operations
    Task SaveMessageAsync(string campaignId, MessageRole role, string content, string? imageUrl = null, CancellationToken ct = default);
    Task<IEnumerable<ConversationMessage>> GetConversationAsync(string campaignId, int? limit = null, CancellationToken ct = default);
}

public class CampaignPersistenceService : ICampaignPersistenceService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IWorkflowStateRepository _workflowStateRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<CampaignPersistenceService> _logger;

    public CampaignPersistenceService(
        ICampaignRepository campaignRepository,
        IAssetRepository assetRepository,
        IWorkflowStateRepository workflowStateRepository,
        IConversationRepository conversationRepository,
        IBlobStorageService blobStorageService,
        ILogger<CampaignPersistenceService> logger)
    {
        _campaignRepository = campaignRepository;
        _assetRepository = assetRepository;
        _workflowStateRepository = workflowStateRepository;
        _conversationRepository = conversationRepository;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    #region Campaign Operations

    public async Task<Campaign> CreateCampaignAsync(string sessionId, string? brief = null, string? campaignId = null, CancellationToken ct = default)
    {
        var campaign = new Campaign
        {
            Id = campaignId ?? Guid.NewGuid().ToString(),  // Use provided ID or generate new one
            SessionId = sessionId,
            Brief = brief,
            Name = GenerateCampaignName(brief),
            Status = CampaignStatus.Draft,
            CurrentStep = nameof(MarketingWorkflowSteps.CampaignPlanning)
        };

        var created = await _campaignRepository.CreateAsync(campaign, ct);
        _logger.LogInformation("Created campaign {CampaignId} for session {SessionId}", created.Id, sessionId);
        return created;
    }

    public async Task<Campaign?> GetCampaignAsync(string campaignId, string sessionId, CancellationToken ct = default)
    {
        return await _campaignRepository.GetByIdAsync(campaignId, sessionId, ct);
    }

    public async Task<Campaign?> GetActiveCampaignAsync(string sessionId, CancellationToken ct = default)
    {
        var campaigns = await _campaignRepository.GetBySessionIdAsync(sessionId, CampaignStatus.InProgress, ct);
        return campaigns.FirstOrDefault();
    }

    public async Task<Campaign> UpdateCampaignAsync(Campaign campaign, CancellationToken ct = default)
    {
        return await _campaignRepository.UpdateAsync(campaign, ct);
    }

    public async Task<IEnumerable<Campaign>> GetCampaignsBySessionAsync(string sessionId, CampaignStatus? status = null, CancellationToken ct = default)
    {
        return await _campaignRepository.GetBySessionIdAsync(sessionId, status, ct);
    }

    #endregion

    #region Workflow State Operations

    public async Task SaveCheckpointAsync(string campaignId, string step, string? stateJson, CancellationToken ct = default)
    {
        var existing = await _workflowStateRepository.GetLatestByCampaignIdAsync(campaignId, ct);
        
        if (existing != null)
        {
            if (!existing.CompletedSteps.Contains(existing.CurrentStep))
            {
                existing.CompletedSteps.Add(existing.CurrentStep);
            }
            existing.CurrentStep = step;
            existing.StateData = stateJson;
            existing.IsCompleted = step == nameof(MarketingWorkflowSteps.Completed);
            await _workflowStateRepository.UpdateAsync(existing, ct);
        }
        else
        {
            var checkpoint = new WorkflowCheckpoint
            {
                CampaignId = campaignId,
                WorkflowName = "MarketingWorkflow",
                CurrentStep = step,
                StateData = stateJson
            };
            await _workflowStateRepository.CreateAsync(checkpoint, ct);
        }

        _logger.LogInformation("Saved checkpoint for campaign {CampaignId} at step {Step}", campaignId, step);
    }

    public async Task<WorkflowCheckpoint?> GetLatestCheckpointAsync(string campaignId, CancellationToken ct = default)
    {
        return await _workflowStateRepository.GetLatestByCampaignIdAsync(campaignId, ct);
    }

    public async Task RecordHumanDecisionAsync(string campaignId, string gateName, string decision, string? feedback = null, CancellationToken ct = default)
    {
        var checkpoint = await _workflowStateRepository.GetLatestByCampaignIdAsync(campaignId, ct);
        if (checkpoint != null)
        {
            checkpoint.HumanGateDecisions.Add(new HumanGateDecision
            {
                GateName = gateName,
                Decision = decision,
                Feedback = feedback
            });
            await _workflowStateRepository.UpdateAsync(checkpoint, ct);
            _logger.LogInformation("Recorded human decision '{Decision}' for gate '{Gate}' in campaign {CampaignId}", 
                decision, gateName, campaignId);
        }
    }

    #endregion

    #region Asset Operations

    public async Task<AssetMetadata> SaveAssetAsync(
        string campaignId, 
        string sourceUrl, 
        AssetType type, 
        string? caption = null, 
        List<string>? hashtags = null, 
        CancellationToken ct = default)
    {
        // Upload to blob storage
        var extension = type == AssetType.Image ? "png" : "mp4";
        var fileName = $"{Guid.NewGuid()}.{extension}";
        
        _logger.LogInformation("Saving asset for campaign {CampaignId}: {SourceUrl}", campaignId, sourceUrl);
        
        string blobUrl;
        try
        {
            blobUrl = await _blobStorageService.UploadFromUrlAsync(campaignId, fileName, sourceUrl, ct);
            _logger.LogInformation("Successfully uploaded asset to blob storage: {BlobUrl}", blobUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to upload asset to blob storage for campaign {CampaignId}", campaignId);
            throw; // Don't fallback to storing raw data URIs — they exceed Cosmos DB's 2MB item limit
        }

        // Store only the blob URL reference in Cosmos, never the raw source (which may be a large data: URI)
        var originalUrlForCosmos = sourceUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            ? null  // data: URIs are too large for Cosmos — the blob is the canonical copy
            : sourceUrl;

        // Save metadata to Cosmos
        var asset = new AssetMetadata
        {
            CampaignId = campaignId,
            Type = type,
            BlobUrl = blobUrl,
            OriginalUrl = originalUrlForCosmos,
            Caption = caption,
            Hashtags = hashtags ?? []
        };

        var savedAsset = await _assetRepository.CreateAsync(asset, ct);
        _logger.LogInformation("Saved asset metadata to Cosmos DB: {AssetId} for campaign {CampaignId}", savedAsset.Id, campaignId);
        return savedAsset;
    }

    public async Task<IEnumerable<AssetMetadata>> GetAssetsAsync(string campaignId, CancellationToken ct = default)
    {
        return await _assetRepository.GetByCampaignIdAsync(campaignId, ct);
    }

    public async Task UpdateAssetApprovalAsync(string assetId, string campaignId, ApprovalStatus status, CancellationToken ct = default)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId, campaignId, ct);
        if (asset != null)
        {
            asset.ApprovalStatus = status;
            await _assetRepository.UpdateAsync(asset, ct);
            _logger.LogInformation("Updated asset {AssetId} approval status to {Status}", assetId, status);
        }
    }

    #endregion

    #region Conversation Operations

    public async Task SaveMessageAsync(string campaignId, MessageRole role, string content, string? imageUrl = null, CancellationToken ct = default)
    {
        var message = new ConversationMessage
        {
            CampaignId = campaignId,
            Role = role,
            Content = content,
            ImageUrl = imageUrl
        };

        await _conversationRepository.CreateAsync(message, ct);
    }

    public async Task<IEnumerable<ConversationMessage>> GetConversationAsync(string campaignId, int? limit = null, CancellationToken ct = default)
    {
        return await _conversationRepository.GetByCampaignIdAsync(campaignId, limit, ct);
    }

    #endregion

    #region Helpers

    private static string GenerateCampaignName(string? brief)
    {
        if (string.IsNullOrWhiteSpace(brief))
        {
            return $"Campaign {DateTime.UtcNow:MMdd-HHmm}";
        }

        // Extract first few meaningful words
        var words = brief.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(5)
            .Select(w => w.Trim(',', '.', '!', '?'));
        
        var name = string.Join(" ", words);
        return name.Length > 50 ? name[..50] + "..." : name;
    }

    #endregion
}
