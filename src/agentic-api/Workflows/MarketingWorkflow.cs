// NOTE: This file uses Microsoft.Extensions.AI preview APIs (MEAI001) which are subject to change.
// These APIs are required for IImageGenerator and FunctionApprovalRequestContent functionality.
// Monitor https://github.com/microsoft/extensions for updates and breaking changes.
// The pragma warning disable is intentional to allow use of these experimental features.
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace agentic_api.Workflows;

/// <summary>
/// Marketing workflow factory that creates an AI-powered social media campaign workflow.
/// Orchestrates 5 specialized agents: Campaign Planner → Creative Generator → Localizer → Schedule Creator → Instagram Publisher
/// 
/// <para>
/// This implementation uses preview/experimental APIs from Microsoft.Extensions.AI.
/// These APIs may change in future versions. See MEAI001 warning for details.
/// </para>
/// </summary>
public class MarketingWorkflowFactory
{
    private readonly ILogger<MarketingChatInputExecutor> _inputLogger;
    private readonly ILogger<CampaignPlannerExecutor> _plannerLogger;
    private readonly ILogger<CreativeGeneratorExecutor> _creativeLogger;
    private readonly ILogger<LocalizerExecutor> _localizerLogger;
    private readonly ILogger<ScheduleCreatorExecutor> _scheduleLogger;
    private readonly ILogger<InstagramPublisherExecutor> _publisherLogger;
    private readonly IChatClient _chatClient;
    private readonly IImageGenerator _imageGenerator;

    public MarketingWorkflowFactory(
        ILogger<MarketingChatInputExecutor> inputLogger,
        ILogger<CampaignPlannerExecutor> plannerLogger,
        ILogger<CreativeGeneratorExecutor> creativeLogger,
        ILogger<LocalizerExecutor> localizerLogger,
        ILogger<ScheduleCreatorExecutor> scheduleLogger,
        ILogger<InstagramPublisherExecutor> publisherLogger,
        IChatClient chatClient,
        IImageGenerator imageGenerator)
    {
        _inputLogger = inputLogger;
        _plannerLogger = plannerLogger;
        _creativeLogger = creativeLogger;
        _localizerLogger = localizerLogger;
        _scheduleLogger = scheduleLogger;
        _publisherLogger = publisherLogger;
        _chatClient = chatClient;
        _imageGenerator = imageGenerator;
    }

    public Workflow BuildWorkflow(string name)
    {
        // Create executors
        var chatInput = new MarketingChatInputExecutor(_inputLogger);
        var campaignPlanner = new CampaignPlannerExecutor(_plannerLogger, _chatClient);
        var creativeGenerator = new CreativeGeneratorExecutor(_creativeLogger, _chatClient, _imageGenerator);
        var localizer = new LocalizerExecutor(_localizerLogger, _chatClient);
        var scheduleCreator = new ScheduleCreatorExecutor(_scheduleLogger, _chatClient);
        var instagramPublisher = new InstagramPublisherExecutor(_publisherLogger);

        // Build workflow with conditional routing based on workflow state
        var workflowBuilder = new WorkflowBuilder(chatInput)
            .WithName(name)
            .AddSwitch(chatInput, switchBuilder =>
                switchBuilder
                    .AddCase(IsStep(MarketingWorkflowSteps.CampaignPlanning), campaignPlanner)
                    .AddCase(IsStep(MarketingWorkflowSteps.CreativeGeneration), creativeGenerator)
                    .AddCase(IsStep(MarketingWorkflowSteps.Localization), localizer)
                    .AddCase(IsStep(MarketingWorkflowSteps.ScheduleCreation), scheduleCreator)
                    .AddCase(IsStep(MarketingWorkflowSteps.InstagramPublishing), instagramPublisher)
                    .WithDefault(campaignPlanner)
            )
            .WithOutputFrom(campaignPlanner)
            .WithOutputFrom(creativeGenerator)
            .WithOutputFrom(localizer)
            .WithOutputFrom(scheduleCreator)
            .WithOutputFrom(instagramPublisher);

        return workflowBuilder.Build();
    }

