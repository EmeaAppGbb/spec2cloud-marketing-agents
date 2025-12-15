# Feature: Social Media Campaign Workflow

## Feature Overview

**Feature Name:** AI-Powered Social Media Campaign Creation & Publishing Workflow

**Business Purpose:** Enable users to create complete social media campaigns—from strategic planning through creative asset generation, multi-market localization, schedule creation, and automated publishing—through an intelligent multi-agent workflow with human oversight at key decision points.

**Current Status:** ❌ **Not Implemented**

## User Story

**As a** marketing professional or social media manager  
**I want to** provide a campaign brief and receive AI-generated strategic plans, creative assets (images and video), localized content, and a ready-to-publish schedule  
**So that** I can efficiently launch professional multi-market social media campaigns without manual coordination of multiple specialists

## Agent Pipeline

The workflow orchestrates five specialized agents in sequence:

**Campaign Planner → Creative Generator → Localizer → Schedule Creator → Instagram Publisher**

Each agent has a specific responsibility and produces outputs consumed by downstream agents or presented to users for approval.

## Functional Requirements

### FR-1: Campaign Planning & Strategic Input

**Requirement:** Users must be able to provide a campaign brief, and the system must create a strategic campaign plan with smart defaults to minimize user input burden

**Acceptance Criteria:**
- ✅ User can submit campaign brief through chat interface with minimal required information
- ✅ System applies intelligent defaults (2-week duration, Instagram/TikTok platforms, 18-45 age demographic)
- ✅ System only requests clarification if critical information is genuinely missing
- ✅ User receives strategic campaign plan for review
- ✅ Plan includes campaign objectives, target audience, platform selection, timeline, and budget considerations
- ✅ Clear confirmation that planning phase is complete and workflow is proceeding

**Input Requirements (Optional - Smart Defaults Applied):**
- Campaign objectives/goals
- Target audience demographics
- Platforms (default: Instagram, TikTok)
- Timeline (default: 2 weeks)
- Budget constraints (if any)
- Brand voice/tone preferences
- Key messages or themes

**Output Requirements:**
- Strategic campaign plan document
- Campaign parameters (objectives, audience, platforms, timeline)
- Completion confirmation flag indicating planning is finalized

### FR-1.1: Human Gate - Campaign Planning Clarification (Conditional)

**Requirement:** If the Campaign Planner determines that critical information is missing, it must request clarification from the user before proceeding

**Acceptance Criteria:**
- ✅ System identifies genuinely critical missing information (not covered by defaults)
- ✅ User is prompted with specific, focused questions
- ✅ User responses are captured and integrated into campaign plan
- ✅ Planning phase completes only after all critical questions are answered
- ✅ This gate is SKIPPED if campaign brief contains sufficient information

**User Actions:**
- **Provide Answers:** User responds to specific questions → Planning completes → Proceed to creative generation (FR-2)
- **Decline/Skip:** User chooses to proceed with defaults → Planning completes → Proceed to creative generation (FR-2)

### FR-2: Creative Asset Generation

**Requirement:** System must generate three creative assets for the campaign: two images and one video, each with accompanying captions and hashtags

**Acceptance Criteria:**
- ✅ TWO distinct social media images are generated reflecting campaign objectives and strategy
- ✅ ONE promotional video is generated aligned with campaign messaging
- ✅ Each asset includes optimized caption copy for social media engagement
- ✅ Each asset includes relevant hashtags appropriate for target platforms and audience
- ✅ All three assets are presented to user for review when generation completes
- ✅ Asset metadata includes file references, captions, and hashtags for each item
- ✅ Generation occurs only after campaign planning is finalized

**Output Requirements:**
- Two social media image assets (visual format suitable for Instagram/TikTok)
- One promotional video asset (duration suitable for social platforms)
- Caption copy for each asset (optimized for engagement and character limits)
- Hashtag sets for each asset (relevant, trending, platform-appropriate)
- Asset metadata containing file references, URLs, captions, and hashtags

### FR-2.1: Human Gate - Creative Asset Approval

**Requirement:** After creative assets are generated, a human user must review and either approve all assets or provide feedback for regeneration before the workflow proceeds to localization

**Acceptance Criteria:**
- ✅ Workflow pauses after asset generation and awaits human review
- ✅ User is presented with all three assets (2 images + 1 video) for review
- ✅ Each asset is displayed with its caption and hashtags
- ✅ User interface provides clear approval option (e.g., "Approve All Assets")
- ✅ User interface provides feedback option for requesting revisions
- ✅ User can provide specific feedback text describing desired changes
- ✅ Feedback is used to regenerate assets incorporating user requirements
- ✅ Regenerated assets return to human approval (FR-2.1)
- ✅ Approval loop continues until user explicitly approves all assets
- ✅ System clearly indicates it is waiting for user approval

