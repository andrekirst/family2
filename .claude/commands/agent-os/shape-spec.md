# Shape Spec

Gather context and structure planning for significant work. **Run this command while in plan mode.**

## Important Guidelines

- **Always use AskUserQuestion tool** when asking the user anything
- **Offer suggestions** — Present options the user can confirm, adjust, or correct
- **Keep it lightweight** — This is shaping, not exhaustive documentation

## Prerequisites

This command **must be run in plan mode**.

**Before proceeding, check if you are currently in plan mode.**

If NOT in plan mode, **stop immediately** and tell the user:

```
Shape-spec must be run in plan mode. Please enter plan mode first, then run /shape-spec again.
```

Do not proceed with any steps below until confirmed to be in plan mode.

## Process

### Step 1: Clarify What We're Building

Use AskUserQuestion to understand the scope:

```
What are we building? Please describe the feature or change.

(Be as specific as you like — I'll ask follow-up questions if needed)
```

Based on their response, ask 1-2 clarifying questions if the scope is unclear. Examples:

- "Is this a new feature or a change to existing functionality?"
- "What's the expected outcome when this is done?"
- "Are there any constraints or requirements I should know about?"

### Step 2: Gather Visuals

Use AskUserQuestion:

```
Do you have any visuals to reference?

- Mockups or wireframes
- Screenshots of similar features
- Examples from other apps

(Paste images, share file paths, or say "none")
```

If visuals are provided, note them for inclusion in the spec folder.

### Step 3: Identify Reference Implementations

Use AskUserQuestion:

```
Is there similar code in this codebase I should reference?

Examples:
- "The comments feature is similar to what we're building"
- "Look at how src/features/notifications/ handles real-time updates"
- "No existing references"

(Point me to files, folders, or features to study)
```

If references are provided, read and analyze them to inform the plan.

### Step 4: Check Product Context

Check if `agent-os/product/` exists and contains files.

If it exists, read key files (like `mission.md`, `roadmap.md`, `tech-stack.md`) and use AskUserQuestion:

```
I found product context in agent-os/product/. Should this feature align with any specific product goals or constraints?

Key points from your product docs:
- [summarize relevant points]

(Confirm alignment or note any adjustments)
```

If no product folder exists, skip this step.

### Step 5: Surface Relevant Standards

Read `agent-os/standards/index.yml` to identify relevant standards based on the feature being built.

Use AskUserQuestion to confirm:

```
Based on what we're building, these standards may apply:

1. **api/response-format** — API response envelope structure
2. **api/error-handling** — Error codes and exception handling
3. **database/migrations** — Migration patterns

Should I include these in the spec? (yes / adjust: remove 3, add frontend/forms)
```

Read the confirmed standards files to include their content in the plan context.

### Step 6: Gather GitHub Issue Metadata

Present a pre-filled summary derived from Steps 1-5 and ask the user to confirm or adjust.

Use AskUserQuestion:

```
I'll create a GitHub issue to track this work. Here's what I've inferred:

- **Title**: [Feature] {derived from Step 1 description}
- **Phase**: {inferred from product context or "Phase 1" default}
- **Service**: {inferred from scope/references}
- **Domain**: {inferred from feature area}
- **Priority**: P2 (default)
- **Effort**: {estimated from plan complexity: S/M/L/XL}
- **User Story**:
  **As a** {persona}
  **I want** {capability}
  **So that** {benefit}

Should I create a GitHub issue with these details? (yes / adjust / skip)
```

If the user says **"skip"**, do not create a GitHub issue — proceed without it. Spec files will still be saved and committed, but commit messages and plan.md will not reference an issue number.

If the user says **"adjust"**, ask follow-up questions to correct the metadata.

**Label mapping tables** — use these exact label strings when creating issues:

| Phase | Label |
|-------|-------|
| Phase 0 - Foundation & Tooling | `phase-0` |
| Phase 1 - Core MVP | `phase-1` |
| Phase 2 - Health Integration & Event Chains | `phase-2` |
| Phase 3 - Meal Planning & Finance | `phase-3` |
| Phase 4 - Recurrence & Advanced Features | `phase-4` |
| Phase 5 - Microservices Extraction | `phase-5` |
| Phase 6 - Mobile App & Extended Features | `phase-6` |
| Future (Phase 7+) | `phase-7-future` |