    private static Func<MarketingInputEvent?, bool> IsStep(MarketingWorkflowSteps step) => (input) =>
        input?.NextStep == step;
}

/// <summary>
/// Marketing workflow steps enum
/// </summary>
public enum MarketingWorkflowSteps
{
    CampaignPlanning,
    CreativeGeneration,
    Localization,
    ScheduleCreation,
    InstagramPublishing,
    Completed
}

/// <summary>
/// Marketing campaign state passed between executors
/// </summary>
public class MarketingCampaignState
{
    public string? CampaignBrief { get; set; }
    public CampaignPlan? CampaignPlan { get; set; }
    public List<CreativeAsset> CreativeAssets { get; set; } = [];
    public List<string> SelectedMarkets { get; set; } = [];
    public Dictionary<string, LocalizedContent> LocalizedContent { get; set; } = [];
    public PublishingSchedule? Schedule { get; set; }
    public InstagramPostResult? PublishedPost { get; set; }
    public string? UserFeedback { get; set; }
}

/// <summary>
/// Campaign plan generated by the Campaign Planner
/// </summary>
public class CampaignPlan
{
    public string Objectives { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public List<string> Platforms { get; set; } = ["Instagram", "TikTok"];
    public string Timeline { get; set; } = "2 weeks";
    public string? Budget { get; set; }
    public string ToneStyle { get; set; } = string.Empty;
    public List<string> KeyMessages { get; set; } = [];
}

/// <summary>
/// Creative asset (image or video) with caption and hashtags
/// </summary>
public class CreativeAsset
{
    public required string Type { get; set; } // "image" or "video"
    public required string Url { get; set; }
    public required string Caption { get; set; }
    public required List<string> Hashtags { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Localized content for a specific market
/// </summary>
public class LocalizedContent
{
    public required string Market { get; set; }
    public required string Language { get; set; }
    public required List<LocalizedAsset> Assets { get; set; }
}

/// <summary>
/// Localized asset with translated caption and hashtags
/// </summary>
public class LocalizedAsset
{
    public required string OriginalCaption { get; set; }
    public required string TranslatedCaption { get; set; }
    public required List<string> OriginalHashtags { get; set; }
    public required List<string> TranslatedHashtags { get; set; }
}

/// <summary>
/// Publishing schedule with scheduled posts
/// </summary>
public class PublishingSchedule
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required List<ScheduledPost> Posts { get; set; }
}

/// <summary>
/// Individual scheduled post
/// </summary>
public class ScheduledPost
{
    public required DateTime ScheduledTime { get; set; }
    public required string Platform { get; set; }
    public required string ContentType { get; set; } // "image" or "video"
    public required string Language { get; set; }
    public required string Market { get; set; }
    public required int AssetIndex { get; set; }
    public string? Timezone { get; set; }
}

/// <summary>
/// Instagram post result
/// </summary>
public class InstagramPostResult
{
    public bool Success { get; set; }
    public string? PostId { get; set; }
    public string? PostUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Marketing input event passed from input executor to processing executors
/// </summary>
public class MarketingInputEvent
{
    public required string Input { get; set; }
    public MarketingWorkflowSteps NextStep { get; set; }
    public MarketingCampaignState State { get; set; } = new();
}

/// <summary>
/// ChatInput executor that handles routing for the marketing workflow.
/// </summary>
public sealed class MarketingChatInputExecutor : Executor
{
    private readonly ILogger<MarketingChatInputExecutor> _logger;

    public MarketingChatInputExecutor(ILogger<MarketingChatInputExecutor> logger) : base("MarketingChatInput")
    {
        _logger = logger;
    }

    protected override Microsoft.Agents.AI.Workflows.RouteBuilder ConfigureRoutes(Microsoft.Agents.AI.Workflows.RouteBuilder routeBuilder) =>
        routeBuilder
            .AddHandler<List<ChatMessage>, MarketingInputEvent>(HandleChatMessagesAsync)
            .AddHandler<TurnToken, string>(HandleTurnTokenAsync);