**User Actions:**
- **Approve:** User confirms all assets meet requirements → Proceed to market selection (FR-3)
- **Reject with Feedback:** User provides specific feedback on what needs improvement → Creative Generator regenerates assets using feedback → Return to human approval (FR-2.1)

**System Behavior:**
- Display clear "Review Creative Assets" prompt in UI
- Present all assets in viewable format with captions and hashtags
- Show approval button prominently
- Provide feedback input field for rejection
- Capture user feedback and pass to Creative Generator for regeneration
- Loop continues until human approval is granted

### FR-3: Target Market Selection

**Requirement:** After creative asset approval, the user must select which geographic markets require localized content before the workflow proceeds to translation

**Acceptance Criteria:**
- ✅ Workflow pauses after asset approval and awaits market selection
- ✅ User is prompted to select target markets for localization
- ✅ System provides list of available market options (Spain, Mexico, France, Germany, Brazil, etc.)
- ✅ User can select one or multiple markets
- ✅ User can skip localization entirely (English-only campaign)
- ✅ Selected markets are captured and passed to localization phase
- ✅ System clearly indicates it is waiting for market selection

**Available Market Options:**
- Spain (Spanish - European)
- Mexico (Spanish - Latin American)
- France (French)
- Germany (German)
- Brazil (Portuguese)
- Italy (Italian)
- Japan (Japanese)
- Other markets as needed

**User Actions:**
- **Select Markets:** User chooses one or more target markets (e.g., "Spain and Mexico") → Proceed to localization (FR-4)
- **Skip Localization:** User declines localization → Skip FR-4 → Proceed directly to schedule creation (FR-5) with English content only

**System Behavior:**
- Display clear "Select Target Markets" prompt
- Present market options in selectable format (checkboxes, multi-select, or natural language)
- Confirm selected markets before proceeding
- Pass market selections to localization phase

### FR-4: Content Localization

**Requirement:** For each selected target market, the system must translate captions and hashtags while maintaining marketing effectiveness and cultural appropriateness

**Acceptance Criteria:**
- ✅ Localization occurs only after market selection is confirmed
- ✅ All asset captions are translated for each selected market
- ✅ All asset hashtags are translated or adapted for each selected market
- ✅ Translations maintain marketing tone, persuasiveness, and emotional impact
- ✅ Translations follow market-specific conventions (formality, idioms, cultural nuances)
- ✅ Hashtags are adapted to local trending topics and search behavior where appropriate
- ✅ Original English content is preserved alongside translations
- ✅ Localized content is presented to user when translation completes
- ✅ This phase is SKIPPED if user selected no markets in FR-3

**Output Requirements:**
- Translated captions for each asset in each selected market
- Adapted hashtags for each asset in each selected market
- Localized content maintains character limits for target platforms
- Original English content included in output for reference
- Clear labeling of which language/market each translation represents

**Quality Criteria:**
- Translations are grammatically correct and natural-sounding
- Marketing tone and persuasiveness are preserved
- Cultural appropriateness for target market
- Platform-specific conventions respected (Instagram/TikTok norms)
- Hashtag relevance and discoverability in target market

### FR-5: Publishing Schedule Creation

**Requirement:** System must generate a two-week content publishing schedule that includes all approved assets in both English and localized versions, optimized for platform algorithms and audience engagement times

**Acceptance Criteria:**
- ✅ Schedule is generated only after localization completes (or is skipped)
- ✅ Schedule spans two weeks (14 days) from current date
- ✅ Schedule includes all three assets (2 images + 1 video) in English
- ✅ Schedule includes all three assets in each localized language (if applicable)
- ✅ Each scheduled post includes date, time, platform, content type, language, and priority
- ✅ Posting times are optimized for target audience engagement (platform-specific best practices)
- ✅ Schedule balances content distribution across Instagram and TikTok
- ✅ Timezone information is included for each scheduled post
- ✅ Schedule is presented to user in readable format when generation completes

**Output Requirements:**
- Two-week posting schedule with specific dates and times
- Platform assignment for each post (Instagram, TikTok, or both)
- Content type indication (image or video)
- Language/market indication for each post
- Timezone information
- Priority or sequencing logic (if applicable)
- Rationale for timing choices (optional enhancement)

**Scheduling Criteria:**
- Posts distributed evenly across 14-day period
- Posting times aligned with peak engagement hours for target audience
- Platform-specific best practices respected (Instagram vs TikTok norms)
- Localized posts scheduled at appropriate times for target market timezones
- No conflicts or excessive posting on single day

### FR-5.1: Human Gate - Schedule Approval

**Requirement:** After the publishing schedule is generated, a human user must review and either approve the schedule or provide feedback for adjustments before the workflow proceeds to publishing

