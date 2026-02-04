# AGENTS.md

> **AI Agent Instructions for agentic-shell-dotnet**  
> Human developers should refer to [README.md](README.md) and `/specs/docs/` for documentation.

---

## Project Overview

**agentic-shell-dotnet** is a microservices-based AI agent application using Microsoft Agent Framework.

| Component | Stack |
|-----------|-------|
| **Frontend** | Next.js 16 + React 19 + TypeScript + CopilotKit |
| **Backend** | ASP.NET Core 10 + Microsoft Agent Framework |
| **Orchestration** | .NET Aspire |
| **Deployment** | Azure Container Apps + Azure AI services |
| **Status** | Prototype/Demo (not production-ready) |

**Key files**: `src/agentic-api/Program.cs` (backend), `src/agentic-api/Workflows/DummyWorkflow.cs` (demo workflow), `src/agentic-ui/app/page.tsx` (frontend), `apphost.cs` (Aspire orchestration)

---

## Quick Commands

```bash
# Setup
az login && azd auth login && azd provision

# Run locally (required for proper env injection)
aspire run   # Dashboard: http://localhost:15888 | UI: http://localhost:3000 | API: http://localhost:5149

# Build & Deploy
./build.sh                     # Build all
azd deploy                     # Deploy to Azure
dotnet test tests/agentic-api-tests/agentic-api-tests.csproj  # Backend tests
cd src/agentic-ui && npm test  # Frontend tests
```

---

## Technology Stack (Critical Versions)

**Backend**: .NET 10.0, `Microsoft.Agents.AI.*` 1.0.0-preview.251125.1, `Azure.AI.OpenAI` 2.5.0-beta.1  
**Frontend**: Next.js 16.0.3, React 19.2.0, `@copilotkit/react-*` ^1.10.6, `@ag-ui/client` ^0.0.41

⚠️ **Do not change versions without testing** - 70% of dependencies are preview/beta.

---

## Adding a New Agent Workflow

### 1. Create Workflow File (`src/agentic-api/Workflows/MyWorkflow.cs`)

```csharp
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace agentic_api.Workflows;

// Input Executor: Receives chat input, converts to internal event
public class MyChatInputExecutor(ILogger<MyChatInputExecutor> logger)
    : ExecutorBase<IConversationUpdate, UserInputEvent>(logger)
{
    protected override ValueTask ExecuteAsync(IConversationUpdate input, CancellationToken ct)
    {
        var userMessage = input switch
        {
            ChatMessage msg => msg.Text,
            TurnToken token => token.Text,
            _ => "Hello"
        };
        return ValueTask.FromResult(new ExecutionResult<UserInputEvent>(new UserInputEvent { Input = userMessage }));
    }
}

// Processing Executor: Handles business logic, calls AI models
public class MyProcessingExecutor(ILogger<MyProcessingExecutor> logger, IChatClient chatClient)
    : ExecutorBase<UserInputEvent, WorkflowOutputEvent>(logger)
{
    protected override async ValueTask ExecuteAsync(UserInputEvent input, CancellationToken ct)
    {
        var response = await chatClient.CompleteAsync($"User message: {input.Input}", cancellationToken: ct);
        return new ExecutionResult<WorkflowOutputEvent>(new WorkflowOutputEvent(response.Message.Text ?? "Hello!"));
    }
}

// Factory: Builds the workflow graph
public class MyWorkflowFactory(
    ILogger<MyChatInputExecutor> inputLogger,
    ILogger<MyProcessingExecutor> processingLogger,
    IChatClient chatClient)
{
    public Workflow BuildWorkflow(string name)
    {
        var inputExecutor = new MyChatInputExecutor(inputLogger);
        var processingExecutor = new MyProcessingExecutor(processingLogger, chatClient);
        
        return new WorkflowBuilder(inputExecutor)
            .WithName(name)
            .AddEdge(inputExecutor, processingExecutor)
            .WithOutputFrom(processingExecutor)  // Required for streaming to UI
            .Build();
    }
}
```

### 2. Register in `Program.cs`

```csharp
builder.Services.AddSingleton<MyWorkflowFactory>();
builder.AddWorkflow("MyWorkflow", (sp, name) => 
    sp.GetRequiredService<MyWorkflowFactory>().BuildWorkflow(name))
    .AddAsAIAgent();  // Wraps with AGUIWorkflowAgent for AGUI protocol compatibility
```

**`.AddAsAIAgent()` does:**
- Wraps workflow with `AGUIWorkflowAgent` for AGUI protocol
- Registers agent with AGUI endpoint (via `app.MapAGUI()`)
- Makes workflow accessible via `/api/copilotkit`