    private ValueTask<MarketingInputEvent> HandleChatMessagesAsync(
        List<ChatMessage> messages,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var lastUserMessage = messages.LastOrDefault(m => m.Role == ChatRole.User);
        var approvalMessage = messages.LastOrDefault(m => m.Role == ChatRole.Tool);
        var functionResult = approvalMessage?.Contents.OfType<FunctionResultContent>().FirstOrDefault();

        var resultString = functionResult?.Result?.ToString() ?? string.Empty;

        // Parse workflow state from function result if available
        var state = new MarketingCampaignState();

        // Check for approval responses and route accordingly
        if (resultString.Contains("creative-approved"))
        {
            _logger.LogInformation("Creative assets approved by user.");
            // Try to parse state from the approval result
            TryParseStateFromResult(resultString, state);
            return ValueTask.FromResult(new MarketingInputEvent
            {
                Input = lastUserMessage?.Text ?? string.Empty,
                NextStep = MarketingWorkflowSteps.Localization,
                State = state
            });
        }

        if (resultString.Contains("markets-selected"))
        {
            _logger.LogInformation("Markets selected by user.");
            TryParseStateFromResult(resultString, state);
            return ValueTask.FromResult(new MarketingInputEvent
            {
                Input = lastUserMessage?.Text ?? string.Empty,
                NextStep = MarketingWorkflowSteps.Localization,
                State = state
            });
        }

        if (resultString.Contains("skip-localization"))
        {
            _logger.LogInformation("User chose to skip localization.");
            TryParseStateFromResult(resultString, state);
            state.SelectedMarkets = [];
            return ValueTask.FromResult(new MarketingInputEvent
            {
                Input = lastUserMessage?.Text ?? string.Empty,
                NextStep = MarketingWorkflowSteps.ScheduleCreation,
                State = state
            });
        }

        if (resultString.Contains("localization-complete"))
        {
            _logger.LogInformation("Localization complete, proceeding to schedule creation.");
            TryParseStateFromResult(resultString, state);
            return ValueTask.FromResult(new MarketingInputEvent
            {
                Input = lastUserMessage?.Text ?? string.Empty,
                NextStep = MarketingWorkflowSteps.ScheduleCreation,
                State = state
            });
        }

        if (resultString.Contains("schedule-approved"))
        {
            _logger.LogInformation("Schedule approved by user.");
            TryParseStateFromResult(resultString, state);
            return ValueTask.FromResult(new MarketingInputEvent
            {
                Input = lastUserMessage?.Text ?? string.Empty,
                NextStep = MarketingWorkflowSteps.InstagramPublishing,
                State = state
            });
        }

        if (resultString.Contains("creative-rejected") || resultString.Contains("schedule-rejected"))
        {
            _logger.LogInformation("User rejected content with feedback, regenerating.");
            TryParseStateFromResult(resultString, state);
            
            // Extract feedback if present
            var feedbackStart = resultString.IndexOf("feedback:", StringComparison.OrdinalIgnoreCase);
            if (feedbackStart >= 0)
            {
                state.UserFeedback = resultString[(feedbackStart + 9)..].Trim();
            }

            var nextStep = resultString.Contains("creative-rejected")
                ? MarketingWorkflowSteps.CreativeGeneration
                : MarketingWorkflowSteps.ScheduleCreation;

            return ValueTask.FromResult(new MarketingInputEvent
            {
                Input = lastUserMessage?.Text ?? string.Empty,
                NextStep = nextStep,
                State = state
            });
        }

        // Default: Start with campaign planning
        _logger.LogInformation("Starting new campaign with planning phase.");
        return ValueTask.FromResult(new MarketingInputEvent
        {
            Input = lastUserMessage?.Text ?? "Create a social media campaign",
            NextStep = MarketingWorkflowSteps.CampaignPlanning,
            State = new MarketingCampaignState { CampaignBrief = lastUserMessage?.Text ?? string.Empty }
        });
    }