**Acceptance Criteria:**
- ✅ Workflow pauses after schedule generation and awaits human review
- ✅ User is presented with complete two-week schedule in readable format
- ✅ Schedule clearly shows dates, times, platforms, content, and languages
- ✅ User interface provides clear approval option (e.g., "Approve Schedule")
- ✅ User interface provides feedback option for requesting schedule changes
- ✅ User can provide specific feedback text describing desired adjustments
- ✅ Feedback is used to regenerate schedule incorporating user requirements
- ✅ Regenerated schedule returns to human approval (FR-5.1)
- ✅ Approval loop continues until user explicitly approves schedule
- ✅ System clearly indicates it is waiting for schedule approval

**User Actions:**
- **Approve:** User confirms schedule meets requirements → Proceed to Instagram publishing (FR-6)
- **Reject with Feedback:** User provides specific feedback on timing, distribution, or other schedule aspects → Schedule Creator regenerates schedule using feedback → Return to human approval (FR-5.1)

**System Behavior:**
- Display clear "Review Publishing Schedule" prompt in UI
- Present schedule in calendar or table format for easy review
- Show approval button prominently
- Provide feedback input field for rejection
- Capture user feedback and pass to Schedule Creator for regeneration
- Loop continues until human approval is granted

### FR-6: Instagram Post Publishing

**Requirement:** After schedule approval, the system must automatically publish the first scheduled post to Instagram as proof of end-to-end campaign execution

**Acceptance Criteria:**
- ✅ Publishing occurs only after schedule approval
- ✅ System identifies the first post in approved schedule
- ✅ First post is published to Instagram automatically
- ✅ Post includes correct asset (image or video), caption, and hashtags
- ✅ Post respects language/market selection from schedule
- ✅ User receives confirmation that post was successfully published
- ✅ Post ID and URL are captured and presented to user
- ✅ Publishing status is clearly indicated in UI

**Output Requirements:**
- Published Instagram post (live on platform)
- Post ID from Instagram
- Post URL for user verification
- Publishing status confirmation
- Timestamp of publication

**Error Handling (Business Requirements):**
- If publishing fails, user must be notified with clear error message
- Failed publishing should not block access to complete schedule
- User should have visibility into what succeeded vs failed

### FR-7: Final Campaign Delivery

**Requirement:** Upon completion of all workflow stages, deliver the complete campaign package to the user with confirmation of published content

**Acceptance Criteria:**
- ✅ User receives confirmation that workflow is complete
- ✅ User can access all approved creative assets (2 images + 1 video)
- ✅ User can access all captions and hashtags (English + localized)
- ✅ User can access complete two-week publishing schedule
- ✅ User receives confirmation of Instagram post publication with URL
- ✅ Clear indication of which assets/posts are live vs scheduled
- ✅ Workflow summary showing decisions made at each human gate

**Deliverables:**
- Three creative assets (2 images + 1 video) with captions and hashtags
- Localized content for all selected markets
- Approved two-week publishing schedule
- Confirmation of first Instagram post with URL
- Campaign summary document
- Status indicators for each workflow phase

## Non-Functional Requirements

### NFR-1: Performance

**Requirement:** Complete social media campaign workflow should execute within reasonable time for automated steps, with human approval wait times excluded from performance targets

**Target Performance (Automated Steps Only):**
- Campaign planning: < 10 seconds
- Creative asset generation (2 images + 1 video): < 45 seconds total
- Localization per market: < 10 seconds per market
- Publishing schedule creation: < 8 seconds
- Instagram post publishing: < 15 seconds
- Total automated workflow time (no localization): < 90 seconds
- Total automated workflow time (3 markets): < 120 seconds

**Human Approval Wait Times (Excluded from Performance Targets):**
- Campaign planning clarification (if needed): Wait indefinitely for user response
- Creative asset approval: Wait indefinitely for user decision
- Market selection: Wait indefinitely for user selection
- Schedule approval: Wait indefinitely for user decision
- System must remain responsive during all wait periods
- User can take as long as needed to review and decide

**Total User Experience Time:**
- Automated workflow: ~90-120 seconds
- Human decision time: Variable (user-dependent, typically 5-10 minutes total)
- Realistic total time: 10-20 minutes including all human review gates

### NFR-2: Reliability

**Requirement:** Workflow must handle failures gracefully and not lose user input or approved content

**Acceptance Criteria:**
- ✅ Agent failures do not crash the entire workflow
- ✅ User campaign brief and all approval decisions are preserved throughout process
- ✅ Approved assets are retained even if downstream phases fail
- ✅ Workflow state can be recovered if interrupted between phases
- ✅ Clear error messages if any phase cannot complete
- ✅ Failed asset generation uses placeholder/fallback and continues workflow
- ✅ Failed publishing does not prevent schedule delivery to user

**Error Recovery Behaviors:**
- Failed image generation: Log error, use placeholder, continue workflow
- Failed video generation: Log error, skip video, continue with images only
- Failed localization: Log error, skip problematic market, continue with successful markets
- Failed schedule creation: Log error, provide basic schedule, continue workflow
- Failed Instagram publishing: Log error, provide schedule without publication confirmation

