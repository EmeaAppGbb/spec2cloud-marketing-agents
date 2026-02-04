using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Storage.Blobs;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using agentic_api.Services;
using agentic_api.Workflows;
using agentic_api.Models;
using Microsoft.Agents.AI.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configure Azure Monitor / Application Insights (only if connection string is available)
var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

builder.Services.AddHttpClient().AddLogging();
builder.Services.AddAGUI();

// Add HttpContextAccessor to access HTTP headers in workflows
builder.Services.AddHttpContextAccessor();

// Configure request timeout from configuration or use default
var timeoutSeconds = builder.Configuration.GetValue<int?>("RequestTimeoutSeconds") ?? 120;
builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(timeoutSeconds)
    };
});

// Add health checks with basic readiness check
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

string endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"]
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

string deploymentName = builder.Configuration["AZURE_OPENAI_DEPLOYMENT_NAME"]
    ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is not set.");

string imageDeploymentName = builder.Configuration["AZURE_IMAGE_MODEL_DEPLOYMENT_NAME"]
    ?? throw new InvalidOperationException("AZURE_IMAGE_MODEL_DEPLOYMENT_NAME is not set.");

// Configure DefaultAzureCredential with user-assigned managed identity if specified
var credentialOptions = new DefaultAzureCredentialOptions();
var managedIdentityClientId = builder.Configuration["AZURE_CLIENT_ID"];
if (!string.IsNullOrEmpty(managedIdentityClientId))
{
    credentialOptions.ManagedIdentityClientId = managedIdentityClientId;
}
var credential = new DefaultAzureCredential(credentialOptions);

// Register Cosmos DB client and repositories
string cosmosEndpoint = builder.Configuration["AZURE_COSMOS_ENDPOINT"]
    ?? throw new InvalidOperationException("AZURE_COSMOS_ENDPOINT is not set.");

builder.Services.AddSingleton(_ => new CosmosClient(cosmosEndpoint, credential, new CosmosClientOptions
{
    SerializerOptions = new CosmosSerializationOptions
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
    }
}));

builder.Services.AddSingleton<ICampaignRepository, CosmosCampaignRepository>();
builder.Services.AddSingleton<IAssetRepository, CosmosAssetRepository>();
builder.Services.AddSingleton<IWorkflowStateRepository, CosmosWorkflowStateRepository>();
builder.Services.AddSingleton<IConversationRepository, CosmosConversationRepository>();

// Register Blob Storage client and service
string storageEndpoint = builder.Configuration["AZURE_STORAGE_ENDPOINT"]
    ?? throw new InvalidOperationException("AZURE_STORAGE_ENDPOINT is not set.");

builder.Services.AddSingleton(_ => new BlobServiceClient(new Uri(storageEndpoint), credential));
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
builder.Services.AddSingleton<ICampaignPersistenceService, CampaignPersistenceService>();

// Register IChatClient
builder.Services.AddSingleton(_ =>
    new AzureOpenAIClient(new Uri(endpoint), credential)
        .GetChatClient(deploymentName)
        .AsIChatClient());

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Services.AddSingleton(_ =>
    new AzureOpenAIClient(new Uri(endpoint), credential)
        .GetImageClient(imageDeploymentName)
        .AsIImageGenerator());
#pragma warning restore MEAI001 // Ty

// Register the dummy workflow factory
builder.Services.AddSingleton<DummyWorkflowFactory>();

// Register the marketing workflow factory
builder.Services.AddSingleton<MarketingWorkflowFactory>();

builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

builder.AddWorkflow("DummyWorkflow", (sp, name) =>
{
    var factory = sp.GetRequiredService<DummyWorkflowFactory>();
    return factory.BuildWorkflow("DummyWorkflow");
}).AddAsAIAgent();

builder.AddWorkflow("MarketingWorkflow", (sp, name) =>
{
    var factory = sp.GetRequiredService<MarketingWorkflowFactory>();
    return factory.BuildWorkflow("MarketingWorkflow");
}).AddAsAIAgent();

var app = builder.Build();

// Add request timeouts
app.UseRequestTimeouts();