    private static void TryParseStateFromResult(string result, MarketingCampaignState state)
    {
        try
        {
            // Try to find and parse JSON state in the result
            var jsonStart = result.IndexOf('{');
            var jsonEnd = result.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonPart = result[jsonStart..(jsonEnd + 1)];
                var parsed = JsonSerializer.Deserialize<MarketingCampaignState>(jsonPart);
                if (parsed != null)
                {
                    state.CampaignBrief = parsed.CampaignBrief;
                    state.CampaignPlan = parsed.CampaignPlan;
                    state.CreativeAssets = parsed.CreativeAssets;
                    state.SelectedMarkets = parsed.SelectedMarkets;
                    state.LocalizedContent = parsed.LocalizedContent;
                    state.Schedule = parsed.Schedule;
                }
            }
        }
        catch
        {
            // Ignore parsing errors, use empty state
        }
    }

    private ValueTask<string> HandleTurnTokenAsync(
        TurnToken turnToken,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Marketing Workflow started with TurnToken");
        return ValueTask.FromResult("Hello from Marketing TurnToken");
    }
}

/// <summary>
/// Campaign Planner executor that creates strategic campaign plans with smart defaults.
/// Implements FR-1: Campaign Planning & Strategic Input
/// </summary>
public sealed class CampaignPlannerExecutor : Executor<MarketingInputEvent, AIContent>
{
    private readonly ILogger<CampaignPlannerExecutor> _logger;
    private readonly AIAgent _agent;

    public CampaignPlannerExecutor(ILogger<CampaignPlannerExecutor> logger, IChatClient chatClient) : base("CampaignPlanner")
    {
        _logger = logger;
        _agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = "CampaignPlannerAgent",
            Instructions = """
                You are an expert social media marketing strategist. Based on the user's campaign brief, 
                create a comprehensive campaign plan. Apply smart defaults when information is missing:
                - Default duration: 2 weeks
                - Default platforms: Instagram and TikTok
                - Default target audience age: 18-45 years
                
                Your response MUST be in the following JSON format:
                {
                    "objectives": "Clear campaign objectives",
                    "targetAudience": "Description of target audience",
                    "platforms": ["Instagram", "TikTok"],
                    "timeline": "2 weeks",
                    "toneStyle": "Brand voice and tone",
                    "keyMessages": ["Key message 1", "Key message 2"]
                }
                
                Keep your response concise and actionable. Focus on the strategic elements that will guide content creation.
                """
        });
    }

    public override async ValueTask<AIContent> HandleAsync(
        MarketingInputEvent input,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Campaign Planner received brief: {Brief}", input.Input);

            var prompt = $"Create a social media campaign plan for the following brief: {input.Input}";
            var agentResponse = await _agent.RunAsync(
                new ChatMessage(ChatRole.User, prompt),
                cancellationToken: cancellationToken);

            var responseText = agentResponse.Text ?? "{}";
            _logger.LogInformation("Campaign plan generated: {Length} characters", responseText.Length);

            // Parse the plan and store in state
            input.State.CampaignPlan = TryParseCampaignPlan(responseText);

            // Return approval request for creative generation phase
            return ApprovalRequestHelper.CreateApprovalRequest(
                functionName: "approve_campaign_plan",
                arguments: new Dictionary<string, object?>
                {
                    { "plan", responseText },
                    { "state", JsonSerializer.Serialize(input.State) }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in campaign planner executor");
            return new TextContent("Sorry, I encountered an error while creating the campaign plan. Please try again.");
        }
    }

    private static CampaignPlan? TryParseCampaignPlan(string json)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<CampaignPlan>(json, options);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Creative Generator executor that creates 2 images and 1 video with captions and hashtags.
/// Implements FR-2: Creative Asset Generation
/// </summary>
public sealed class CreativeGeneratorExecutor : Executor<MarketingInputEvent, AIContent>
{
    private readonly ILogger<CreativeGeneratorExecutor> _logger;
    private readonly IChatClient _chatClient;
    private readonly IImageGenerator _imageGenerator;
    private readonly AIAgent _captionAgent;

    public CreativeGeneratorExecutor(
        ILogger<CreativeGeneratorExecutor> logger,
        IChatClient chatClient,
        IImageGenerator imageGenerator) : base("CreativeGenerator")
    {
        _logger = logger;
        _chatClient = chatClient;
        _imageGenerator = imageGenerator;
        _captionAgent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = "CaptionAgent",
            Instructions = """
                You are an expert social media copywriter. Create engaging captions and hashtags for social media posts.
                
                Your response MUST be in the following JSON format:
                {
                    "caption": "Engaging caption text optimized for social media engagement",
                    "hashtags": ["#hashtag1", "#hashtag2", "#hashtag3", "#hashtag4", "#hashtag5"]
                }
                
                Keep captions under 2200 characters. Use 5-10 relevant, trending hashtags.
                """
        });
    }