### NFR-3: Quality Assurance

**Requirement:** Human oversight gates ensure professional outputs meet user expectations

**Acceptance Criteria:**
- ✅ Human approval required at four critical decision points
- ✅ Users can reject and provide feedback at each gate
- ✅ Feedback is actionable and incorporated into regeneration
- ✅ No automated quality loop—human decisions are final
- ✅ Users maintain control over content, markets, and schedule
- ✅ Final outputs reflect user approvals and preferences

### NFR-4: Transparency & User Experience

**Requirement:** Users can understand workflow progress, see agent outputs, and make informed decisions at approval gates through clear visual presentation and content formatting

**Acceptance Criteria:**
- ✅ Workflow progress is visible to user throughout entire process
- ✅ Current workflow phase is clearly indicated
- ✅ Agent outputs are presented when each phase completes
- ✅ User knows when workflow is waiting for their input/decision
- ✅ Messages are displayed in chronological order with clear agent attribution
- ✅ Agent name/role is prominently displayed with each message
- ✅ Each agent type has a distinct visual identity in the UI (color, icon, styling)
- ✅ Structured data (campaign plans, schedules) is automatically formatted for readability
- ✅ No raw technical artifacts shown to users (JSON dumps, error stack traces, etc.)
- ✅ Content is rendered with appropriate formatting (markdown, tables, media previews)
- ✅ Error messages are user-friendly and actionable

**Visual Presentation Guidelines:**
- Campaign Planner: Displays strategic plan in structured format
- Creative Generator: Displays images/video with captions and hashtags
- Localizer: Displays translations organized by market
- Schedule Creator: Displays schedule in calendar or table format
- Instagram Publisher: Displays confirmation with post URL

### NFR-5: Scalability & Resource Management

**Requirement:** System must handle reasonable campaign volumes without degradation or excessive resource consumption

**Acceptance Criteria:**
- ✅ Support multiple concurrent users creating campaigns
- ✅ Asset generation does not exhaust model quota or rate limits
- ✅ Localization scales efficiently with number of markets (reasonable limit: 10 markets)
- ✅ Schedule creation handles two-week timeframe without performance issues
- ✅ Publishing integration respects platform API rate limits

**Resource Constraints:**
- Maximum 3 creative assets per campaign (2 images + 1 video)
- Maximum 10 target markets for localization
- Schedule limited to 14 days
- Single Instagram post per workflow execution

## User Workflows

### Primary Workflow: Create and Publish Social Media Campaign

1. **User Action:** Initiate social media campaign creation through chat interface
2. **User Action:** Provide campaign brief (objectives, audience, platforms, timeline, etc.)
3. **System Response:** Confirm brief received and campaign planning started
4. **System Action:** Generate strategic campaign plan with smart defaults
5. **System Response:** Display campaign plan
6. **System Action (Conditional):** Request clarification if critical info missing (HUMAN GATE 1)
7. **User Action (Conditional):** Provide answers or proceed with defaults
8. **System Action:** Generate 3 creative assets (2 images + 1 video) with captions/hashtags
9. **System Response:** Display all assets for review
10. **User Action:** Approve assets or provide feedback for regeneration (HUMAN GATE 2)
11. **System Action:** Prompt for target market selection
12. **User Action:** Select target markets for localization or skip (HUMAN GATE 3)
13. **System Action:** Translate captions/hashtags for selected markets
14. **System Response:** Display localized content
15. **System Action:** Generate two-week publishing schedule
16. **System Response:** Display schedule for review
17. **User Action:** Approve schedule or provide feedback for adjustments (HUMAN GATE 4)
18. **System Action:** Publish first post to Instagram
19. **System Response:** Display publication confirmation with post URL
20. **System Response:** Deliver complete campaign package (assets, schedule, confirmations)


### Alternate Workflow: Skip Localization

**Scenario:** User wants English-only campaign without localization

1. Follow primary workflow steps 1-10 (through creative asset approval)
2. **User Action:** Skip market selection or indicate "English only" (HUMAN GATE 3)
3. **System Action:** Skip FR-4 (localization phase)
4. **System Action:** Generate schedule with English content only
5. Continue with steps 15-20 (schedule approval and publishing)

### Alternate Workflow: Early Termination

**Scenario:** User requests workflow cancellation or critical error occurs

1. **User Action:** Request to cancel workflow at any approval gate
2. **System Response:** Confirm cancellation
3. **System Action:** Save any work completed up to that point
4. **System Response:** Return partial results (campaign plan, assets, localized content, or schedule) if any phase completed
5. **System Response:** Indicate which phase was incomplete

## Dependencies

### External Services
- **Azure OpenAI** - Required for AI-powered campaign planning, content creation, and translation
- **GPT-Image (or equivalent)** - Required for social media image generation
- **Sora (or equivalent video generation)** - Required for promotional video creation
- **Instagram Publishing Platform** - Required for automated post publishing (social media API integration)