### Using IImageGenerator for Text-to-Image Generation

**IImageGenerator** provides text-to-image generation using Azure AI Foundry models (Flux, GPT-Image, DALL-E, etc.).

**1. Register in `Program.cs`** (deployment name auto-populated by `azd provision`):

```csharp
string imageDeploymentName = builder.Configuration["AZURE_IMAGE_MODEL_DEPLOYMENT_NAME"]
    ?? throw new InvalidOperationException("AZURE_IMAGE_MODEL_DEPLOYMENT_NAME is not set.");

#pragma warning disable MEAI001
builder.Services.AddSingleton(_ =>
    new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        .GetImageClient(imageDeploymentName)
        .AsIImageGenerator());
#pragma warning restore MEAI001
```

**2. Inject and use in executors**:

```csharp
public class MyExecutor(ILogger<MyExecutor> logger, IChatClient chatClient, IImageGenerator imageGenerator)
    : Executor<InputEvent, OutputEvent>("MyExecutor")
{
    public override async ValueTask<OutputEvent> HandleAsync(InputEvent input, IWorkflowContext context, CancellationToken ct)
    {
        var options = new ImageGenerationOptions
        {
            MediaType = "image/png",
            ResponseFormat = ImageGenerationResponseFormat.Hosted,  // or .Base64
            Size = "1024x1024",  // Model-dependent
            Quality = "standard",  // or "hd"
            Style = "natural"  // or "vivid"
        };
        
        var response = await imageGenerator.GenerateImagesAsync("A futuristic city at sunset", options, ct);
        var dataContent = response.Contents.OfType<DataContent>().First();
        
        return new OutputEvent { Text = "Image generated!", ImageUrl = dataContent.Uri?.ToString() };
    }
}
```

**3. Send to UI** via `YieldOutputAsync` or return with `ImageUrl` property:

```csharp
await context.YieldOutputAsync(new AgentMessage { Text = "Here's your image:", ImageUrl = dataContent.Uri.ToString() });
```

**Frontend handling** (CustomMessageRenderer.tsx):

```typescript
export function CustomMessageRenderer({ message }: { message: { text?: string; imageUrl?: string } }) {
  return (
    <div className="agent-message">
      {message.text && <p>{message.text}</p>}
      {message.imageUrl && <img src={message.imageUrl} alt="Generated by AI" className="max-w-full rounded-lg mt-2" />}
    </div>
  );
}
```

**Best Practices**: Wrap in try-catch, log prompts/URLs, use `Hosted` for UI display, image generation takes 10-30s.

### Implementing Human-in-the-Loop Approval

**HITL** allows agents to request user approval before proceeding. Requires backend-frontend coordination.

#### Backend: Create Approval Requests

**1. Use `ApprovalRequestHelper`** (`src/agentic-api/ApprovalRequestHelper.cs`):

```csharp
public static class ApprovalRequestHelper
{
    public static FunctionApprovalRequestContent CreateApprovalRequest(
        string functionName,
        Dictionary<string, object?> arguments)
    {
        return new FunctionApprovalRequestContent(
            Guid.NewGuid().ToString(),
            new FunctionCallContent(functionName, functionName, arguments: arguments)
        );
    }
}
```

**2. Return approval requests from executors** instead of direct responses:

```csharp
public override async ValueTask<AIContent> HandleAsync(UserInputEvent input, IWorkflowContext context, CancellationToken ct)
{
    var responseText = await GenerateContent(input);
    
    // Return approval request instead of direct response
    return ApprovalRequestHelper.CreateApprovalRequest(
        functionName: "approve_copyright_command",  // Must match frontend hook name
        arguments: new Dictionary<string, object?> { { "copyright", responseText } }
    );
}
```

**3. Handle approval responses in input executor**:

```csharp
private async ValueTask<UserInputEvent> HandleChatMessagesAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken ct)
{
    // Check for approval response (comes back as ChatRole.Tool with FunctionResultContent)
    var approvalMessage = messages.LastOrDefault(m => m.Role == ChatRole.Tool);
    var functionResult = approvalMessage?.Contents.OfType<FunctionResultContent>().FirstOrDefault();
    
    var textApproved = functionResult?.Result?.ToString()?.Contains("text-approved");
    var textRejected = functionResult?.Result?.ToString()?.Contains("text-rejected");
    
    // Route based on approval status
    if (textApproved == true) return new UserInputEvent { NextStep = WorkflowSteps.GenerateImage };
    if (textRejected == true) return new UserInputEvent { NextStep = WorkflowSteps.RegenerateText };
    return new UserInputEvent { NextStep = WorkflowSteps.GenerateText };  // Start workflow
}
```

