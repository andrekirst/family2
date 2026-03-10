# Agent Resource Management Playbook

**Purpose:** Prevent OOM and resource contention when multiple Claude Code agents work in parallel on the Family Hub codebase.

---

## 1. The Problem

Each `dotnet test` invocation spawns a build process plus one or more test host processes, each consuming several hundred MB of RAM. When 6+ parallel agents independently run full-suite tests against `FamilyHub.slnx` (14 test projects, 736+ tests), combined with Keycloak Docker's baseline ~1GB RAM usage, total memory consumption exceeds 32GB and triggers OOM kills. The root cause is the absence of coordination rules — agents each run full-suite tests unaware of each other.

---

## 2. Golden Rules

1. **NEVER** run `dotnet test FamilyHub.slnx` — always target a specific test project
2. **NEVER** run `tests/FamilyHub.IntegrationTests/` — Testcontainers-based, CI-only
3. **NEVER** run `tests/FamilyHub.Architecture.Tests/` — orchestrator runs this at the end
4. **ALWAYS** use convention-based test targeting — changed `Foo.cs` → run `FooTests.cs`
5. **ALWAYS** use `--no-build` when the agent just ran `dotnet build`
6. **ALWAYS** check lock files before testing — wait 120s, then report via `SendMessage`

---

## 3. Convention-Based Test Targeting

### Decision Tree

1. **Changed `Foo.cs`?** → Find `FooTests.cs` → run with `--filter`
2. **No matching test file?** → Run the parent feature's test project
3. **Multiple modules changed?** → Run each module's test project separately (never the solution)

### Correct Examples

```bash
# Changed Features/Family/Commands/CreateFamily/Handler.cs
dotnet test tests/FamilyHub.Family.Tests/ \
  --filter "FullyQualifiedName~CreateFamilyCommandHandlerTests" \
  --no-build

# Changed Features/Auth/Domain/UserAggregate.cs
dotnet test tests/FamilyHub.Auth.Tests/ \
  --filter "FullyQualifiedName~UserAggregateTests" \
  --no-build

# Changed multiple files in Family module, no specific test known
dotnet test tests/FamilyHub.Family.Tests/ --no-build
```

### Incorrect Examples

```bash
# WRONG: Full solution test
dotnet test FamilyHub.slnx

# WRONG: Running integration tests
dotnet test tests/FamilyHub.IntegrationTests/

# WRONG: Building when you already built
dotnet test tests/FamilyHub.Family.Tests/  # without --no-build after a build
```

---

## 4. Source to Test Project Mapping

| Source Path | Test Project |
|---|---|
| `Features/Auth/` | `tests/FamilyHub.Auth.Tests/` |
| `Features/Family/` | `tests/FamilyHub.Family.Tests/` |
| `Features/Calendar/` | `tests/FamilyHub.Calendar.Tests/` |
| `Features/Dashboard/` | `tests/FamilyHub.Dashboard.Tests/` |
| `Features/EventChain/` | `tests/FamilyHub.EventChain.Tests/` |
| `Features/FileManagement/` | `tests/FamilyHub.FileManagement.Tests/` |
| `Features/GoogleIntegration/` | `tests/FamilyHub.GoogleIntegration.Tests/` |
| `Features/Messaging/` | `tests/FamilyHub.Messaging.Tests/` |
| `Features/Photos/` | `tests/FamilyHub.Photos.Tests/` |
| `Features/School/` | `tests/FamilyHub.School.Tests/` |
| `Features/Search/` | `tests/FamilyHub.Search.Tests/` |
| `Common/` | `tests/FamilyHub.Auth.Tests/Common/` |

---

## 5. Per-Project Locking Protocol

Lock files prevent two agents from running the same test project simultaneously. Different test projects CAN run in parallel — locking is per-project.

### Lock File Location

```
/tmp/familyhub-test-lock-{ProjectName}
```

Example: `/tmp/familyhub-test-lock-FamilyHub.Family.Tests`

### Protocol