### Integration Points
- **Chat Interface** - Campaign brief input, agent outputs display, and approval interactions
- **Agent Orchestration System** - Coordinates five agents and manages workflow state across phases
- **Social Media API** - Instagram post publishing with media upload capability
- **Asset Storage** - Temporary storage for generated images and videos during workflow

## Data Model

### Campaign Input (Brief)

**Required Fields:**
- **Campaign Objectives**: Goals and desired outcomes
- **Target Audience**: Description of intended audience demographics

**Optional Fields (Smart Defaults Applied):**
- **Platforms**: Social media platforms (default: Instagram, TikTok)
- **Timeline**: Campaign duration (default: 2 weeks)
- **Budget**: Budget constraints if any
- **Tone/Style**: Desired communication style
- **Brand Guidelines**: Specific brand requirements or constraints
- **Key Messages**: Core themes or value propositions

### Workflow State

**State Tracking:**
- Current phase (planning/planning-clarification/creative-generation/asset-approval/market-selection/localization/schedule-creation/schedule-approval/publishing/completed)
- Human approval status:
  - Planning clarification completion (skipped/completed)
  - Asset approval status (pending/approved/rejected-with-feedback)
  - Market selection status (pending/selected/skipped)
  - Schedule approval status (pending/approved/rejected-with-feedback)
- Generated outputs:
  - Campaign plan document
  - Creative assets (2 images + 1 video) with metadata
  - Captions and hashtags (English + localized)
  - Localized content by market
  - Publishing schedule
  - Instagram post confirmation

### Campaign Output

**Deliverables:**
- **Campaign Plan**: Strategic plan with objectives, audience, platforms, timeline
- **Creative Assets**: 
  - 2 social media images (PNG/JPEG with URLs)
  - 1 promotional video (MP4 or equivalent with URL)
- **Content**:
  - Captions for each asset (English + localized)
  - Hashtags for each asset (English + localized)
- **Localized Content**: Translations organized by target market
- **Publishing Schedule**: Two-week schedule with dates, times, platforms, languages
- **Published Post**: Confirmation of first Instagram post with post ID and URL
- **Workflow Metadata**:
  - Campaign plan approval status
  - Asset regeneration count (if feedback provided)
  - Selected markets list
  - Schedule revision count (if feedback provided)
  - Publishing status and timestamp
  - Any warnings or errors encountered

## Human-in-the-Loop Design Principles

### Overview

This workflow incorporates human oversight at four strategic decision points to ensure that AI-generated campaign elements meet user expectations and business requirements. The human maintains control over content quality, market reach, and publishing strategy.

### Approval Gates

**Gate 1: Planning Clarification (FR-1.1) - Conditional**
- **Timing:** After campaign brief submission, only if critical information is missing
- **Decision:** Provide answers or proceed with defaults
- **Effect:**
  - Answer questions → Planning completes with user input
  - Skip/decline → Planning completes with smart defaults
- **Rationale:** Minimize user burden while ensuring campaign has necessary strategic direction; most campaigns can proceed with defaults

**Gate 2: Creative Asset Approval (FR-2.1) - Required**
- **Timing:** After 3 assets generated (2 images + 1 video)
- **Decision:** Approve all assets or Reject with feedback
- **Effect:**
  - Approve → Proceed to market selection
  - Reject → Regenerate assets incorporating feedback → Return to human for approval
- **Rationale:** Creative assets represent the campaign's visual identity and must meet brand standards and user expectations; subjective quality requires human judgment

**Gate 3: Market Selection (FR-3) - Required**
- **Timing:** After creative asset approval
- **Decision:** Select one or more markets, or skip localization
- **Effect:**
  - Select markets → Proceed to localization
  - Skip → Proceed to schedule creation (English-only)
- **Rationale:** Business decision about which geographic markets to target; requires knowledge of marketing strategy, budget, and resources

**Gate 4: Schedule Approval (FR-5.1) - Required**
- **Timing:** After two-week publishing schedule is generated
- **Decision:** Approve schedule or Reject with feedback
- **Effect:**
  - Approve → Proceed to Instagram publishing
  - Reject → Regenerate schedule incorporating feedback → Return to human for approval
- **Rationale:** Publishing timing and distribution strategy must align with business priorities, events, and marketing calendar; requires human judgment about optimal timing

### Feedback Loop Behavior

**Human Feedback Processing:**
1. User provides free-text feedback explaining what needs improvement
2. Feedback is passed to appropriate agent (Creative Generator or Schedule Creator)
3. Agent uses original campaign details + previous output + human feedback to regenerate
4. Regenerated output returns to human for approval
5. Loop continues until human approves (no maximum iteration limit on human feedback cycles)