**Note**: `AGUIWorkflowAgent` automatically converts `FunctionApprovalRequestContent` to `FunctionCallContent` for the frontend (auto-registered via `.AddAsAIAgent()`).

#### Frontend: Add useHumanInTheLoop Hook

```typescript
import { useHumanInTheLoop } from "@copilotkit/react-core";

export default function Page() {
  const [approvedContent, setApprovedContent] = useState<string | null>(null);

  useHumanInTheLoop({
    name: "approve_copyright_command",  // Must match backend functionName
    description: "Ask the user to approve the generated text content",
    parameters: [
      { name: "copyright", type: "string", description: "The text to approve", required: true },
    ],
    render: ({ args, respond }) => {
      if (!respond) return <></>;
      return (
        <div className="approval-container">
          <pre>{args.copyright}</pre>
          <button onClick={() => { setApprovedContent(args.copyright); respond("text-approved"); }}>✓ Approve</button>
          <button onClick={() => { respond("text-rejected"); }}>✗ Reject</button>
        </div>
      );
    },
  });
  // ...
}
```

**Key matching requirements**:
- Frontend `name` ↔ Backend `functionName`
- Frontend `parameters[].name` ↔ Backend `arguments` dictionary keys
- Frontend `respond()` value ↔ Backend `FunctionResultContent.Result` check

#### Workflow with Conditional Routing

```csharp
public Workflow BuildWorkflow(string name)
{
    var chatInput = new DummyChatInputExecutor(_inputLogger);
    var textGenerator = new TextGeneratorExecutor(_logger, _chatClient);
    var imageGenerator = new ImageGeneratorExecutor(_logger, _chatClient, _imageGenerator);

    return new WorkflowBuilder(chatInput)
        .WithName(name)
        .AddSwitch(chatInput, switchBuilder =>
            switchBuilder
                .AddCase(input => input?.NextStep == WorkflowSteps.GenerateText, textGenerator)
                .AddCase(input => input?.NextStep == WorkflowSteps.GenerateImage, imageGenerator)
                .WithDefault(textGenerator))
        .WithOutputFrom(textGenerator)
        .WithOutputFrom(imageGenerator)
        .Build();
}
```

**Execution flow**: User → InputExecutor (checks approvals) → Executor → Returns `FunctionApprovalRequestContent` → Frontend shows UI → User responds → InputExecutor routes to next step

#### HITL Best Practices & Troubleshooting

- **Naming**: Use descriptive names like `approve_copyright_command`, `validate_data_command`
- **Response values**: Use clear values like `"text-approved"`, `"image-rejected"`
- **Approval UI not appearing?** Check `functionName` matches exactly, executor registered with `.WithOutputFrom()`
- **Response not reaching backend?** Ensure `respond()` called with string, check `ChatRole.Tool` messages in input executor

---

## Code Style & Conventions

### Backend (.NET)
- **File naming**: PascalCase (`DummyWorkflow.cs`, `AGUIWorkflowAgent.cs`)
- **Nullable reference types**: Enabled | **Target**: `net10.0`
- **Naming**: PascalCase public, `_camelCase` private fields
- **Async**: Always `async`/`await`, never `.Result` or `.Wait()`
- **DI**: Constructor injection only | **Logging**: `ILogger<T>`

### Frontend (TypeScript/React)
- **File naming**: kebab-case (`chat-header.tsx`) | **Components**: PascalCase
- **TypeScript**: Strict mode | **React 19**: Functional components + hooks only
- **Props**: Always type explicitly | **Styling**: Tailwind CSS

### Bicep (Infrastructure)
- **File naming**: kebab-case (`main.bicep`, `ai-project.bicep`)
- Use Azure Verified Modules (`br/public:avm/...`) when available
- Use `@description` decorator for parameters

---

## Environment Variables

**Backend** (auto-populated by `azd provision`):
```bash
AZURE_OPENAI_ENDPOINT=https://YOUR-RESOURCE.openai.azure.com/
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-5-mini
AZURE_IMAGE_MODEL_DEPLOYMENT_NAME=<auto-generated>
AZURE_COSMOS_ENDPOINT=https://YOUR-COSMOS.documents.azure.com:443/
AZURE_STORAGE_ENDPOINT=https://YOUR-STORAGE.blob.core.windows.net/
# Auth: az login OR AZURE_TENANT_ID + AZURE_CLIENT_ID + AZURE_CLIENT_SECRET
```

**Frontend**: `AGENT_API_URL=http://localhost:5149` (local) or Container Apps URL (prod)

**Do NOT commit**: `apphost.settings.json`, `.env`, `.env.local`, any secrets