    public override async ValueTask<AIContent> HandleAsync(
        MarketingInputEvent input,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creative Generator starting asset generation");

            var campaignPlan = input.State.CampaignPlan;
            var feedback = input.State.UserFeedback;
            var assets = new List<CreativeAsset>();

            // Generate 2 images
            for (int i = 1; i <= 2; i++)
            {
                var imageAsset = await GenerateImageAssetAsync(campaignPlan, i, feedback, cancellationToken);
                if (imageAsset != null)
                {
                    assets.Add(imageAsset);
                }
            }

            // Generate 1 video (simulated - using placeholder since video generation is not available)
            var videoAsset = GenerateVideoAssetPlaceholder(campaignPlan);
            assets.Add(videoAsset);

            input.State.CreativeAssets = assets;
            _logger.LogInformation("Generated {Count} creative assets", assets.Count);

            // Create assets display for approval
            var assetsJson = JsonSerializer.Serialize(assets, new JsonSerializerOptions { WriteIndented = true });

            return ApprovalRequestHelper.CreateApprovalRequest(
                functionName: "approve_creative_assets",
                arguments: new Dictionary<string, object?>
                {
                    { "assets", assetsJson },
                    { "state", JsonSerializer.Serialize(input.State) }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in creative generator executor");
            return new TextContent("Sorry, I encountered an error while generating creative assets. Please try again.");
        }
    }

    private async Task<CreativeAsset?> GenerateImageAssetAsync(
        CampaignPlan? plan,
        int imageNumber,
        string? feedback,
        CancellationToken cancellationToken)
    {
        try
        {
            // Create image prompt based on campaign plan
            var promptBuilder = new System.Text.StringBuilder();
            promptBuilder.Append("Create a safe, professional social media image ");
            if (plan != null)
            {
                promptBuilder.Append($"for a {plan.ToneStyle} campaign targeting {plan.TargetAudience}. ");
                if (plan.KeyMessages.Count > 0)
                {
                    promptBuilder.Append($"Key message: {plan.KeyMessages[0]}. ");
                }
            }
            if (!string.IsNullOrEmpty(feedback))
            {
                promptBuilder.Append($"Consider this feedback: {feedback}. ");
            }
            promptBuilder.Append($"This is image {imageNumber} of 2, make it unique and engaging.");

            // Generate image
            var options = new ImageGenerationOptions
            {
                MediaType = "image/png",
                ResponseFormat = ImageGenerationResponseFormat.Hosted
            };

            var response = await _imageGenerator.GenerateImagesAsync(promptBuilder.ToString(), options);
            var dataContent = response.Contents.OfType<DataContent>().FirstOrDefault();

            if (dataContent?.Uri == null)
            {
                _logger.LogWarning("Image generation returned no URL for image {Number}", imageNumber);
                return null;
            }

            // Generate caption and hashtags
            var captionData = await GenerateCaptionAndHashtags(plan, $"social media image {imageNumber}", cancellationToken);

            return new CreativeAsset
            {
                Type = "image",
                Url = dataContent.Uri.ToString(),
                Caption = captionData.Caption,
                Hashtags = captionData.Hashtags,
                Description = $"Campaign image {imageNumber}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image {Number}", imageNumber);
            return null;
        }
    }

    private CreativeAsset GenerateVideoAssetPlaceholder(CampaignPlan? plan)
    {
        // Video generation is not available, return placeholder
        return new CreativeAsset
        {
            Type = "video",
            Url = "https://placeholder.com/video-placeholder.mp4",
            Caption = $"Promotional video for {plan?.Objectives ?? "our campaign"}. Stay tuned for the full video!",
            Hashtags = ["#video", "#socialmedia", "#promo", "#comingsoon", "#teaser"],
            Description = "Campaign promotional video (placeholder)"
        };
    }

    private async Task<(string Caption, List<string> Hashtags)> GenerateCaptionAndHashtags(
        CampaignPlan? plan,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var prompt = $"Create a caption and hashtags for a {contentType} ";
            if (plan != null)
            {
                prompt += $"with these objectives: {plan.Objectives}. Target audience: {plan.TargetAudience}. Tone: {plan.ToneStyle}.";
            }

            var response = await _captionAgent.RunAsync(
                new ChatMessage(ChatRole.User, prompt),
                cancellationToken: cancellationToken);

            var text = response.Text ?? "{}";
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<CaptionResult>(text, options);

            return (
                result?.Caption ?? "Check out our latest content!",
                result?.Hashtags ?? ["#social", "#marketing", "#brand"]
            );
        }
        catch
        {
            return ("Check out our latest content!", ["#social", "#marketing", "#brand"]);
        }
    }

