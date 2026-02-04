import { useAzureMonitor } from "@azure/monitor-opentelemetry";

export function register() {
  // Only initialize Azure Monitor on the server side when connection string is available
  if (process.env.NEXT_RUNTIME === "nodejs" && process.env.APPLICATIONINSIGHTS_CONNECTION_STRING) {
    useAzureMonitor();
  }
}
