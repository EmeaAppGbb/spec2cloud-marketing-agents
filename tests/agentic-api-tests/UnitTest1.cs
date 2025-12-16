using agentic_api.Workflows;
using Microsoft.Extensions.AI;

namespace agentic_api_tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        // Placeholder test
    }
}

/// <summary>
/// Unit tests for the Marketing Workflow components
/// </summary>
public class MarketingWorkflowTests
{
    [Fact]
    public void CampaignPlan_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var plan = new CampaignPlan();

        // Assert
        Assert.Equal(2, plan.Platforms.Count);
        Assert.Contains("Instagram", plan.Platforms);
        Assert.Contains("TikTok", plan.Platforms);
        Assert.Equal("2 weeks", plan.Timeline);
    }

    [Fact]
    public void CreativeAsset_CanBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var asset = new CreativeAsset
        {
            Type = "image",
            Url = "https://example.com/image.png",
            Caption = "Test caption",
            Hashtags = new List<string> { "#test", "#marketing" }
        };

        // Assert
        Assert.Equal("image", asset.Type);
        Assert.Equal("https://example.com/image.png", asset.Url);
        Assert.Equal("Test caption", asset.Caption);
        Assert.Equal(2, asset.Hashtags.Count);
    }

    [Fact]
    public void MarketingCampaignState_InitializesWithEmptyCollections()
    {
        // Arrange & Act
        var state = new MarketingCampaignState();

        // Assert
        Assert.Empty(state.CreativeAssets);
        Assert.Empty(state.SelectedMarkets);
        Assert.Empty(state.LocalizedContent);
        Assert.Null(state.CampaignPlan);
        Assert.Null(state.Schedule);
        Assert.Null(state.PublishedPost);
    }

    [Fact]
    public void MarketingInputEvent_CanSetWorkflowStep()
    {
        // Arrange & Act
        var inputEvent = new MarketingInputEvent
        {
            Input = "Test campaign brief",
            NextStep = MarketingWorkflowSteps.CampaignPlanning
        };

        // Assert
        Assert.Equal("Test campaign brief", inputEvent.Input);
        Assert.Equal(MarketingWorkflowSteps.CampaignPlanning, inputEvent.NextStep);
    }

    [Fact]
    public void MarketingWorkflowSteps_HasAllExpectedValues()
    {
        // Assert all workflow steps exist
        Assert.Equal(6, Enum.GetValues<MarketingWorkflowSteps>().Length);
        Assert.True(Enum.IsDefined(MarketingWorkflowSteps.CampaignPlanning));
        Assert.True(Enum.IsDefined(MarketingWorkflowSteps.CreativeGeneration));
        Assert.True(Enum.IsDefined(MarketingWorkflowSteps.Localization));
        Assert.True(Enum.IsDefined(MarketingWorkflowSteps.ScheduleCreation));
        Assert.True(Enum.IsDefined(MarketingWorkflowSteps.InstagramPublishing));
        Assert.True(Enum.IsDefined(MarketingWorkflowSteps.Completed));
    }

    [Fact]
    public void ScheduledPost_CanBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var post = new ScheduledPost
        {
            ScheduledTime = DateTime.UtcNow.AddDays(1),
            Platform = "Instagram",
            ContentType = "image",
            Language = "English",
            Market = "English",
            AssetIndex = 0,
            Timezone = "UTC"
        };

        // Assert
        Assert.Equal("Instagram", post.Platform);
        Assert.Equal("image", post.ContentType);
        Assert.Equal("English", post.Language);
        Assert.Equal(0, post.AssetIndex);
    }

    [Fact]
    public void PublishingSchedule_CanHoldMultiplePosts()
    {
        // Arrange
        var schedule = new PublishingSchedule
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            Posts = new List<ScheduledPost>
            {
                new ScheduledPost
                {
                    ScheduledTime = DateTime.UtcNow.AddDays(1),
                    Platform = "Instagram",
                    ContentType = "image",
                    Language = "English",
                    Market = "English",
                    AssetIndex = 0
                },
                new ScheduledPost
                {
                    ScheduledTime = DateTime.UtcNow.AddDays(2),
                    Platform = "TikTok",
                    ContentType = "video",
                    Language = "Spanish",
                    Market = "Spain",
                    AssetIndex = 1
                }
            }
        };

        // Assert
        Assert.Equal(2, schedule.Posts.Count);
        Assert.Equal("Instagram", schedule.Posts[0].Platform);
        Assert.Equal("TikTok", schedule.Posts[1].Platform);
    }

    [Fact]
    public void LocalizedContent_CanStoreTranslations()
    {
        // Arrange & Act
        var content = new LocalizedContent
        {
            Market = "Spain",
            Language = "Spanish (European)",
            Assets = new List<LocalizedAsset>
            {
                new LocalizedAsset
                {
                    OriginalCaption = "Hello world!",
                    TranslatedCaption = "¡Hola mundo!",
                    OriginalHashtags = new List<string> { "#hello", "#world" },
                    TranslatedHashtags = new List<string> { "#hola", "#mundo" }
                }
            }
        };

        // Assert
        Assert.Equal("Spain", content.Market);
        Assert.Equal("Spanish (European)", content.Language);
        Assert.Single(content.Assets);
        Assert.Equal("¡Hola mundo!", content.Assets[0].TranslatedCaption);
    }

    [Fact]
    public void InstagramPostResult_CanRepresentSuccess()
    {
        // Arrange & Act
        var result = new InstagramPostResult
        {
            Success = true,
            PostId = "ig_123456",
            PostUrl = "https://instagram.com/p/123456",
            PublishedAt = DateTime.UtcNow
        };

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.PostId);
        Assert.NotNull(result.PostUrl);
        Assert.NotNull(result.PublishedAt);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void InstagramPostResult_CanRepresentFailure()
    {
        // Arrange & Act
        var result = new InstagramPostResult
        {
            Success = false,
            ErrorMessage = "Failed to publish: API error"
        };

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.PostId);
        Assert.Null(result.PostUrl);
        Assert.Equal("Failed to publish: API error", result.ErrorMessage);
    }
}
