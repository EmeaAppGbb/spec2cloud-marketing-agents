"use client";
import { CopilotSidebar } from "@copilotkit/react-ui";
import { useHumanInTheLoop } from "@copilotkit/react-core";
import { useState } from "react";

// Agent badge component with distinct colors for each agent type
function AgentBadge({ agentName }: { agentName: string }) {
  const agentStyles: Record<string, { bg: string; text: string; icon: string }> = {
    "CampaignPlanner": { bg: "bg-blue-100 dark:bg-blue-900", text: "text-blue-800 dark:text-blue-200", icon: "📋" },
    "CreativeGenerator": { bg: "bg-green-100 dark:bg-green-900", text: "text-green-800 dark:text-green-200", icon: "🎨" },
    "Localizer": { bg: "bg-purple-100 dark:bg-purple-900", text: "text-purple-800 dark:text-purple-200", icon: "🌍" },
    "ScheduleCreator": { bg: "bg-orange-100 dark:bg-orange-900", text: "text-orange-800 dark:text-orange-200", icon: "📅" },
    "InstagramPublisher": { bg: "bg-pink-100 dark:bg-pink-900", text: "text-pink-800 dark:text-pink-200", icon: "📸" },
  };

  const style = agentStyles[agentName] || { bg: "bg-gray-100 dark:bg-gray-800", text: "text-gray-800 dark:text-gray-200", icon: "🤖" };

  return (
    <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-semibold ${style.bg} ${style.text}`}>
      {style.icon} {agentName}
    </span>
  );
}

// JSON display component for structured data
function JsonDisplay({ data, title }: { data: string; title?: string }) {
  let parsed: unknown;
  try {
    parsed = JSON.parse(data);
  } catch {
    return <pre className="text-sm whitespace-pre-wrap break-words">{data}</pre>;
  }

  return (
    <div className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
      {title && <h4 className="font-semibold text-gray-800 dark:text-gray-200 mb-2">{title}</h4>}
      <pre className="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap break-words overflow-auto max-h-96">
        {JSON.stringify(parsed, null, 2)}
      </pre>
    </div>
  );
}

// Creative asset display component
function CreativeAssetDisplay({ asset }: { asset: { type: string; url: string; caption: string; hashtags: string[] } }) {
  return (
    <div className="border border-gray-200 dark:border-gray-700 rounded-lg p-4 mb-4">
      <div className="flex items-center gap-2 mb-2">
        <span className={`px-2 py-1 rounded text-xs font-semibold ${
          asset.type === "video" 
            ? "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200" 
            : "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200"
        }`}>
          {asset.type === "video" ? "🎬 Video" : "🖼️ Image"}
        </span>
      </div>
      {asset.type === "image" ? (
        /* eslint-disable-next-line @next/next/no-img-element */
        <img src={asset.url} alt="Campaign asset" className="max-w-full h-auto rounded-lg shadow-md mb-3" />
      ) : (
        <div className="bg-gray-200 dark:bg-gray-700 rounded-lg p-8 text-center mb-3">
          <span className="text-4xl">🎬</span>
          <p className="text-gray-600 dark:text-gray-400 mt-2">Video placeholder</p>
          <p className="text-xs text-gray-500 dark:text-gray-500 mt-1">{asset.url}</p>
        </div>
      )}
      <p className="text-gray-800 dark:text-gray-200 mb-2">{asset.caption}</p>
      <div className="flex flex-wrap gap-1">
        {asset.hashtags.map((tag, i) => (
          <span key={i} className="text-blue-600 dark:text-blue-400 text-sm">{tag}</span>
        ))}
      </div>
    </div>
  );
}

// Schedule table component
function ScheduleTable({ schedule }: { schedule: { posts: Array<{ scheduledTime: string; platform: string; contentType: string; language: string; market: string }> } }) {
  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
        <thead className="bg-gray-50 dark:bg-gray-800">
          <tr>
            <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Date/Time</th>
            <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Platform</th>
            <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Type</th>
            <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Market</th>
          </tr>
        </thead>
        <tbody className="bg-white dark:bg-gray-900 divide-y divide-gray-200 dark:divide-gray-700">
          {schedule.posts.slice(0, 14).map((post, i) => (
            <tr key={i}>
              <td className="px-4 py-2 text-sm text-gray-700 dark:text-gray-300">
                {new Date(post.scheduledTime).toLocaleString()}
              </td>
              <td className="px-4 py-2 text-sm">
                <span className={`px-2 py-1 rounded text-xs font-semibold ${
                  post.platform === "Instagram" 
                    ? "bg-pink-100 text-pink-800 dark:bg-pink-900 dark:text-pink-200" 
                    : "bg-black text-white"
                }`}>
                  {post.platform}
                </span>
              </td>
              <td className="px-4 py-2 text-sm text-gray-700 dark:text-gray-300">{post.contentType}</td>
              <td className="px-4 py-2 text-sm text-gray-700 dark:text-gray-300">{post.market}</td>
            </tr>
          ))}
        </tbody>
      </table>
      {schedule.posts.length > 14 && (
        <p className="text-center text-gray-500 dark:text-gray-400 mt-2 text-sm">
          + {schedule.posts.length - 14} more posts
        </p>
      )}
    </div>
  );
}

// Market selection component with internal state
function MarketSelector({ 
  markets, 
  onSelect, 
  onSkip 
}: { 
  markets: string[]; 
  onSelect: (selected: string[]) => void; 
  onSkip: () => void;
}) {
  const [selectedMarkets, setSelectedMarkets] = useState<string[]>([]);

  return (
    <>
      <div className="grid grid-cols-2 md:grid-cols-3 gap-2 mb-4">
        {markets.map((market) => (
          <label key={market} className="flex items-center gap-2 p-2 bg-white dark:bg-gray-800 rounded border border-gray-200 dark:border-gray-700 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700">
            <input
              type="checkbox"
              checked={selectedMarkets.includes(market)}
              onChange={(e) => {
                if (e.target.checked) {
                  setSelectedMarkets([...selectedMarkets, market]);
                } else {
                  setSelectedMarkets(selectedMarkets.filter(m => m !== market));
                }
              }}
              className="w-4 h-4 text-purple-600 rounded"
            />
            <span className="text-gray-700 dark:text-gray-300">{market}</span>
          </label>
        ))}
      </div>
      <div className="flex gap-3">
        <button 
          onClick={() => onSelect(selectedMarkets)}
          className="flex-1 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg transition-colors shadow-md disabled:opacity-50"
          disabled={selectedMarkets.length === 0}
        >
          🌍 Localize for {selectedMarkets.length} Market{selectedMarkets.length !== 1 ? 's' : ''}
        </button>
        <button 
          onClick={onSkip}
          className="flex-1 px-4 py-2 bg-gray-500 hover:bg-gray-600 text-white font-semibold rounded-lg transition-colors shadow-md"
        >
          Skip (English Only)
        </button>
      </div>
    </>
  );
}

export default function MarketingPage() {
  const [campaignState, setCampaignState] = useState<{
    plan?: unknown;
    assets?: Array<{ type: string; url: string; caption: string; hashtags: string[] }>;
    localizedContent?: Record<string, unknown>;
    schedule?: { posts: Array<{ scheduledTime: string; platform: string; contentType: string; language: string; market: string }> };
    publishedPost?: { postUrl?: string; success?: boolean };
  }>({});

  // Human Gate 1: Campaign Plan Approval
  useHumanInTheLoop({
    name: "approve_campaign_plan",
    description: "Review and approve the campaign plan",
    parameters: [
      { name: "plan", type: "string", description: "The campaign plan JSON", required: true },
      { name: "state", type: "string", description: "Current workflow state", required: true },
    ],
    render: ({ args, respond }) => {
      if (!respond) return <></>;
      
      return (
        <div className="p-4 mb-4 bg-blue-50 dark:bg-blue-900/20 border-2 border-blue-300 dark:border-blue-700 rounded-lg shadow-md">
          <div className="flex items-center gap-2 mb-3">
            <AgentBadge agentName="CampaignPlanner" />
            <span className="text-lg font-semibold text-blue-900 dark:text-blue-100">Campaign Plan Ready</span>
          </div>
          <p className="text-sm text-blue-800 dark:text-blue-200 mb-3">
            Review the strategic campaign plan below. Approve to proceed with creative asset generation.
          </p>
          <JsonDisplay data={args.plan} title="Campaign Strategy" />
          <div className="flex gap-3 mt-4">
            <button 
              onClick={() => respond("plan-approved")}
              className="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 text-white font-semibold rounded-lg transition-colors shadow-md"
            >
              ✓ Approve Plan
            </button>
          </div>
        </div>
      );
    },
  });

  // Human Gate 2: Creative Assets Approval (FR-2.1)
  useHumanInTheLoop({
    name: "approve_creative_assets",
    description: "Review and approve the generated creative assets",
    parameters: [
      { name: "assets", type: "string", description: "The creative assets JSON", required: true },
      { name: "state", type: "string", description: "Current workflow state", required: true },
    ],
    render: ({ args, respond }) => {
      if (!respond) return <></>;

      let assets: Array<{ type: string; url: string; caption: string; hashtags: string[] }> = [];
      try {
        assets = JSON.parse(args.assets);
      } catch {
        assets = [];
      }

      return (
        <div className="p-4 mb-4 bg-green-50 dark:bg-green-900/20 border-2 border-green-300 dark:border-green-700 rounded-lg shadow-md">
          <div className="flex items-center gap-2 mb-3">
            <AgentBadge agentName="CreativeGenerator" />
            <span className="text-lg font-semibold text-green-900 dark:text-green-100">Creative Assets Ready for Review</span>
          </div>
          <p className="text-sm text-green-800 dark:text-green-200 mb-3">
            Review the {assets.length} creative assets (2 images + 1 video) below. Approve all or provide feedback for regeneration.
          </p>
          <div className="max-h-96 overflow-y-auto mb-4">
            {assets.map((asset, i) => (
              <CreativeAssetDisplay key={i} asset={asset} />
            ))}
          </div>
          <div className="flex gap-3">
            <button 
              onClick={() => {
                setCampaignState(prev => ({ ...prev, assets }));
                respond("creative-approved");
              }}
              className="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 text-white font-semibold rounded-lg transition-colors shadow-md"
            >
              ✓ Approve All Assets
            </button>
            <button 
              onClick={() => {
                const feedback = prompt("Please provide feedback for what needs to be changed:");
                if (feedback) {
                  respond(`creative-rejected|feedback:${feedback}`);
                }
              }}
              className="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 text-white font-semibold rounded-lg transition-colors shadow-md"
            >
              ✗ Reject with Feedback
            </button>
          </div>
        </div>
      );
    },
  });

  // Human Gate 3: Market Selection (FR-3)
  useHumanInTheLoop({
    name: "select_target_markets",
    description: "Select target markets for localization",
    parameters: [
      { name: "availableMarkets", type: "string", description: "Available markets list", required: true },
      { name: "state", type: "string", description: "Current workflow state", required: true },
    ],
    render: ({ args, respond }) => {
      if (!respond) return <></>;

      let markets: string[] = [];
      try {
        markets = JSON.parse(args.availableMarkets);
      } catch {
        markets = ["Spain", "Mexico", "France", "Germany", "Brazil", "Italy", "Japan"];
      }

      return (
        <div className="p-4 mb-4 bg-purple-50 dark:bg-purple-900/20 border-2 border-purple-300 dark:border-purple-700 rounded-lg shadow-md">
          <div className="flex items-center gap-2 mb-3">
            <AgentBadge agentName="Localizer" />
            <span className="text-lg font-semibold text-purple-900 dark:text-purple-100">Select Target Markets</span>
          </div>
          <p className="text-sm text-purple-800 dark:text-purple-200 mb-3">
            Select the geographic markets where you want to localize your campaign content.
          </p>
          <MarketSelector 
            markets={markets}
            onSelect={(selected) => respond(`markets-selected|${JSON.stringify(selected)}`)}
            onSkip={() => respond("skip-localization")}
          />
        </div>
      );
    },
  });

  // Localization Complete notification
  useHumanInTheLoop({
    name: "localization_complete",
    description: "Display localized content and proceed",
    parameters: [
      { name: "localizedContent", type: "string", description: "Localized content JSON", required: true },
      { name: "state", type: "string", description: "Current workflow state", required: true },
    ],
    render: ({ args, respond }) => {
      if (!respond) return <></>;

      let content: Record<string, unknown> = {};
      try {
        content = JSON.parse(args.localizedContent);
      } catch {
        content = {};
      }

      return (
        <div className="p-4 mb-4 bg-purple-50 dark:bg-purple-900/20 border-2 border-purple-300 dark:border-purple-700 rounded-lg shadow-md">
          <div className="flex items-center gap-2 mb-3">
            <AgentBadge agentName="Localizer" />
            <span className="text-lg font-semibold text-purple-900 dark:text-purple-100">Localization Complete</span>
          </div>
          <p className="text-sm text-purple-800 dark:text-purple-200 mb-3">
            Content has been translated for {Object.keys(content).length} market(s).
          </p>
          <JsonDisplay data={args.localizedContent} title="Localized Content by Market" />
          <div className="flex gap-3 mt-4">
            <button 
              onClick={() => {
                setCampaignState(prev => ({ ...prev, localizedContent: content }));
                respond("localization-complete");
              }}
              className="flex-1 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg transition-colors shadow-md"
            >
              ✓ Continue to Schedule Creation
            </button>
          </div>
        </div>
      );
    },
  });

  // Human Gate 4: Schedule Approval (FR-5.1)
  useHumanInTheLoop({
    name: "approve_schedule",
    description: "Review and approve the publishing schedule",
    parameters: [
      { name: "schedule", type: "string", description: "The publishing schedule JSON", required: true },
      { name: "state", type: "string", description: "Current workflow state", required: true },
    ],
    render: ({ args, respond }) => {
      if (!respond) return <></>;

      let schedule: { posts: Array<{ scheduledTime: string; platform: string; contentType: string; language: string; market: string }> } | null = null;
      try {
        schedule = JSON.parse(args.schedule);
      } catch {
        schedule = null;
      }

      return (
        <div className="p-4 mb-4 bg-orange-50 dark:bg-orange-900/20 border-2 border-orange-300 dark:border-orange-700 rounded-lg shadow-md">
          <div className="flex items-center gap-2 mb-3">
            <AgentBadge agentName="ScheduleCreator" />
            <span className="text-lg font-semibold text-orange-900 dark:text-orange-100">Publishing Schedule Ready</span>
          </div>
          <p className="text-sm text-orange-800 dark:text-orange-200 mb-3">
            Review the 2-week publishing schedule below. {schedule?.posts?.length || 0} posts scheduled.
          </p>
          {schedule && <ScheduleTable schedule={schedule} />}
          <div className="flex gap-3 mt-4">
            <button 
              onClick={() => {
                if (schedule) {
                  setCampaignState(prev => ({ ...prev, schedule }));
                }
                respond("schedule-approved");
              }}
              className="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 text-white font-semibold rounded-lg transition-colors shadow-md"
            >
              ✓ Approve Schedule
            </button>
            <button 
              onClick={() => {
                const feedback = prompt("Please provide feedback for schedule changes:");
                if (feedback) {
                  respond(`schedule-rejected|feedback:${feedback}`);
                }
              }}
              className="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 text-white font-semibold rounded-lg transition-colors shadow-md"
            >
              ✗ Reject with Feedback
            </button>
          </div>
        </div>
      );
    },
  });

  // Campaign Complete notification (FR-7)
  useHumanInTheLoop({
    name: "campaign_complete",
    description: "Display campaign completion summary",
    parameters: [
      { name: "summary", type: "string", description: "Campaign summary JSON", required: true },
      { name: "postUrl", type: "string", description: "Instagram post URL", required: false },
      { name: "success", type: "string", description: "Whether publishing succeeded", required: true },
    ],
    render: ({ args, respond }) => {
      if (!respond) return <></>;

      const success = String(args.success) === "true";

      return (
        <div className={`p-4 mb-4 ${
          success 
            ? "bg-pink-50 dark:bg-pink-900/20 border-pink-300 dark:border-pink-700" 
            : "bg-yellow-50 dark:bg-yellow-900/20 border-yellow-300 dark:border-yellow-700"
        } border-2 rounded-lg shadow-md`}>
          <div className="flex items-center gap-2 mb-3">
            <AgentBadge agentName="InstagramPublisher" />
            <span className={`text-lg font-semibold ${
              success 
                ? "text-pink-900 dark:text-pink-100" 
                : "text-yellow-900 dark:text-yellow-100"
            }`}>
              {success ? "🎉 Campaign Published Successfully!" : "⚠️ Campaign Complete with Issues"}
            </span>
          </div>
          
          {success && args.postUrl && (
            <div className="bg-white dark:bg-gray-800 p-4 rounded-lg mb-4">
              <p className="text-gray-700 dark:text-gray-300 mb-2">Your first post is now live on Instagram!</p>
              <a 
                href={args.postUrl} 
                target="_blank" 
                rel="noopener noreferrer"
                className="text-pink-600 dark:text-pink-400 hover:underline font-medium"
              >
                📸 View on Instagram →
              </a>
            </div>
          )}

          <JsonDisplay data={args.summary} title="Campaign Summary" />
          
          <div className="flex gap-3 mt-4">
            <button 
              onClick={() => {
                setCampaignState(prev => ({ ...prev, publishedPost: { postUrl: args.postUrl, success } }));
                respond("acknowledged");
              }}
              className="flex-1 px-4 py-2 bg-pink-600 hover:bg-pink-700 text-white font-semibold rounded-lg transition-colors shadow-md"
            >
              ✓ Complete Campaign
            </button>
          </div>
        </div>
      );
    },
  });

  return (
    <CopilotSidebar
      defaultOpen={true}
      labels={{
        title: "Marketing Campaign AI",
        initial: "👋 Welcome! I'm your AI marketing assistant. Tell me about your campaign idea and I'll help you create a complete social media strategy with images, captions, translations, and a publishing schedule.",
        placeholder: "Describe your campaign brief...",
      }}
      instructions="You are an AI marketing assistant that helps create complete social media campaigns. Guide the user through campaign planning, creative asset generation, localization, and publishing."
    >
      <main className="min-h-screen bg-gradient-to-br from-pink-50 to-purple-50 dark:from-gray-900 dark:to-purple-900">
        <div className="container mx-auto px-4 py-12 max-w-5xl">
          {/* Campaign Status Dashboard */}
          {(campaignState.assets || campaignState.schedule || campaignState.publishedPost) && (
            <div className="mb-8 bg-white dark:bg-gray-800 rounded-2xl shadow-2xl p-6 border-2 border-purple-200 dark:border-purple-700">
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">📊 Campaign Dashboard</h2>
              
              {/* Progress indicators */}
              <div className="flex items-center justify-between mb-6 overflow-x-auto pb-2">
                {["Planning", "Creative", "Localization", "Schedule", "Published"].map((step, i) => {
                  const stepStatus = 
                    i === 0 ? "complete" :
                    i === 1 && campaignState.assets ? "complete" :
                    i === 2 && campaignState.localizedContent ? "complete" :
                    i === 3 && campaignState.schedule ? "complete" :
                    i === 4 && campaignState.publishedPost ? "complete" : "pending";
                  
                  return (
                    <div key={step} className="flex items-center">
                      <div className={`flex items-center justify-center w-8 h-8 rounded-full text-sm font-semibold ${
                        stepStatus === "complete" 
                          ? "bg-green-500 text-white" 
                          : "bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-400"
                      }`}>
                        {stepStatus === "complete" ? "✓" : i + 1}
                      </div>
                      <span className={`ml-2 text-sm ${
                        stepStatus === "complete" 
                          ? "text-green-600 dark:text-green-400 font-medium" 
                          : "text-gray-500 dark:text-gray-400"
                      }`}>{step}</span>
                      {i < 4 && <div className="w-8 h-0.5 bg-gray-300 dark:bg-gray-600 mx-2"></div>}
                    </div>
                  );
                })}
              </div>

              {/* Approved assets display */}
              {campaignState.assets && campaignState.assets.length > 0 && (
                <div className="mb-6">
                  <h3 className="text-lg font-semibold text-gray-800 dark:text-gray-200 mb-3">✅ Approved Assets</h3>
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                    {campaignState.assets.map((asset, i) => (
                      <div key={i} className="border border-gray-200 dark:border-gray-700 rounded-lg p-3">
                        {asset.type === "image" ? (
                          /* eslint-disable-next-line @next/next/no-img-element */
                          <img src={asset.url} alt={`Asset ${i+1}`} className="w-full h-32 object-cover rounded" />
                        ) : (
                          <div className="w-full h-32 bg-gray-200 dark:bg-gray-700 rounded flex items-center justify-center">
                            <span className="text-3xl">🎬</span>
                          </div>
                        )}
                        <p className="text-xs text-gray-600 dark:text-gray-400 mt-2 truncate">{asset.caption}</p>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Published post confirmation */}
              {campaignState.publishedPost?.success && campaignState.publishedPost?.postUrl && (
                <div className="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-700 rounded-lg p-4">
                  <h3 className="text-lg font-semibold text-green-800 dark:text-green-200 mb-2">🎉 First Post Published!</h3>
                  <a 
                    href={campaignState.publishedPost.postUrl} 
                    target="_blank" 
                    rel="noopener noreferrer"
                    className="text-green-600 dark:text-green-400 hover:underline"
                  >
                    View on Instagram →
                  </a>
                </div>
              )}
            </div>
          )}

          {/* Welcome section */}
          <div className="text-center space-y-6">
            <div className="inline-block p-3 bg-gradient-to-br from-pink-100 to-purple-100 dark:from-pink-900 dark:to-purple-900 rounded-full mb-4">
              <svg 
                className="w-12 h-12 text-pink-600 dark:text-pink-400" 
                fill="none" 
                stroke="currentColor" 
                viewBox="0 0 24 24"
              >
                <path 
                  strokeLinecap="round" 
                  strokeLinejoin="round" 
                  strokeWidth={2} 
                  d="M11 5.882V19.24a1.76 1.76 0 01-3.417.592l-2.147-6.15M18 13a3 3 0 100-6M5.436 13.683A4.001 4.001 0 017 6h1.832c4.1 0 7.625-1.234 9.168-3v14c-1.543-1.766-5.067-3-9.168-3H7a3.988 3.988 0 01-1.564-.317z" 
                />
              </svg>
            </div>
            <h1 className="text-5xl font-bold bg-gradient-to-r from-pink-600 to-purple-600 bg-clip-text text-transparent">
              Marketing Campaign Studio
            </h1>
            <p className="text-xl text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
              Create complete social media campaigns with AI-powered planning, creative generation, multi-market localization, and automated publishing.
            </p>
          </div>

          {/* Feature cards */}
          <div className="mt-16 grid grid-cols-1 md:grid-cols-5 gap-4">
            {[
              { icon: "📋", title: "Campaign Planner", desc: "Strategic planning with smart defaults", color: "blue" },
              { icon: "🎨", title: "Creative Generator", desc: "2 images + 1 video with captions", color: "green" },
              { icon: "🌍", title: "Localizer", desc: "Multi-market translation", color: "purple" },
              { icon: "📅", title: "Schedule Creator", desc: "2-week publishing calendar", color: "orange" },
              { icon: "📸", title: "Publisher", desc: "Automated Instagram posting", color: "pink" },
            ].map((agent) => (
              <div key={agent.title} className={`bg-white dark:bg-gray-800 rounded-lg shadow-lg p-4 hover:shadow-xl transition-shadow border-t-4 border-${agent.color}-500`}>
                <div className="text-3xl mb-2">{agent.icon}</div>
                <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-1">
                  {agent.title}
                </h3>
                <p className="text-xs text-gray-600 dark:text-gray-300">
                  {agent.desc}
                </p>
              </div>
            ))}
          </div>
        </div>
      </main>
    </CopilotSidebar>
  );
}