**No Automated Quality Loops:**
- This workflow does NOT include automated review/revision cycles
- All quality decisions are made by humans at approval gates
- Simpler workflow with faster execution
- Human is the sole quality arbiter

### User Experience Considerations

**Wait State Indicators:**
- Clear visual indication that workflow is paused and awaiting human decision
- Distinct UI styling for approval prompts vs. informational messages
- Prominent, accessible approve/reject buttons or selection controls
- Feedback text input should be available when rejecting
- Clear labeling of what is being approved (assets, schedule, etc.)

**Decision Support:**
- Present relevant context (campaign objectives, target audience, platform selection) during approval
- For creative assets: Display images and video with captions and hashtags visible
- For market selection: Show available market options clearly
- For schedule: Show calendar or table format with all schedule details visible
- Provide clear instructions for each approval gate

**Response Time:**
- No system timeout - human can take unlimited time to decide
- System must remain responsive during wait periods
- Workflow state preserved across user sessions
- Consider future: notifications when approval is needed (email, Slack, mobile push)

## Configuration Requirements

### Workflow Configuration

**Smart Defaults:**
- Campaign duration: 2 weeks
- Platforms: Instagram, TikTok
- Target audience age: 18-45 years
- Posting frequency: Distributed evenly across 14 days

**Limits and Constraints:**
- Maximum 3 creative assets per campaign (2 images + 1 video)
- Maximum 10 target markets for localization
- Schedule limited to 14 days
- Single automated Instagram post per workflow execution
- Timeout per agent action: 60 seconds
- Total workflow timeout: 10 minutes (automated steps only, excluding human wait time)

### Agent Configuration

**Agent Roles:**
- **Campaign Planner Agent**: Strategic planning with smart defaults and clarification requests
- **Creative Generator Agent**: 2 images + 1 video with captions and hashtags
- **Localizer Agent**: Multi-market translation of captions and hashtags
- **Schedule Creator Agent**: Two-week publishing schedule with optimal timing
- **Instagram Publisher Agent**: Automated posting to Instagram platform

**Agent Capabilities:**
- Campaign Planner: Accepts minimal input, applies smart defaults, requests clarification only if critical
- Creative Generator: Generates 3 distinct assets with social media-optimized captions and hashtags
- Localizer: Translates while maintaining marketing tone and cultural appropriateness
- Schedule Creator: Distributes content across 14 days with platform-optimized timing
- Instagram Publisher: Posts first scheduled item with media upload

**Message Output Requirements:**
- Messages displayed when each phase completes
- Clear agent identification with each message
- Messages displayed in chronological order
- Structured data (plans, schedules) formatted for readability
- Media assets (images, video) displayed inline where possible

**UI Visual Design Requirements (Front-End Implementation):**
- **Campaign Planner Messages**: Displayed with distinct visual identity (color, icon)
- **Creative Generator Messages**: Displayed with distinct visual identity showcasing assets
- **Localizer Messages**: Displayed with distinct visual identity organizing translations by market
- **Schedule Creator Messages**: Displayed with distinct visual identity showing calendar/table
- **Instagram Publisher Messages**: Displayed with distinct visual identity confirming publication
- **System Messages**: Displayed with neutral styling for workflow status updates
- **Agent Badge**: Each message includes visible badge or label with agent name and role
- **Consistency**: Visual styling remains consistent throughout workflow execution
- **Accessibility**: Sufficient contrast and clear labeling for all interactive elements

**Message Content Formatting Requirements:**
- **JSON Detection**: Frontend must automatically detect JSON content in agent responses
- **JSON Parsing**: Valid JSON must be parsed and displayed as formatted, readable structure
- **JSON Rendering**: Use appropriate UI components (collapsible sections, key-value pairs, syntax highlighting)
- **Structured Data**: Display objects and arrays in human-readable format with proper indentation
- **Fallback Rendering**: If JSON parsing fails, display as plain text with proper line breaks
- **Markdown Support**: Support basic markdown formatting (bold, italic, lists, code blocks) in agent messages
- **Long Content**: Implement scrolling or truncation with "expand" option for lengthy responses
- **No Raw Dumps**: Never display raw JSON strings or escaped characters to end users

## Error Handling

### Required Error Handling Capabilities

**Workflow-Level Requirements:**
- Handle agent failures without losing campaign brief or approved content
- Retry transient failures automatically (network issues, temporary API unavailability)
- Escalate to user with clear messaging if workflow cannot continue
- Preserve all generated content and human decisions for recovery
- Allow workflow restart from last successful approval gate

**Agent-Level Requirements:**
- Handle asset generation failures with graceful fallback (placeholders, skip failed assets, continue with successful ones)
- Manage timeout scenarios without blocking entire workflow
- Provide meaningful, actionable error messages to user
- Log all errors with full context for debugging and improvement
- Distinguish between retryable and fatal errors

