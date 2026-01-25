---
name: memory-context-load
description: Load session context from Serena memories
category: tools
module-aware: false
inputs:
  - task_type: Type of task (feature, debugging, refactoring)
  - module: Target module if applicable (optional)
---

# Memory Context Loading

Workflow for loading relevant Serena memories at session start.

## When to Invoke

- Starting a new session
- Switching to a different module or task type
- Before complex multi-session work
- When context from previous sessions is needed

## Step 1: List Available Memories

```
list_memories()
```

Review the memory names for relevance to current task.

## Step 2: Identify Relevant Memories

**By task type:**

| Task Type | Look For |
|-----------|----------|
| Feature implementation | `*-implementation-plan.md`, `*-patterns.md` |
| Debugging | `*-debugging-insights.md`, `*-gotchas.md` |
| Refactoring | `*-architecture.md`, `*-migration.md` |
| Module work | `{module}-*.md` |

**By module:**

```
# Look for memories containing module name
# e.g., for auth module: auth-*, *-auth-*
```

## Step 3: Load Relevant Memories

```
read_memory(memory_file_name="relevant-memory-name.md")
```

**Priority order:**

1. Task-specific memories (e.g., current feature plan)
2. Module-specific memories (e.g., module gotchas)
3. Cross-cutting memories (e.g., architecture decisions)

## Step 4: Synthesize Context

After loading memories, synthesize:

- **What was decided**: Key decisions from previous sessions
- **What to remember**: Gotchas, patterns, constraints
- **What's pending**: Incomplete work, open questions

## Memory Management During Session

### Creating New Memories

When discovering reusable context:

```
write_memory(
    memory_file_name="{topic}-{context}.md",
    content="# Title\n\n## Context\n...\n## Key Points\n...\n## When to Apply\n..."
)
```

**Good memory candidates:**

- Implementation decisions not in ADRs
- Module-specific quirks discovered
- Debugging insights for complex issues
- Cross-session task plans

### Updating Existing Memories

When context evolves:

```
edit_memory(
    memory_file_name="existing-memory.md",
    needle="outdated content",
    repl="updated content",
    mode="literal"
)
```

### Memory Naming Convention

Format: `{topic}-{context}.md`

Examples:

- `phase-0-implementation-plan.md`
- `auth-module-oauth-gotchas.md`
- `event-chain-debugging-insights.md`
- `family-creation-ui-implementation-plan.md`

## Integration with Agent OS Profiles

**Memories complement profiles:**

| Profiles Provide | Memories Provide |
|------------------|------------------|
| Static structure | Dynamic status |
| Entity definitions | Implementation progress |
| File conventions | Session context |
| Pattern references | Discovered gotchas |

**Load sequence:**

1. Load memories for dynamic context
2. Load profile for static structure
3. Combine for full understanding

## Verification

- [ ] Listed all available memories
- [ ] Identified memories relevant to current task
- [ ] Loaded and reviewed relevant memories
- [ ] Synthesized context for current session
- [ ] Noted any memories that need updating
