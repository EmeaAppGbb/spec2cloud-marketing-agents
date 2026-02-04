"use client";

import { Campaign } from "../hooks/useSession";

interface CampaignHistoryProps {
  campaigns: Campaign[];
  onSelect: (campaignId: string) => void;
  onRefresh: () => void;
  loading: boolean;
  selectedCampaignId?: string;
}

const statusColors: Record<string, { bg: string; text: string }> = {
  Draft: { bg: "bg-gray-100 dark:bg-gray-700", text: "text-gray-800 dark:text-gray-200" },
  InProgress: { bg: "bg-blue-100 dark:bg-blue-900", text: "text-blue-800 dark:text-blue-200" },
  Completed: { bg: "bg-green-100 dark:bg-green-900", text: "text-green-800 dark:text-green-200" },
  Published: { bg: "bg-purple-100 dark:bg-purple-900", text: "text-purple-800 dark:text-purple-200" },
};

const stepLabels: Record<string, string> = {
  CampaignPlanning: "📋 Planning",
  CreativeGeneration: "🎨 Creative",
  Localization: "🌍 Localization",
  ScheduleCreation: "📅 Scheduling",
  InstagramPublishing: "📸 Publishing",
  Completed: "✅ Completed",
};

export function CampaignHistory({
  campaigns,
  onSelect,
  onRefresh,
  loading,
  selectedCampaignId,
}: CampaignHistoryProps) {
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString(undefined, {
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <div className="h-full flex flex-col bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700">
      <div className="p-4 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-gray-800 dark:text-gray-200">
          Campaign History
        </h2>
        <button
          onClick={onRefresh}
          disabled={loading}
          className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-full transition-colors disabled:opacity-50"
          title="Refresh campaigns"
        >
          <svg
            className={`w-5 h-5 text-gray-500 dark:text-gray-400 ${loading ? "animate-spin" : ""}`}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
            />
          </svg>
        </button>
      </div>

      <div className="flex-1 overflow-y-auto">
        {campaigns.length === 0 ? (
          <div className="p-4 text-center text-gray-500 dark:text-gray-400">
            <p className="text-sm">No campaigns yet.</p>
            <p className="text-xs mt-1">Start a conversation to create your first campaign!</p>
          </div>
        ) : (
          <ul className="divide-y divide-gray-200 dark:divide-gray-700">
            {campaigns.map((campaign) => {
              const colors = statusColors[campaign.status] || statusColors.Draft;
              const stepLabel = stepLabels[campaign.currentStep || "CampaignPlanning"] || campaign.currentStep;
              const isSelected = campaign.id === selectedCampaignId;

              return (
                <li key={campaign.id}>
                  <button
                    onClick={() => onSelect(campaign.id)}
                    className={`w-full p-4 text-left hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors ${
                      isSelected ? "bg-blue-50 dark:bg-blue-900/30 border-l-4 border-blue-500" : ""
                    }`}
                  >
                    <div className="flex items-start justify-between">
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-gray-900 dark:text-gray-100 truncate">
                          {campaign.name || "Untitled Campaign"}
                        </p>
                        <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                          {formatDate(campaign.updatedAt)}
                        </p>
                      </div>
                      <span
                        className={`ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${colors.bg} ${colors.text}`}
                      >
                        {campaign.status}
                      </span>
                    </div>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-2">
                      {stepLabel}
                    </p>
                  </button>
                </li>
              );
            })}
          </ul>
        )}
      </div>

      <div className="p-3 border-t border-gray-200 dark:border-gray-700">
        <button
          onClick={() => onSelect("")}
          className="w-full py-2 px-4 bg-blue-500 hover:bg-blue-600 text-white text-sm font-medium rounded-lg transition-colors"
        >
          + New Campaign
        </button>
      </div>
    </div>
  );
}