**User-Facing Error Experience:**
- Clear notification explaining what failed and why
- Option to retry failed phase or entire workflow
- Access to partial results (campaign plan, assets, localized content, or schedule)
- Ability to export partial campaign deliverables
- Support for manual intervention (e.g., manual Instagram posting if automated publishing fails)
- Contact information or escalation path for unrecoverable failures

**Current State:** Business requirements defined; implementation pending

## Limitations and Known Issues

### Design Considerations

1. **Linear Workflow Progression**
   - Each phase must complete before next phase begins
   - No parallel exploration of multiple creative directions
   - User must approve assets before seeing localization or schedule
   - Cannot preview schedule without committing to asset selection
   - Total time increases with each human rejection and regeneration cycle

2. **Asset Generation Constraints**
   - Fixed 3-asset configuration (2 images + 1 video) per campaign
   - No option to generate additional assets beyond initial set
   - No variation options within video generation (single video per attempt)
   - Asset regeneration replaces all assets, not individual ones
   - Video generation may be slower and less reliable than image generation

3. **Localization Scope**
   - Translation quality depends on model capabilities for each language
   - Limited to caption and hashtag translation (no image text localization)
   - No cultural adaptation beyond translation (idioms, references, humor)
   - No validation of translated hashtag effectiveness in target markets
   - Maximum 10 markets to manage performance and complexity

4. **Publishing Automation Limitations**
   - Only Instagram supported for automated publishing in initial version
   - TikTok and other platforms require manual posting from schedule
   - Single post published per workflow (first scheduled item only)
   - No bulk publishing or automated follow-up posts
   - Publishing failure does not retry automatically—manual intervention required

5. **Human Approval Experience**
   - No time limits on human decisions—workflow can pause indefinitely
   - No mechanism to escalate or timeout approval decisions
   - No collaborative approval (multiple stakeholders voting or discussing)
   - Feedback quality and specificity vary by user—no guidance prompts provided
   - No comparison to previous versions when rejecting with feedback
   - Approval state not preserved across browser sessions (requires implementation)

6. **Schedule Creation Constraints**
   - Fixed 14-day timeframe (2 weeks)
   - Limited customization of posting times and frequency
   - Schedule distribution logic is opaque to user
   - No integration with external marketing calendars or campaign timelines
   - No consideration of holidays, events, or blackout dates

7. **UI Accessibility and Usability**
   - Visual distinction relies heavily on color coding
   - Users with color vision deficiencies need additional cues (agent badges/labels provide this)
   - Long workflows produce many messages—may require scrolling and search
   - Large JSON responses may impact frontend performance
   - Mobile experience may be suboptimal for image comparison and schedule review

8. **Data and State Management**
   - Campaign brief and decisions not automatically saved across sessions
   - No version history or ability to revert to previous approved assets
   - Partial results may be lost if workflow is interrupted
   - No mechanism to clone or reuse campaigns with modifications

## Future Enhancements (Not Implemented)

### Potential Improvements

1. **Expanded Platform Support**
   - TikTok automated publishing integration
   - Facebook, Twitter/X, LinkedIn support
   - YouTube Shorts and Reels integration
   - Multi-platform simultaneous posting
   - Platform-specific asset optimization (aspect ratios, durations, formats)

2. **Advanced Scheduling Capabilities**
   - Customizable campaign duration (beyond 14 days)
   - Integration with external marketing calendars
   - Holiday and event awareness
   - Time zone optimization per market
   - Bulk publishing for entire schedule
   - Automated follow-up posts and drip campaigns

3. **Enhanced Localization**
   - Image text translation (text overlay on images)
   - Cultural adaptation beyond translation (idioms, references, humor)
   - Market-specific asset variations (different images for different markets)
   - Localized video subtitles and voiceovers
   - Validation of hashtag effectiveness per market
   - Support for right-to-left languages

4. **Workflow Customization**
   - Configurable asset count (more or fewer than 3 assets)
   - Optional workflow phases (legal review, stakeholder approval, brand compliance check)
   - Different workflows for different campaign types (product launch, event promotion, brand awareness)
   - Parallel creative exploration (generate multiple concepts simultaneously)
   - Custom approval chains (multi-level approval hierarchies)

5. **Collaborative Features**
   - Multi-user approval and voting
   - Comments and threaded discussions on assets
   - Role-based permissions (creator, reviewer, approver, publisher)
   - Real-time collaboration on campaign briefs
   - Activity feed and notifications

6. **Asset Management**
   - Save campaign templates for reuse
   - Campaign cloning with modifications
   - Version history with ability to revert
   - Asset library for organizing generated content
   - Export to various formats (PDF, ZIP, CSV)
   - Integration with digital asset management (DAM) systems

7. **Enhanced User Experience**
   - Guided feedback prompts with suggestions
   - Feedback templates for common improvement requests
   - Side-by-side comparison with previous versions
   - Preview mode with zoom and detail inspection
   - Mobile-optimized experience
   - Approval time limits with escalation
   - Session persistence and workflow recovery

