using agentic_api.Models;

namespace agentic_api.Services;

/// <summary>
/// Repository interface for workflow state operations
/// </summary>
public interface IWorkflowStateRepository
{
    Task<WorkflowCheckpoint> CreateAsync(WorkflowCheckpoint checkpoint, CancellationToken ct = default);
    Task<WorkflowCheckpoint?> GetLatestByCampaignIdAsync(string campaignId, CancellationToken ct = default);
    Task<WorkflowCheckpoint> UpdateAsync(WorkflowCheckpoint checkpoint, CancellationToken ct = default);
    Task<IEnumerable<WorkflowCheckpoint>> GetByCampaignIdAsync(string campaignId, CancellationToken ct = default);
}