```bash
PROJECT="FamilyHub.Family.Tests"
LOCK="/tmp/familyhub-test-lock-${PROJECT}"

# 1. Check if locked
if [ -f "$LOCK" ]; then
  echo "Test project $PROJECT is locked. Waiting..."
  WAITED=0
  while [ -f "$LOCK" ] && [ $WAITED -lt 120 ]; do
    sleep 5
    WAITED=$((WAITED + 5))
  done
  if [ -f "$LOCK" ]; then
    echo "Lock still held after 120s. Report to orchestrator via SendMessage."
    exit 1
  fi
fi

# 2. Acquire lock
echo "$$" > "$LOCK"

# 3. Run tests
dotnet test "tests/${PROJECT}/" --no-build

# 4. Release lock
rm -f "$LOCK"
```

### Rules

- Create the lock file **before** running `dotnet test`
- Remove the lock file **after** the test completes (success or failure)
- If a lock is held: poll every 5s for up to 120s
- After 120s: stop waiting, report to the orchestrator via `SendMessage`
- Different projects can run in parallel — only the same project is serialized

---

## 6. Build Coordination

- **Prefer `--no-build`** when testing immediately after a build
- **Build specific test projects**, not the full solution: `dotnet build tests/FamilyHub.Family.Tests/`
- **Only the orchestrator** runs `dotnet build FamilyHub.slnx` for full solution builds
- Sub-agents should build only the test project they need

---

## 7. Excluded Test Projects

| Project | Excluded From | Reason |
|---|---|---|
| `FamilyHub.IntegrationTests` | ALL agents | Testcontainers (Keycloak + PostgreSQL Docker), high resource usage |
| `FamilyHub.Architecture.Tests` | Sub-agents | Solution-wide checks; orchestrator runs once at the end |
| `FamilyHub.Dev.Tests` | ALL agents | Developer utility tests, not part of CI validation |

---

## 8. Orchestrator Responsibilities

The orchestrator (main Claude Code session coordinating sub-agents) is responsible for:

1. **Run `Architecture.Tests`** after all sub-agents complete their work
2. **Run `IntegrationTests`** if integration-level changes were made
3. **Full solution build** (`dotnet build FamilyHub.slnx`) for final verification
4. **Handle `SendMessage` reports** from agents blocked by stale locks
5. **Clean up stale lock files** at the start and end of orchestration:

   ```bash
   rm -f /tmp/familyhub-test-lock-*
   ```

---

## 9. Complete Decision Tree

```
Changed a source file?
│
├─ Which module?
│  └─ Look up test project in Section 4 mapping table
│
├─ Is there a matching *Tests.cs file?
│  ├─ YES → dotnet test tests/{Module}.Tests/ --filter "FullyQualifiedName~{TestClass}" --no-build
│  └─ NO  → dotnet test tests/{Module}.Tests/ --no-build
│
├─ Multiple modules changed?
│  └─ Run each module's test project separately (never dotnet test FamilyHub.slnx)
│
├─ Changed Common/ infrastructure?
│  └─ Run tests/FamilyHub.Auth.Tests/ (Common tests live here)
│
├─ Is the test project locked?
│  ├─ YES → Wait up to 120s (poll every 5s)
│  │        └─ Still locked? → SendMessage to orchestrator
│  └─ NO  → Acquire lock → Run tests → Release lock
│
└─ Is this IntegrationTests or Architecture.Tests?
   └─ Do NOT run. Orchestrator handles these.
```

---

## 10. Troubleshooting

### Stale Lock Files

If an agent crashes or is killed, its lock file persists. Symptoms: agents permanently blocked.

**Fix:** Remove stale locks manually:

```bash
rm -f /tmp/familyhub-test-lock-*
```

The orchestrator should clean up locks at the start of each session.

### Build Errors After Another Agent's Changes

If `--no-build` fails because another agent modified shared code:

1. Build just your test project: `dotnet build tests/FamilyHub.Family.Tests/`
2. Then test with `--no-build`
3. Do NOT rebuild the full solution — that's the orchestrator's job

### OOM Despite Following Rules

If OOM still occurs with per-project testing:

1. Check if Docker containers are consuming excessive memory: `docker stats`
2. Check for runaway processes: `ps aux --sort=-%mem | head -20`
3. Reduce parallel agent count (4 agents safer than 6+ on 32GB)
4. Report to orchestrator — the team size may need adjustment

### Test Filter Not Matching

If `--filter "FullyQualifiedName~FooTests"` runs zero tests:

1. Verify the test class name: `grep -r "class.*FooTests" tests/`
2. Try a broader filter: `--filter "FullyQualifiedName~Foo"`
3. Fall back to running the entire test project without `--filter`

---

**Last Updated:** 2026-03-08
