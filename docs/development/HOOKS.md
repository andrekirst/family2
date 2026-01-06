# Claude Code Hooks Guide

**Purpose:** Comprehensive guide to Claude Code hooks - deterministic shell commands that execute at specific points in the AI workflow.

**Audience:** Developers working with Claude Code on Family Hub

---

## What Are Hooks?

**Hooks** are deterministic shell commands that execute automatically at specific events in Claude Code's workflow. They are NOT LLM-generated suggestions - they're app-level code that runs every time, like Git hooks.

### Key Characteristics

- **Deterministic:** Always execute when triggered (no AI decision-making)
- **Shell-based:** Standard bash commands with stdin/stdout
- **Event-driven:** Triggered by specific Claude Code events
- **Configurable:** Project-wide or user-local configuration

---

## Hook Events

### PostToolUse

**Trigger:** Fires immediately after Claude uses `Edit` or `Write` tool

**Common uses:**

- Auto-format code (Prettier, ESLint, dotnet format)
- Run linters
- Update generated files
- Trigger builds
- Run tests

**Input format (stdin):**

```json
{
  "tool_name": "Write",
  "tool_input": {
    "file_path": "/absolute/path/to/file.ts"
  }
}
```

### Other Events (Future)

- **PreToolUse:** Before Claude uses a tool (not yet implemented)
- **PostSession:** After AI session completes (not yet implemented)
- **PreCommit:** Before git commit (not yet implemented)

---

## Configuration Files

### Project-Wide: `.claude/settings.json`

**Purpose:** Shared hooks committed to git for team consistency

**Location:** `/home/andrekirst/git/github/andrekirst/family2/.claude/settings.json`

**When to use:**

- Standard formatting/linting rules
- Required quality checks
- Team conventions

**Committed to git:** ✅ YES

### User-Local: `.claude/settings.local.json`

**Purpose:** Personal overrides not shared with team

**Location:** `/home/andrekirst/git/github/andrekirst/family2/.claude/settings.local.json`

**When to use:**

- Personal tool preferences
- Local development overrides
- Experimental hooks

**Committed to git:** ❌ NO (gitignored)

**Precedence:** Local settings override project settings

---

## Family Hub Hook Configuration

### Current Setup

**File:** `.claude/settings.json`

**What it does:**

1. After Claude edits a TypeScript/JavaScript file → Run Prettier + ESLint
2. After Claude edits a C# file → Run dotnet format
3. Other files → No action

**Hook command:**

```bash
file=$(jq -r '.tool_input.file_path')
if echo "$file" | grep -qE '\\.(ts|tsx|js|jsx)$'; then
  cd "$(dirname "$file")" && \
  npx prettier --write "$(basename "$file")" 2>/dev/null && \
  npx eslint --fix "$file" 2>/dev/null || true
elif echo "$file" | grep -q '\\.cs$'; then
  dotnet format "$file" 2>/dev/null || true
fi
```

**Breakdown:**

- `file=$(jq -r '.tool_input.file_path')` - Extract file path from JSON input
- `if echo "$file" | grep -qE '\\.(ts|tsx|js|jsx)$'` - Check if TypeScript/JavaScript
- `cd "$(dirname "$file")"` - Change to file's directory (for npx context)
- `npx prettier --write "$(basename "$file")"` - Format with Prettier
- `npx eslint --fix "$file"` - Fix ESLint violations
- `2>/dev/null` - Suppress error output
- `|| true` - Prevent hook failure from blocking Claude

---

### Phase 2: Documentation and Configuration Hooks

**Added:** January 2026

**Markdown Linting (markdownlint-cli2):**

- **Files:** 191 markdown files (docs/, .claude/, src/frontend/, root)
- **Rules:** `.markdownlint-cli2.jsonc` (balanced rules for documentation style)
- **Auto-fix:** Heading hierarchy, list formatting, code block languages, link formatting
- **Token savings:** 40% reduction for documentation-heavy sessions

**JSON/YAML Formatting (Prettier):**

- **Files:** appsettings.json, angular.json, tsconfig.json, docker-compose.yml, workflows
- **Exclusions:** package-lock.json, appsettings.Development.json (auto-generated/secrets)
- **Consistency:** Standardized indentation (2 spaces), key ordering, line breaks

**Performance:**

- Markdownlint: ~50ms per markdown file
- Prettier JSON/YAML: ~20ms per file
- Total hook chain: <200ms for multi-file edits (well within 30s timeout)

**Hook command breakdown:**

