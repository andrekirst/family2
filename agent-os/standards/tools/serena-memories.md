# Serena Memories

Pattern for using Serena memories to complement Agent OS profiles.

## Why

Memories persist information across sessions. They should complement (not
duplicate) Agent OS profiles, storing dynamic/contextual information that
profiles don't capture.

## Memory-Profile Relationship

| Profiles Store | Memories Store |
|----------------|----------------|
| Static module structure | Current implementation status |
| Entity/event definitions | Discovered patterns/gotchas |
| Tech stack constants | Session-specific context |
| File path conventions | Navigation shortcuts |
| ADR references | Implementation decisions not in ADRs |

## Appropriate Memory Content

### Good Memory Topics

- Implementation decisions not captured in ADRs
- Discovered code patterns specific to this codebase
- Cross-session task context (e.g., "phase-0-implementation-plan")
- Module-specific gotchas or quirks
- Debugging insights for complex issues
- User preferences discovered during sessions

### Do Not Memory

- Tech stack (already in base.yaml)
- File paths (already in profiles)
- Standard patterns (already in standards/)
- Entity lists (already in module profiles)
- Temporary debugging notes

## Pattern: Session Start

Before starting work on a module:

```
list_memories()
→ Check for relevant memories by name
→ read_memory(memory_file_name="relevant-memory.md")
```

Memory naming convention: `{topic}-{context}.md` (kebab-case)

Examples:

- `phase-0-implementation-plan.md`
- `auth-module-gotchas.md`
- `event-chain-debugging-insights.md`

## Pattern: Write Learnings

After discovering important context:

```
write_memory(
    memory_file_name="module-specific-insight.md",
    content="# Insight Title\n\n## Context\n...\n## Pattern\n...\n## When to Apply\n..."
)
```

## Pattern: Update Memories

When information becomes stale:

```
edit_memory(
    memory_file_name="existing-memory.md",
    needle="old content",
    repl="new content",
    mode="literal"  # or "regex"
)
```

## Pattern: Memory Lifecycle

1. **Create**: When discovering reusable cross-session context
2. **Read**: At session start for relevant modules
3. **Update**: When context evolves
4. **Delete**: When explicitly requested by user or clearly obsolete

## Rules

- Check `list_memories()` at session start for relevant context
- Memory names should be descriptive (kebab-case)
- Don't duplicate profile content
- Update memories when information becomes stale
- Delete memories when no longer relevant (user request only)
- Keep memories focused - one topic per memory file