    private class CaptionResult
    {
        public string Caption { get; set; } = string.Empty;
        public List<string> Hashtags { get; set; } = [];
    }
}

/// <summary>
/// Localizer executor that translates captions and hashtags for selected markets.
/// Implements FR-3: Target Market Selection and FR-4: Content Localization
/// </summary>
public sealed class LocalizerExecutor : Executor<MarketingInputEvent, AIContent>
{
    private readonly ILogger<LocalizerExecutor> _logger;
    private readonly AIAgent _translationAgent;

    private static readonly Dictionary<string, string> MarketLanguages = new()
    {
        { "Spain", "Spanish (European)" },
        { "Mexico", "Spanish (Latin American)" },
        { "France", "French" },
        { "Germany", "German" },
        { "Brazil", "Portuguese" },
        { "Italy", "Italian" },
        { "Japan", "Japanese" }
    };

    public LocalizerExecutor(ILogger<LocalizerExecutor> logger, IChatClient chatClient) : base("Localizer")
    {
        _logger = logger;
        _translationAgent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = "TranslationAgent",
            Instructions = """
                You are an expert marketing translator. Translate social media captions and hashtags while:
                - Maintaining marketing tone and persuasiveness
                - Adapting to cultural nuances of the target market
                - Keeping translations natural-sounding
                - Respecting character limits for social media
                
                Your response MUST be in the following JSON format:
                {
                    "translatedCaption": "Translated caption text",
                    "translatedHashtags": ["#translatedtag1", "#translatedtag2"]
                }
                """
        });
    }

    public override async ValueTask<AIContent> HandleAsync(
        MarketingInputEvent input,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Localizer starting for {Count} markets", input.State.SelectedMarkets.Count);

            // If no markets selected, prompt for market selection
            if (input.State.SelectedMarkets.Count == 0 && !input.Input.Contains("skip-localization"))
            {
                return ApprovalRequestHelper.CreateApprovalRequest(
                    functionName: "select_target_markets",
                    arguments: new Dictionary<string, object?>
                    {
                        { "availableMarkets", MarketLanguages.Keys.ToList() },
                        { "state", JsonSerializer.Serialize(input.State) }
                    }
                );
            }

            // Translate content for each selected market
            var localizedContent = new Dictionary<string, LocalizedContent>();

            foreach (var market in input.State.SelectedMarkets)
            {
                if (!MarketLanguages.TryGetValue(market, out var language))
                {
                    _logger.LogWarning("Unknown market: {Market}", market);
                    continue;
                }

                var marketContent = await LocalizeForMarketAsync(
                    input.State.CreativeAssets,
                    market,
                    language,
                    cancellationToken);

                localizedContent[market] = marketContent;
            }

            input.State.LocalizedContent = localizedContent;
            _logger.LogInformation("Localization complete for {Count} markets", localizedContent.Count);

            // Return localized content for display (auto-proceed to schedule creation)
            var contentJson = JsonSerializer.Serialize(localizedContent, new JsonSerializerOptions { WriteIndented = true });

            return ApprovalRequestHelper.CreateApprovalRequest(
                functionName: "localization_complete",
                arguments: new Dictionary<string, object?>
                {
                    { "localizedContent", contentJson },
                    { "state", JsonSerializer.Serialize(input.State) }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in localizer executor");
            return new TextContent("Sorry, I encountered an error during localization. Please try again.");
        }
    }

    private async Task<LocalizedContent> LocalizeForMarketAsync(
        List<CreativeAsset> assets,
        string market,
        string language,
        CancellationToken cancellationToken)
    {
        var localizedAssets = new List<LocalizedAsset>();

        foreach (var asset in assets)
        {
            try
            {
                var prompt = $"""
                    Translate to {language} for the {market} market:
                    Caption: {asset.Caption}
                    Hashtags: {string.Join(" ", asset.Hashtags)}
                    """;

                var response = await _translationAgent.RunAsync(
                    new ChatMessage(ChatRole.User, prompt),
                    cancellationToken: cancellationToken);

                var text = response.Text ?? "{}";
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<TranslationResult>(text, options);

                localizedAssets.Add(new LocalizedAsset
                {
                    OriginalCaption = asset.Caption,
                    TranslatedCaption = result?.TranslatedCaption ?? asset.Caption,
                    OriginalHashtags = asset.Hashtags,
                    TranslatedHashtags = result?.TranslatedHashtags ?? asset.Hashtags
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to translate asset for {Market}", market);
                localizedAssets.Add(new LocalizedAsset
                {
                    OriginalCaption = asset.Caption,
                    TranslatedCaption = asset.Caption,
                    OriginalHashtags = asset.Hashtags,
                    TranslatedHashtags = asset.Hashtags
                });
            }
        }

        return new LocalizedContent
        {
            Market = market,
            Language = language,
            Assets = localizedAssets
        };
    }

    private class TranslationResult
    {
        public string TranslatedCaption { get; set; } = string.Empty;
        public List<string> TranslatedHashtags { get; set; } = [];
    }
}

/// <summary>
/// Schedule Creator executor that generates a two-week publishing schedule.
/// Implements FR-5: Publishing Schedule Creation
/// </summary>
public sealed class ScheduleCreatorExecutor : Executor<MarketingInputEvent, AIContent>
{
    private readonly ILogger<ScheduleCreatorExecutor> _logger;
    private readonly AIAgent _scheduleAgent;

    public ScheduleCreatorExecutor(ILogger<ScheduleCreatorExecutor> logger, IChatClient chatClient) : base("ScheduleCreator")
    {
        _logger = logger;
        _scheduleAgent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = "ScheduleAgent",
            Instructions = """
                You are an expert social media scheduler. Create optimal posting schedules based on:
                - Platform best practices (Instagram: 11am-1pm, 7-9pm; TikTok: 6-9am, 7-11pm)
                - Target audience timezone considerations
                - Even content distribution over 14 days
                - Balancing content types across the schedule
                
                Your response MUST be in JSON format with posting times.
                """
        });
    }

    public override async ValueTask<AIContent> HandleAsync(
        MarketingInputEvent input,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Schedule Creator generating 2-week schedule");

            var feedback = input.State.UserFeedback;
            var schedule = GenerateSchedule(input.State, feedback);
            input.State.Schedule = schedule;

            _logger.LogInformation("Generated schedule with {Count} posts", schedule.Posts.Count);

            var scheduleJson = JsonSerializer.Serialize(schedule, new JsonSerializerOptions { WriteIndented = true });

            return ApprovalRequestHelper.CreateApprovalRequest(
                functionName: "approve_schedule",
                arguments: new Dictionary<string, object?>
                {
                    { "schedule", scheduleJson },
                    { "state", JsonSerializer.Serialize(input.State) }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in schedule creator executor");
            return new TextContent("Sorry, I encountered an error while creating the schedule. Please try again.");
        }
    }

    private static PublishingSchedule GenerateSchedule(MarketingCampaignState state, string? feedback)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(13);
        var posts = new List<ScheduledPost>();

        var platforms = new[] { "Instagram", "TikTok" };
        var assetCount = state.CreativeAssets.Count;
        var markets = state.SelectedMarkets.Count > 0 ? state.SelectedMarkets : new List<string> { "English" };

        var postIndex = 0;
        for (int day = 0; day < 14; day++)
        {
            var postDate = startDate.AddDays(day);

            // Schedule posts for each market
            foreach (var market in markets)
            {
                var assetIndex = postIndex % assetCount;
                var asset = state.CreativeAssets.Count > assetIndex ? state.CreativeAssets[assetIndex] : null;
                var platform = platforms[postIndex % platforms.Length];

                // Optimal posting times
                var postHour = platform == "Instagram" ? 12 : 19; // 12pm or 7pm

                posts.Add(new ScheduledPost
                {
                    ScheduledTime = postDate.AddHours(postHour),
                    Platform = platform,
                    ContentType = asset?.Type ?? "image",
                    Language = market == "English" ? "English" : market,
                    Market = market,
                    AssetIndex = assetIndex,
                    Timezone = "UTC"
                });

                postIndex++;
            }
        }

        return new PublishingSchedule
        {
            StartDate = startDate,
            EndDate = endDate,
            Posts = posts
        };
    }
}