// Get the dummy workflow and convert it to an agent
var dummyWorkflowFactory = app.Services.GetRequiredService<DummyWorkflowFactory>();
var dummyWorkflow = dummyWorkflowFactory.BuildWorkflow("DummyWorkflow");
var dummyAgent = new AGUIWorkflowAgent(dummyWorkflow.AsAgent(name: "DummyWorkflow"));

// Get the marketing workflow and convert it to an agent
var marketingWorkflowFactory = app.Services.GetRequiredService<MarketingWorkflowFactory>();
var marketingWorkflow = marketingWorkflowFactory.BuildWorkflow("MarketingWorkflow");
var marketingAgent = new AGUIWorkflowAgent(marketingWorkflow.AsAgent(name: "MarketingWorkflow"));

app.MapOpenAIResponses();
app.MapOpenAIConversations();

// Map the dummy workflow agent to the default AGUI endpoint
app.MapAGUI("/", marketingAgent);

// Map health check endpoint
app.MapHealthChecks("/health");

// Campaign API endpoints
app.MapGet("/api/campaigns", async (
    string sessionId,
    string? status,
    ICampaignPersistenceService persistence,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    logger.LogInformation("Listing campaigns for sessionId: {SessionId}, status: {Status}", sessionId, status ?? "all");
    
    agentic_api.Models.CampaignStatus? statusFilter = status?.ToLowerInvariant() switch
    {
        "draft" => agentic_api.Models.CampaignStatus.Draft,
        "inprogress" => agentic_api.Models.CampaignStatus.InProgress,
        "completed" => agentic_api.Models.CampaignStatus.Completed,
        "published" => agentic_api.Models.CampaignStatus.Published,
        _ => null
    };
    
    var campaigns = await persistence.GetCampaignsBySessionAsync(sessionId, statusFilter, ct);
    var result = campaigns.Select(c => new
    {
        c.Id,
        c.Name,
        c.Status,
        c.CurrentStep,
        c.CreatedAt,
        c.UpdatedAt
    }).ToList();
    
    logger.LogInformation("Found {Count} campaigns for sessionId: {SessionId}. IDs: {Ids}", 
        result.Count, sessionId, string.Join(", ", result.Select(c => c.Id)));
    
    return Results.Ok(result);
});

app.MapGet("/api/campaigns/{campaignId}", async (
    string campaignId,
    string sessionId,
    ICampaignPersistenceService persistence,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    logger.LogInformation("Fetching campaign detail: campaignId={CampaignId}, sessionId={SessionId}", campaignId, sessionId);
    
    var campaign = await persistence.GetCampaignAsync(campaignId, sessionId, ct);
    if (campaign == null)
    {
        logger.LogWarning("Campaign not found: {CampaignId}", campaignId);
        return Results.NotFound();
    }

    var assets = await persistence.GetAssetsAsync(campaignId, ct);
    logger.LogInformation("Found {AssetCount} assets for campaign {CampaignId}", assets.Count(), campaignId);
    
    var conversation = await persistence.GetConversationAsync(campaignId, ct: ct);
    var checkpoint = await persistence.GetLatestCheckpointAsync(campaignId, ct);

    return Results.Ok(new
    {
        Campaign = campaign,
        Assets = assets,
        Conversation = conversation,
        Checkpoint = checkpoint
    });
});

// Debug endpoint to list all assets in database
app.MapGet("/api/debug/assets", async (
    CosmosClient cosmosClient,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    logger.LogInformation("Debug: Listing all assets in database");
    
    try
    {
        var container = cosmosClient.GetContainer("agentic-storage", "assets");
        var query = container.GetItemQueryIterator<AssetMetadata>("SELECT * FROM c");
        var results = new List<object>();
        
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(ct);
            foreach (var item in response)
            {
                results.Add(new { item.Id, item.CampaignId, item.Type, item.BlobUrl, item.CreatedAt });
            }
        }
        
        logger.LogInformation("Debug: Found {Count} total assets in database", results.Count);
        return Results.Ok(new { TotalAssets = results.Count, Assets = results });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Debug: Error listing assets");
        return Results.Problem(ex.Message);
    }
});

if (builder.Environment.IsDevelopment())
{
    // Map DevUI endpoint to /devui
    app.MapDevUI();
}


await app.RunAsync();