| Service | Label |
|---------|-------|
| Auth Service | `service-auth` |
| Calendar Service | `service-calendar` |
| Task Service | `service-task` |
| Shopping Service | `service-shopping` |
| Health Service | `service-health` |
| Meal Planning Service | `service-meal-planning` |
| Finance Service | `service-finance` |
| Communication Service | `service-communication` |
| Frontend (Angular) | `service-frontend` |
| Infrastructure/DevOps | `service-infrastructure` |

| Domain | Label |
|--------|-------|
| Event Chain Automation | `domain-event-chain` |
| Security | `domain-security` |
| Performance | `domain-performance` |
| Accessibility | `domain-accessibility` |

| Priority | Label |
|----------|-------|
| P0 - Critical | `priority-p0` |
| P1 - High | `priority-p1` |
| P2 - Medium | `priority-p2` |
| P3 - Low | `priority-p3` |

| Effort | Label |
|--------|-------|
| Small (< 4 hours) | `effort-s` |
| Medium (4-16 hours) | `effort-m` |
| Large (2-5 days) | `effort-l` |
| Extra Large (1+ weeks) | `effort-xl` |

### Step 7: Generate Spec Folder Name

Create a folder name using this format:

```
YYYY-MM-DD-{feature-slug}/
```

Where:

- Date is today's date
- Feature slug is derived from the feature description (lowercase, hyphens, max 40 chars)

Example: `2026-01-15-user-comment-system/`

**Note:** If `agent-os/specs/` doesn't exist, create it when saving the spec folder.

### Step 8: Structure the Plan

Now build the plan with **Task 1 always being "Save spec, commit, and create GitHub issue"** (or "Save spec and commit" if issue creation was skipped).

Present this structure to the user:

```
Here's the plan structure. Task 1 saves all our shaping work, creates the GitHub issue, and commits before implementation begins.

---

## Task 1: Save Spec, Commit, and Create GitHub Issue

1. Write spec files to `agent-os/specs/{folder-name}/`:
   - **plan.md** — This full plan
   - **shape.md** — Shaping notes (scope, decisions, context from our conversation)
   - **standards.md** — Relevant standards that apply to this work
   - **references.md** — Pointers to reference implementations studied
   - **visuals/** — Any mockups or screenshots provided

2. Create GitHub issue via `gh issue create` → capture issue number
   {or "Skipped — no issue creation" if user chose skip}

3. Update plan.md and shape.md headers with `**GitHub Issue**: #{number}`

4. Git commit all spec files:
   `docs(spec): add {slug} spec (#{number})`

## Task 2: [First implementation task]

[Description based on the feature]

## Task 3: [Next task]

...

---

Does this plan structure look right? I'll fill in the implementation tasks next.
```

### Step 9: Complete the Plan

After Task 1 is confirmed, continue building out the remaining implementation tasks based on:

- The feature scope from Step 1
- Patterns from reference implementations (Step 3)
- Constraints from standards (Step 5)

Each task should be specific and actionable.

### Step 10: Ready for Execution

When the full plan is ready:

```
Plan complete. When you approve, Task 1 will:

1. Save spec documentation to agent-os/specs/{folder-name}/
2. Create GitHub issue with labels: {labels summary}
3. Cross-link issue number in plan.md and shape.md
4. Commit spec files to git

Then implementation tasks proceed.

Ready to start? (approve / adjust)
```

If issue creation was skipped in Step 6, adjust the message:

```
Plan complete. When you approve, Task 1 will:

1. Save spec documentation to agent-os/specs/{folder-name}/
2. Commit spec files to git

Then implementation tasks proceed.

Ready to start? (approve / adjust)
```

## Task 1 Execution Instructions

When the user approves and Task 1 runs, follow these steps in order:

### Step A: Write Spec Files

Write all spec files to `agent-os/specs/{folder-name}/` (plan.md, shape.md, standards.md, references.md, and optionally visuals/).

Use the plan.md and shape.md content templates defined below. **Do not include the `**GitHub Issue**` field yet** — it will be added after issue creation.

### Step B: Create GitHub Issue (unless skipped)

Run `gh issue create` with this format:

```bash
gh issue create \
  --title "[Feature] {feature title}" \
  --label "type-feature" \
  --label "status-planning" \
  --label "{phase-label}" \
  --label "{service-label}" \
  --label "{priority-label}" \
  --label "{effort-label}" \
  --body "$(cat <<'ISSUE_EOF'
## Summary

{2-3 sentence description from Step 1}

## User Story

**As a** {persona}
**I want** {capability}
**So that** {benefit}

## Acceptance Criteria

- [ ] {derived from plan tasks and scope}