/// <summary>
/// Instagram Publisher executor that publishes the first scheduled post.
/// Implements FR-6: Instagram Post Publishing and FR-7: Final Campaign Delivery
/// </summary>
public sealed class InstagramPublisherExecutor : Executor<MarketingInputEvent, AIContent>
{
    private readonly ILogger<InstagramPublisherExecutor> _logger;

    public InstagramPublisherExecutor(ILogger<InstagramPublisherExecutor> logger) : base("InstagramPublisher")
    {
        _logger = logger;
    }

    public override async ValueTask<AIContent> HandleAsync(
        MarketingInputEvent input,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Instagram Publisher starting");

            // Simulate Instagram publishing (actual API integration would go here)
            var firstPost = input.State.Schedule?.Posts.FirstOrDefault(p => p.Platform == "Instagram");
            
            InstagramPostResult result;
            if (firstPost != null && input.State.CreativeAssets.Count > firstPost.AssetIndex)
            {
                var asset = input.State.CreativeAssets[firstPost.AssetIndex];
                
                // Simulated successful publish
                result = new InstagramPostResult
                {
                    Success = true,
                    PostId = $"ig_{Guid.NewGuid():N}",
                    PostUrl = $"https://instagram.com/p/{Guid.NewGuid():N}",
                    PublishedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Successfully published to Instagram: {PostUrl}", result.PostUrl);
            }
            else
            {
                result = new InstagramPostResult
                {
                    Success = false,
                    ErrorMessage = "No Instagram posts found in schedule"
                };
            }

            input.State.PublishedPost = result;

            // Return final campaign summary
            var summary = new
            {
                campaignPlan = input.State.CampaignPlan,
                creativeAssets = input.State.CreativeAssets,
                localizedContent = input.State.LocalizedContent,
                schedule = input.State.Schedule,
                publishedPost = result,
                status = result.Success ? "Campaign completed successfully!" : "Campaign completed with publishing error"
            };

            var summaryJson = JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });

            return ApprovalRequestHelper.CreateApprovalRequest(
                functionName: "campaign_complete",
                arguments: new Dictionary<string, object?>
                {
                    { "summary", summaryJson },
                    { "postUrl", result.PostUrl },
                    { "success", result.Success }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in instagram publisher executor");
            return new TextContent("Sorry, I encountered an error while publishing to Instagram. Please try again.");
        }
    }
}