---

## Adding New Environment Variables to Aspire

When adding a new Azure resource that requires endpoint configuration:

### 1. Update `apphost.cs`

Add the parameter and wire it to the API service:

```csharp
var myEndpoint = builder.AddParameter("myEndpoint");

var api = builder.AddCSharpApp("agentic-api", "./src/agentic-api")
    .WithEnvironment("AZURE_MY_ENDPOINT", myEndpoint);
```

### 2. Update `apphost.settings.template.json`

Add the new parameter with empty default:

```json
{
    "Parameters": {
        "openAiEndpoint": "",
        "myEndpoint": ""
    }
}
```

### 3. Update postprovision scripts

Both `infra/scripts/postprovision.sh` and `infra/scripts/postprovision.ps1` must be updated to:
1. Read the azd environment variable (e.g., `AZURE_RESOURCE_MY_SERVICE_ID`)
2. Add validation/warning for missing values
3. Write to the settings JSON file

**Bash** (`postprovision.sh`):
```bash
MY_ENDPOINT="${AZURE_RESOURCE_MY_SERVICE_ID:-}"

if [ -z "$MY_ENDPOINT" ]; then
    echo -e "\033[0;33mWarning: AZURE_RESOURCE_MY_SERVICE_ID environment variable is not set\033[0m"
fi

# In jq command, add:
--arg myEndpoint "$MY_ENDPOINT" \
# And in the filter:
| .Parameters.myEndpoint = $myEndpoint
```

**PowerShell** (`postprovision.ps1`):
```powershell
$MY_ENDPOINT = if ($envVars.ContainsKey('AZURE_RESOURCE_MY_SERVICE_ID')) { $envVars['AZURE_RESOURCE_MY_SERVICE_ID'] } else { "" }

if ([string]::IsNullOrEmpty($MY_ENDPOINT)) {
    Write-Host "Warning: AZURE_RESOURCE_MY_SERVICE_ID environment variable is not set" -ForegroundColor Yellow
}

# Update settings:
$settingsContent.Parameters.myEndpoint = $MY_ENDPOINT
```

### 4. Use in API code (`Program.cs`)

```csharp
string myEndpoint = builder.Configuration["AZURE_MY_ENDPOINT"]
    ?? throw new InvalidOperationException("AZURE_MY_ENDPOINT is not set.");

builder.Services.AddSingleton(_ => new MyClient(myEndpoint, credential));
```

**Key pattern**: Environment variables flow from Bicep → azd env → postprovision scripts → `apphost.settings.json` → Aspire parameters → API environment variables → `IConfiguration`.

---

## Architecture Patterns

### Workflow Executor Pattern
```
User Input → InputExecutor → [ProcessingExecutors...] → OutputExecutor → Response
```

### Streaming Messages to UI

Use `YieldOutputAsync` to stream intermediate results:
```csharp
await context.YieldOutputAsync(new AgentMessage { Text = "Processing..." });
```

**Critical**: Every executor calling `YieldOutputAsync` must be registered with `.WithOutputFrom()`:
```csharp
var workflow = new WorkflowBuilder(inputExecutor)
    .AddEdge(inputExecutor, processingExecutor)
    .WithOutputFrom(processingExecutor)  // Required!
    .Build();
```

### AGUI Protocol Adapter
`AGUIWorkflowAgent` wraps workflows for AGUI compatibility. Auto-registered via `.AddAsAIAgent()`.

### Dependency Injection
**Always use constructor injection**, never service locator pattern.

---

## Observability

**Local**: Aspire Dashboard at `http://localhost:15888` (auto-opens with `aspire run`)
- Resources, Console logs, Structured logs, Traces, Metrics

**Production**: Application Insights (auto-injected connection string)

---

## Quick Reference

| Task | Command |
|------|---------|
| Run locally | `aspire run` |
| Build all | `./build.sh` |
| Deploy | `azd deploy` |
| Backend tests | `dotnet test tests/agentic-api-tests/agentic-api-tests.csproj` |
| Frontend tests | `cd src/agentic-ui && npm test` |

### Key Files
- `src/agentic-api/Program.cs` - Backend config
- `src/agentic-api/Workflows/DummyWorkflow.cs` - Demo workflow
- `src/agentic-api/AGUIWorkflowAgent.cs` - AGUI adapter
- `src/agentic-ui/app/page.tsx` - Frontend page
- `infra/main.bicep` - Infrastructure

### Known Limitations
- No authentication, input validation, rate limiting
- Cosmos DB & AI Search provisioned but unused
- Single agent (DummyWorkflow) only

---

**Last Updated**: December 15, 2025 | **Status**: Prototype/Demo