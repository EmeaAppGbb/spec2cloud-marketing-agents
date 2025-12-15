import { CopilotKit } from "@copilotkit/react-core"; 
import "@copilotkit/react-ui/styles.css";

export default function MarketingLayout({ children }: {children: React.ReactNode}) {
  return (
    <CopilotKit runtimeUrl="/api/copilotkit/marketing" agent="marketing_agent">
      {children}
    </CopilotKit>
  );
}
