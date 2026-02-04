import {
  CopilotRuntime,
  ExperimentalEmptyAdapter,
  copilotRuntimeNextJSAppRouterEndpoint,
} from "@copilotkit/runtime";
import { HttpAgent } from "@ag-ui/client";
import { NextRequest } from "next/server";

// 1. You can use any service adapter here for multi-agent support. We use
//    the empty adapter since we're only using one agent.
const serviceAdapter = new ExperimentalEmptyAdapter();

// 2. Build a Next.js API route that handles the CopilotKit runtime requests.
export const POST = async (req: NextRequest) => {
  // Extract session ID from request headers
  const sessionId = req.headers.get("x-session-id");
  
  // Create HttpAgent with session ID header for this request
  const agentUrl = process.env.AGENT_API_URL || "http://localhost:5149";
  const httpAgent = new HttpAgent({ 
    url: agentUrl,
    headers: sessionId ? { "x-session-id": sessionId } : undefined,
  });
  
  // Create runtime with the configured agent
  const runtime = new CopilotRuntime({
    agents: {
      my_agent: httpAgent,
    },
  });
  
  const { handleRequest } = copilotRuntimeNextJSAppRouterEndpoint({
    runtime,
    serviceAdapter,
    endpoint: "/api/copilotkit",
  });
  return handleRequest(req);
};