8. **Intelligence and Learning**
   - Track successful campaign patterns and outcomes
   - Learn from user feedback and modifications
   - Improve generation quality based on approval rates
   - A/B testing recommendations
   - Performance analytics integration (engagement, conversions, ROI)
   - Predictive analytics for optimal posting times

9. **Expanded Asset Options**
   - Generate more than 3 assets per campaign
   - Individual asset regeneration (without replacing all assets)
   - More than 2 image variations (configurable count)
   - Multiple video duration options
   - Asset style variations (different visual styles from same content)
   - GIF and animation support

10. **Publishing Analytics**
    - Post performance tracking across platforms
    - Engagement metrics dashboard
    - ROI and conversion tracking
    - Automated reporting
    - Optimization recommendations based on performance

## Acceptance Criteria Summary

### Core Workflow ✅
- ✅ User can provide campaign brief through chat interface
- ✅ Campaign Planner generates strategic plan with smart defaults
- ✅ Planner requests clarification only if critical information is missing (HUMAN GATE 1—conditional)
- ✅ Creative Generator produces 3 assets (2 images + 1 video) with captions and hashtags
- ✅ Human approval required for creative assets (HUMAN GATE 2—required)
- ✅ Human can reject assets with feedback, triggering regeneration
- ✅ User selects target markets for localization or skips (HUMAN GATE 3—required)
- ✅ Localizer translates captions/hashtags for selected markets
- ✅ Schedule Creator generates two-week publishing schedule
- ✅ Human approval required for schedule (HUMAN GATE 4—required)
- ✅ Human can reject schedule with feedback, triggering regeneration
- ✅ Instagram Publisher posts first scheduled item automatically
- ✅ Complete campaign package delivered (plan, assets, localized content, schedule, post confirmation)
- ✅ Workflow pauses and waits indefinitely for human decisions at all gates

### Quality Assurance ✅
- ✅ Human feedback incorporated into regeneration at approval gates
- ✅ Human has final say on all creative and strategic decisions
- ✅ No automated quality loops—simpler, faster workflow
- ✅ Failed asset generation uses fallback and continues workflow
- ✅ User receives partial results if workflow cannot complete
- ✅ Quality depends on human judgment at four decision points

### User Experience ✅
- ✅ Clear visibility into workflow progress
- ✅ Agent outputs displayed when each phase completes
- ✅ Complete transparency of workflow status and agent actions
- ✅ Agent messages visually distinguished with distinct identities (UI-side implementation)
- ✅ Clear agent attribution (name/role) on every message
- ✅ Professional message formatting with structured data rendering
- ✅ Approval prompts clearly distinguished from informational messages
- ✅ Easy-to-use approval and feedback interfaces at each gate
### Success Metrics 📊
- Time to complete workflow (target: 10-20 minutes including human decisions)
- User approval rate at first attempt vs. regeneration rate
- Number of feedback-regeneration cycles per approval gate
- Asset generation success rate (images vs. video)
- Localization accuracy and naturalness
- Schedule acceptance rate
- Instagram publishing success rate
- User satisfaction with final campaign deliverables
- Workflow completion rate vs. abandonment rate
- Copy format and length specifications
- Handling of brand asset requirements (logos, colors, fonts) in image prompts
- Integration with external marketing platforms
- Image resolution and aspect ratio requirements per campaign type
- **Human approval UI/UX design details (button placement, feedback text area size, image comparison layout)**
- **Maximum allowed feedback text length from human users**
- **Notification strategy when workflow is awaiting human approval (email, push, in-app only)**
- **Timeout policy for human approvals (or explicit decision to wait indefinitely)**
- **Handling of concurrent workflows when user has multiple campaigns awaiting approval**
- **Image generation model configuration to request count=2 for variation generation**
- **Storage and display of rejected image variations (for user reference or rollback)**
- **Audit trail requirements for human approval decisions (compliance, tracking)**

### UI Design Specifications ✅
- **Message Timing**: Atomic messages displayed when executors complete (not real-time streaming)
- **Agent Color Palette**: Calm, distinguishable colors for Writer (blue), Image Prompter (green/teal), Designer (purple), Auditor (distinguished amber/yellow for emphasis), System (gray)
- **Color Implementation**: All agent color coding and visual distinction implemented on front-end/UI side
- **Visual Hierarchy**: Clear agent attribution with badges/labels on each message
- **Auditor Emphasis**: Auditor feedback displayed with particularly distinguished color/format to ensure review outcomes are highly visible
- **Accessibility**: Non-color-dependent identification through labels and structure
- **Content Parsing**: Automatic JSON detection, parsing, and formatted rendering in UI
- **Format Support**: JSON, Markdown, plain text with appropriate rendering for each
- **User-Friendly Display**: No raw technical artifacts (JSON dumps, escape sequences) shown to users