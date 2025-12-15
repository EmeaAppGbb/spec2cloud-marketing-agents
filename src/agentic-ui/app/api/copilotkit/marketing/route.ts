import {
  CopilotRuntime,
  ExperimentalEmptyAdapter,
  copilotRuntimeNextJSAppRouterEndpoint,
} from "@copilotkit/runtime";
import { HttpAgent } from "@ag-ui/client";
import { NextRequest } from "next/server";

// Service adapter for multi-agent support
const serviceAdapter = new ExperimentalEmptyAdapter();

// Create the CopilotRuntime instance pointing to the marketing workflow endpoint
const runtime = new CopilotRuntime({
  agents: {
    marketing_agent: new HttpAgent({ 
      url: `${process.env.AGENT_API_URL || "http://localhost:5149"}/marketing` 
    }),
  },
});

// Build a Next.js API route that handles the CopilotKit runtime requests
export const POST = async (req: NextRequest) => {
  const { handleRequest } = copilotRuntimeNextJSAppRouterEndpoint({
    runtime,
    serviceAdapter,
    endpoint: "/api/copilotkit/marketing",
  });
  return handleRequest(req);
};