```bash
file=$(jq -r '.tool_input.file_path')

# TypeScript/JavaScript → Prettier + ESLint
if echo "$file" | grep -qE '\\.(ts|tsx|js|jsx)$'; then
  cd "$(dirname "$file")" && \
  npx prettier --write "$(basename "$file")" 2>/dev/null && \
  npx eslint --fix "$file" 2>/dev/null || true

# C# → dotnet format
elif echo "$file" | grep -q '\\.cs$'; then
  dotnet format "$file" 2>/dev/null || true

# Markdown → markdownlint-cli2 (NEW)
elif echo "$file" | grep -q '\\.md$'; then
  npx markdownlint-cli2 --fix "$file" 2>/dev/null || true

# JSON/YAML → Prettier (NEW, with exclusions)
elif echo "$file" | grep -qE '\\.(json|ya?ml)$' && \
     ! echo "$file" | grep -qE '(package-lock|Development|Local)\\.'; then
  npx prettier --write "$file" 2>/dev/null || true
fi
```

**Exclusion logic:** The `! echo "$file" | grep -qE '(package-lock|Development|Local)\\.'` ensures:

- package-lock.json NOT formatted (auto-generated, 570KB)
- appsettings.Development.json NOT formatted (contains secrets)
- appsettings.*.Local.json NOT formatted (local overrides)

---

## Hook Configuration Syntax

### Basic Structure

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "echo 'File edited: $(jq -r .tool_input.file_path)'",
            "timeout": 30000
          }
        ]
      }
    ]
  }
}
```

### Matcher Patterns

**Purpose:** Filter which tool invocations trigger hooks

**Examples:**

- `"Edit|Write"` - Trigger on file edits or new file writes
- `"Edit"` - Only file edits (not new files)
- `"Write"` - Only new file creation
- `"Read"` - After file reads (useful for validation)
- `"Bash"` - After shell commands (useful for build triggers)

### Multiple Hooks

**Parallel execution:**

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "npx prettier --write \"$(jq -r '.tool_input.file_path')\""
          },
          {
            "type": "command",
            "command": "npx eslint --fix \"$(jq -r '.tool_input.file_path')\""
          }
        ]
      }
    ]
  }
}
```

**Note:** Hooks run in parallel. Use `&&` within a single command for sequential execution.

---

## Common Hook Patterns

### 1. Format Specific File Types

```json
{
  "type": "command",
  "command": "file=$(jq -r '.tool_input.file_path'); if echo \"$file\" | grep -q '\\.py$'; then black \"$file\"; fi"
}
```

### 2. Run Tests After Code Changes

```json
{
  "type": "command",
  "command": "file=$(jq -r '.tool_input.file_path'); if echo \"$file\" | grep -q '\\.test\\.ts$'; then npm test -- \"$file\"; fi"
}
```

### 3. Update Generated Files

```json
{
  "type": "command",
  "command": "file=$(jq -r '.tool_input.file_path'); if echo \"$file\" | grep -q 'schema\\.graphql'; then npm run codegen; fi"
}
```

### 4. Custom Script Execution

```json
{
  "type": "command",
  "command": "\"$CLAUDE_PROJECT_DIR\"/.claude/hooks/custom-hook.sh"
}
```

**Note:** `$CLAUDE_PROJECT_DIR` variable provides project root path

---

## Troubleshooting

### Hook Not Running

**Check:**

1. Is hook enabled? Look for execution messages in Claude Code output
2. Does matcher pattern match? Check tool name (Edit vs Write)
3. Is command valid? Test manually: `echo '{"tool_input":{"file_path":"test.ts"}}' | jq -r '.tool_input.file_path'`
4. Is timeout sufficient? Default 60s, may need increase for slow tools

### Hook Failures

**Symptoms:** Claude reports hook execution failed

**Causes:**

- Tool not installed (npx prettier, dotnet format)
- Invalid file path
- Syntax error in command
- Timeout exceeded

**Solutions:**

- Add error suppression: `2>/dev/null || true`
- Increase timeout: `"timeout": 60000`
- Test command manually
- Check Claude Code output for errors

### Disabling Hooks Temporarily

**Method 1: Override in local settings**

Edit `.claude/settings.local.json`:

```json
{
  "hooks": {
    "PostToolUse": []
  }
}
```

**Method 2: Comment out in project settings**

Not recommended (changes affect team)

**Method 3: Skip via environment variable**

```bash
SKIP_HOOKS=1 claude-code
```

(Feature may not be available - check Claude Code docs)

---

## Best Practices

### 1. Keep Hooks Fast

**Target:** <100ms per hook

**Why:** Hooks run after every file edit - slow hooks frustrate users

**How:**

- Use targeted formatters (not full project scans)
- Suppress verbose output with `2>/dev/null`
- Set reasonable timeouts (30s default)

### 2. Handle Errors Gracefully

