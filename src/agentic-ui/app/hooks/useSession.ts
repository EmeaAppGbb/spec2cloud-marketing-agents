"use client";

import { useState, useEffect, useCallback } from "react";

const SESSION_ID_KEY = "marketing-campaign-session-id";

export interface Campaign {
  id: string;
  name: string;
  status: "Draft" | "InProgress" | "Completed" | "Published";
  currentStep: string;
  createdAt: string;
  updatedAt: string;
}

export interface CampaignDetail {
  campaign: Campaign & {
    brief?: string;
    plan?: object;
    schedule?: object;
    publishResult?: object;
    localizedContent?: Record<string, object>;
  };
  assets: Array<{
    id: string;
    type: "Image" | "Video";
    blobUrl: string;
    caption?: string;
    hashtags?: string[];
    approvalStatus: "Pending" | "Approved" | "Rejected";
  }>;
  conversation: Array<{
    id: string;
    role: "User" | "Assistant" | "System";
    content: string;
    imageUrl?: string;
    timestamp: string;
  }>;
  checkpoint?: {
    currentStep: string;
    completedSteps: string[];
    humanGateDecisions: Array<{
      gateName: string;
      decision: string;
      feedback?: string;
      timestamp: string;
    }>;
  };
}

export function useSession() {
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [selectedCampaign, setSelectedCampaign] = useState<CampaignDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Initialize session ID on mount
  useEffect(() => {
    let id = localStorage.getItem(SESSION_ID_KEY);
    if (!id) {
      id = crypto.randomUUID();
      localStorage.setItem(SESSION_ID_KEY, id);
    }
    setSessionId(id);
  }, []);

  // Fetch campaigns for the current session
  const fetchCampaigns = useCallback(async (status?: string) => {
    if (!sessionId) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const params = new URLSearchParams({ sessionId });
      if (status) params.append("status", status);
      
      const response = await fetch(`/api/campaigns?${params}`);
      if (!response.ok) {
        throw new Error(`Failed to fetch campaigns: ${response.statusText}`);
      }
      
      const data = await response.json();
      setCampaigns(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unknown error");
      console.error("Error fetching campaigns:", err);
    } finally {
      setLoading(false);
    }
  }, [sessionId]);

  // Fetch campaign detail
  const fetchCampaignDetail = useCallback(async (campaignId: string) => {
    if (!sessionId) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const params = new URLSearchParams({ sessionId });
      
      const response = await fetch(`/api/campaigns/${campaignId}?${params}`);
      if (!response.ok) {
        throw new Error(`Failed to fetch campaign detail: ${response.statusText}`);
      }
      
      const data = await response.json();
      setSelectedCampaign(data);
      return data;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unknown error");
      console.error("Error fetching campaign detail:", err);
      return null;
    } finally {
      setLoading(false);
    }
  }, [sessionId]);

  // Clear session (for testing/debugging)
  const clearSession = useCallback(() => {
    localStorage.removeItem(SESSION_ID_KEY);
    const newId = crypto.randomUUID();
    localStorage.setItem(SESSION_ID_KEY, newId);
    setSessionId(newId);
    setCampaigns([]);
    setSelectedCampaign(null);
  }, []);

  return {
    sessionId,
    campaigns,
    selectedCampaign,
    loading,
    error,
    fetchCampaigns,
    fetchCampaignDetail,
    setSelectedCampaign,
    clearSession,
  };
}
