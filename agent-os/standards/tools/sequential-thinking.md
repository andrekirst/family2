# Sequential Thinking

Pattern for structured reasoning through complex problems.

## Why

Sequential Thinking provides explicit step-by-step reasoning with the ability
to revise, branch, and extend thinking. This is critical for complex decisions
where the full scope isn't immediately clear.

## When to Use (Triggers)

| Trigger | Example |
|---------|---------|
| Multi-file changes | Task affects 3+ files or multiple modules |
| Uncertainty | Implementation approach isn't immediately clear |
| Architecture impact | Changes affect domain model or module boundaries |
| Complex debugging | Root cause spans multiple components |
| Design decisions | Multiple valid approaches exist |

## When NOT to Use

- Simple, obvious tasks (typo fixes, single-line changes)
- Tasks with clear, singular solution
- Pure information retrieval
- Already well-defined implementation steps

## Pattern: Planning Sequence

```
Thought 1: Understand the request and identify scope
Thought 2: List affected components/files
Thought 3: Identify dependencies and constraints
Thought 4: Generate solution hypothesis
Thought 5: Verify hypothesis against constraints
Thought 6: Refine or branch if issues found
→ Continue until satisfied
```

## Pattern: Debugging Sequence

```
Thought 1: State the observed behavior vs expected
Thought 2: List potential causes (prioritized)
Thought 3: Identify verification steps for top cause
Thought 4: Execute verification (may require code exploration)
Thought 5: Confirm or eliminate cause, revise if needed
→ Iterate until root cause found
```

## Pattern: Architecture Decision Sequence

```
Thought 1: Define the decision to be made
Thought 2: List constraints (ADRs, existing patterns, tech stack)
Thought 3: Generate 2-3 viable options
Thought 4: Evaluate each option against constraints
Thought 5: Select and justify the recommended option
Thought 6: Document tradeoffs and risks
```

## Key Parameters

| Parameter | Purpose |
|-----------|---------|
| `totalThoughts` | Initial estimate (adjust as needed) |
| `needsMoreThoughts` | Set true to extend beyond estimate |
| `isRevision` | Mark thoughts that reconsider previous thinking |
| `revisesThought` | Reference which thought is being reconsidered |
| `branchFromThought` | Create alternative exploration paths |
| `branchId` | Identifier for the current branch |

## Pattern: Revision and Branching

When discovering an issue with previous reasoning:

```
sequentialthinking(
    thought: "Reconsidering thought 3 - the constraint X was missed...",
    isRevision: true,
    revisesThought: 3,
    ...
)
```

When exploring alternative approaches:

```
sequentialthinking(
    thought: "Branching to explore alternative: using approach Y...",
    branchFromThought: 4,
    branchId: "approach-y",
    ...
)
```

## Rules

- Start with realistic thought estimate, adjust as needed
- Mark revisions explicitly with `isRevision: true`
- Don't hesitate to extend with `needsMoreThoughts`
- Use branching for exploring alternatives
- Always verify hypothesis before concluding
- Final thought should summarize decision and rationale