**Always use:** `|| true` to prevent hook failures from blocking Claude

**Example:**

```bash
npx prettier --write "$file" 2>/dev/null || true
```

**Why:** If formatter fails (tool not installed, syntax error), Claude should continue

### 3. Filter File Types

**Don't format everything:**

```bash
# BAD - runs on all files
npx prettier --write "$(jq -r '.tool_input.file_path')"

# GOOD - only runs on relevant files
file=$(jq -r '.tool_input.file_path')
if echo "$file" | grep -qE '\\.(ts|js|tsx|jsx)$'; then
  npx prettier --write "$file"
fi
```

### 4. Document Custom Hooks

If you add project-specific hooks, document them in this file with:

- Purpose
- When they run
- What they do
- How to disable (if needed)

### 5. Test Hooks Before Committing

**Steps:**

1. Add hook to `.claude/settings.local.json` first
2. Test with Claude Code edits
3. Verify behavior
4. Move to `.claude/settings.json` and commit

---

## Advanced Examples

### Run Tests Only for Test Files

```json
{
  "type": "command",
  "command": "file=$(jq -r '.tool_input.file_path'); if echo \"$file\" | grep -q '\\.spec\\.ts$'; then npm test -- --testPathPattern=\"$(basename \"$file\")\"; fi",
  "timeout": 120000
}
```

### Validate GraphQL Schema

```json
{
  "type": "command",
  "command": "file=$(jq -r '.tool_input.file_path'); if echo \"$file\" | grep -q 'schema\\.graphql'; then npm run graphql:validate || echo 'GraphQL validation failed'; fi"
}
```

### Update OpenAPI Spec

```json
{
  "type": "command",
  "command": "file=$(jq -r '.tool_input.file_path'); if echo \"$file\" | grep -qE 'Controllers/.*\\.cs$'; then dotnet tool run swagger tofile --output swagger.json; fi"
}
```

### Multi-Step Hook with Logging

```json
{
  "type": "command",
  "command": "file=$(jq -r '.tool_input.file_path'); echo \"[Hook] Processing: $file\" >> /tmp/claude-hooks.log; npx prettier --write \"$file\" 2>&1 | tee -a /tmp/claude-hooks.log || true"
}
```

---

## Environment Variables

### Available Variables

- `$CLAUDE_PROJECT_DIR` - Absolute path to project root
- `$HOME` - User home directory
- Standard shell variables (`$PATH`, `$USER`, etc.)

### Custom Variables

Define in `.claude/settings.json`:

```json
{
  "env": {
    "NODE_ENV": "development",
    "FORMATTER_CONFIG": "/path/to/config"
  },
  "hooks": { ... }
}
```

(Feature availability may vary - check Claude Code docs)

---

## Security Considerations

### Hook Execution Context

**Hooks run with your user permissions** - they can:

- Read/write any file you can access
- Execute any command you can run
- Access environment variables (including secrets)

### Best Practices

1. **Review hook commands** before adding to project settings
2. **Don't commit secrets** in hook commands
3. **Use gitignore** for sensitive local settings
4. **Sanitize file paths** to prevent injection attacks
5. **Limit hook permissions** if possible (use restricted tools)

### Example: Safe File Path Handling

```bash
# UNSAFE - vulnerable to injection
command="npx prettier --write $file"

# SAFE - quoted variable
command="npx prettier --write \"$file\""

# SAFER - validated file path
file=$(jq -r '.tool_input.file_path')
if [[ "$file" =~ ^/home/andrekirst/git/github/andrekirst/family2/ ]]; then
  npx prettier --write "$file"
fi
```

---

## FAQ

### Q: Do hooks consume Claude Code tokens?

**A:** No. Hooks are deterministic shell commands, not AI operations.

### Q: Can hooks modify files Claude just edited?

**A:** Yes. Formatters often do this (Prettier, ESLint --fix). Changes appear in next file read.

### Q: What happens if a hook times out?

**A:** Hook is killed, Claude continues. Check timeout setting (default 60s).

### Q: Can I chain multiple commands in one hook?

**A:** Yes. Use `&&` for sequential, `;` for independent:

```bash
"command": "cd $dir && npm install && npm test"
```

### Q: How do I debug hook failures?

**A:**

1. Check Claude Code output for error messages
2. Test command manually with sample JSON input
3. Add logging: `>> /tmp/claude-hooks.log`
4. Simplify command to isolate issue

### Q: Can hooks trigger other hooks?

**A:** No. Hooks don't re-trigger on their own file modifications (prevents infinite loops).

### Q: Are hooks required?

**A:** No. They're optional. Claude works fine without hooks - you just format manually.

---

**Last updated:** 2026-01-06
**Version:** 1.0.0