## Technical Notes

{Standards applied, key decisions from shape.md}

## Spec

[`agent-os/specs/{folder}/`](agent-os/specs/{folder}/)
- [plan.md](agent-os/specs/{folder}/plan.md)
- [shape.md](agent-os/specs/{folder}/shape.md)
ISSUE_EOF
)"
```

Add `--label "{domain-label}"` only if a domain label was identified in Step 6.

Capture the issue number from the command output.

### Step C: Update Spec Files with Issue Number

After capturing the issue number, update:

- **plan.md**: Add `**GitHub Issue**: #{number}` to the metadata header
- **shape.md**: Add `**GitHub Issue**: #{number}` to the metadata header

### Step D: Git Commit

Stage and commit all spec files:

```bash
git add agent-os/specs/{folder-name}/
git commit -m "$(cat <<'EOF'
docs(spec): add {slug} spec (#{number})

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
EOF
)"
```

If issue creation was skipped, omit the issue number from the commit message:

```bash
git commit -m "$(cat <<'EOF'
docs(spec): add {slug} spec

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
EOF
)"
```

## Error Handling

### `gh` not authenticated

If `gh auth status` fails or `gh issue create` returns an authentication error:

1. Skip issue creation gracefully
2. Inform the user: "GitHub CLI is not authenticated. Run `gh auth login` to enable automatic issue creation. Spec files have been saved and committed without an issue link."
3. Continue with commit (without issue number)

### `gh issue create` fails (other errors)

1. Show the formatted issue body so the user can create the issue manually
2. Continue with commit (without issue number)
3. Tell the user: "Issue creation failed. Here's the issue body — you can create it manually at {repo URL}/issues/new"

### Label doesn't exist

If `gh issue create` fails because a label doesn't exist:

1. Remove the offending label from the command
2. Retry `gh issue create` without that label
3. Inform the user which label was skipped

### Git commit fails

1. Report the error to the user
2. Continue — spec files are still written to disk
3. The user can commit manually

## Output Structure

The spec folder will contain:

```
agent-os/specs/{YYYY-MM-DD-feature-slug}/
├── plan.md           # The full plan (with GitHub Issue link)
├── shape.md          # Shaping decisions and context (with GitHub Issue link)
├── standards.md      # Which standards apply and key points
├── references.md     # Pointers to similar code
└── visuals/          # Mockups, screenshots (if any)
```

## plan.md Content

The plan.md file should use this header format:

```markdown
# {Feature Name}

**Created**: {YYYY-MM-DD}
**GitHub Issue**: #{number}
**Spec**: `agent-os/specs/{folder}/`

## Context

[Feature context and description]

## Files to Modify

[Implementation details...]

## Implementation Tasks

### Task 1: Save Spec, Commit, and Create GitHub Issue
...

### Task 2: [First implementation task]
...
```

If issue creation was skipped, omit the `**GitHub Issue**` line.

## shape.md Content

The shape.md file should capture:

```markdown
# {Feature Name} — Shaping Notes

**Feature**: {brief description}
**Created**: {YYYY-MM-DD}
**GitHub Issue**: #{number}

---

## Scope

[What we're building, from Step 1]

## Decisions

- [Key decisions made during shaping]
- [Constraints or requirements noted]

## Context

- **Visuals:** [List of visuals provided, or "None"]
- **References:** [Code references studied]
- **Product alignment:** [Notes from product context, or "N/A"]

## Standards Applied

- api/response-format — [why it applies]
- api/error-handling — [why it applies]
```

If issue creation was skipped, omit the `**GitHub Issue**` line.

## standards.md Content

Include the full content of each relevant standard:

```markdown
# Standards for {Feature Name}

The following standards apply to this work.

---

## api/response-format

[Full content of the standard file]

---

## api/error-handling

[Full content of the standard file]
```

## references.md Content

```markdown
# References for {Feature Name}

## Similar Implementations

### {Reference 1 name}

- **Location:** `src/features/comments/`
- **Relevance:** [Why this is relevant]
- **Key patterns:** [What to borrow from this]

### {Reference 2 name}

...
```

## Tips

- **Keep shaping fast** — Don't over-document. Capture enough to start, refine as you build.
- **Visuals are optional** — Not every feature needs mockups.
- **Standards guide, not dictate** — They inform the plan but aren't always mandatory.
- **Specs are discoverable** — Months later, someone can find this spec and understand what was built and why.
- **Issue creation is opt-out** — The default is to create an issue. Users can skip if they prefer.
