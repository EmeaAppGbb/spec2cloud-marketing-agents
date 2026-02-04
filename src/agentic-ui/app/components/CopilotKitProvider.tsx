"use client";

import { CopilotKit } from "@copilotkit/react-core";
import { useState, useEffect } from "react";

const SESSION_ID_KEY = "marketing-campaign-session-id";

function getOrCreateSessionId(): string {
  if (typeof window === "undefined") {
    return ""; // SSR fallback
  }
  let id = localStorage.getItem(SESSION_ID_KEY);
  if (!id) {
    id = crypto.randomUUID();
    localStorage.setItem(SESSION_ID_KEY, id);
  }
  return id;
}

export function CopilotKitProvider({ children }: { children: React.ReactNode }) {
  const [sessionId, setSessionId] = useState<string>("");
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
    setSessionId(getOrCreateSessionId());
  }, []);

  // Don't render CopilotKit until we have a session ID
  if (!mounted || !sessionId) {
    return <div className="flex items-center justify-center h-screen">Loading...</div>;
  }

  return (
    <CopilotKit 
      runtimeUrl="/api/copilotkit" 
      agent="my_agent"
      headers={{
        "x-session-id": sessionId,
      }}
    >
      {children}
    </CopilotKit>
  );
}
