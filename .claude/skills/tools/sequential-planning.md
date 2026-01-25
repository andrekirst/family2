---
name: sequential-planning
description: Plan complex tasks using Sequential Thinking
category: tools
module-aware: false
inputs:
  - task: Description of the complex task
  - scope: Estimated scope (files, modules affected)
---

# Sequential Thinking Planning

Workflow for planning complex implementations using structured reasoning.

## When to Invoke

- Task affects 3+ files
- Multiple modules involved
- Architectural decisions needed
- Implementation approach unclear
- Root cause analysis required

## Step 1: Initialize Thinking

```
sequentialthinking(
    thought: "Understanding the task: {task}. Initial scope assessment: {scope}. Let me identify what I need to understand.",
    thoughtNumber: 1,
    totalThoughts: 5,
    nextThoughtNeeded: true
)
```

## Step 2: Scope Analysis

Identify in subsequent thoughts:

- **Files to modify**: List specific file paths
- **Modules affected**: Which DDD modules are involved
- **Dependencies**: What depends on what we're changing
- **Constraints**: ADRs, existing patterns, tech stack limits

```
sequentialthinking(
    thought: "Analyzing scope: Files affected are [...]. Modules involved: [...]. Key dependencies: [...]",
    thoughtNumber: 2,
    totalThoughts: 5,
    nextThoughtNeeded: true
)
```

## Step 3: Solution Design

Generate hypothesis:

- **Proposed approach**: High-level strategy
- **Key implementation steps**: Ordered list
- **Risk areas**: What could go wrong

```
sequentialthinking(
    thought: "Proposed approach: [...]. Implementation steps: 1) [...] 2) [...]. Risk areas: [...]",
    thoughtNumber: 3,
    totalThoughts: 5,
    nextThoughtNeeded: true
)
```

## Step 4: Verification

Check against:

- Existing patterns (load from `agent-os/standards/`)
- Module boundaries (load from `agent-os/profiles/modules/`)
- ADR constraints (check `docs/architecture/ADR-*.md`)

```
sequentialthinking(
    thought: "Verifying against constraints: ADR-003 requires [...]. Existing pattern in [module] uses [...]. My approach aligns/conflicts because [...]",
    thoughtNumber: 4,
    totalThoughts: 5,
    nextThoughtNeeded: true
)
```

## Step 5: Finalize or Branch

**If issues found:**

```
sequentialthinking(
    thought: "Issue identified: [...]. Reconsidering approach...",
    isRevision: true,
    revisesThought: 3,
    thoughtNumber: 5,
    totalThoughts: 6,
    needsMoreThoughts: true,
    nextThoughtNeeded: true
)
```

**If exploring alternatives:**

```
sequentialthinking(
    thought: "Exploring alternative approach: [...]. This differs from main approach in [...]",
    branchFromThought: 3,
    branchId: "alternative-approach",
    thoughtNumber: 5,
    totalThoughts: 6,
    nextThoughtNeeded: true
)
```

**If satisfied:**

```
sequentialthinking(
    thought: "Final decision: [...]. Implementation plan: 1) [...] 2) [...]. Key considerations: [...]",
    thoughtNumber: 5,
    totalThoughts: 5,
    nextThoughtNeeded: false
)
```

## Common Planning Scenarios

### New Feature Planning

1. Understand requirements
2. Map to DDD layers (Domain → Application → Persistence → Presentation)
3. Identify existing patterns to follow
4. Plan file creation order
5. Verify against ADRs

### Debugging Complex Issue

1. State observed vs expected behavior
2. List potential causes (prioritized by likelihood)
3. Design verification steps for each cause
4. Trace through code to confirm/eliminate
5. Identify fix approach

### Refactoring Decision

1. Define what's being refactored and why
2. List affected components
3. Design incremental migration strategy
4. Identify backward compatibility needs
5. Plan verification/testing approach

## Verification

- [ ] Started with scope understanding
- [ ] Identified all affected components
- [ ] Generated verifiable hypothesis
- [ ] Checked against existing patterns (ADRs, standards)
- [ ] Revised thinking when issues found
- [ ] Final thought summarizes decision clearly
