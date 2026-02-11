# Pre-Merge Rebase Protocol for EF Core Migrations

## Problem

When multiple agents develop features in parallel (each in a git worktree), EF Core migrations create snapshot conflicts. The `AppDbContextModelSnapshot.cs` file is auto-generated and cannot be merged textually.

## Protocol

Before creating a PR, every agent that adds or modifies EF Core migrations MUST follow this sequence:

### 1. Rebase onto main

```bash
git fetch origin
git rebase origin/main
```

### 2. Remove your migration snapshot

If there are conflicts in `AppDbContextModelSnapshot.cs`, accept the incoming (main) version:

```bash
git checkout --theirs src/FamilyHub.Api/Migrations/AppDbContextModelSnapshot.cs
git add src/FamilyHub.Api/Migrations/AppDbContextModelSnapshot.cs
git rebase --continue
```

### 3. Re-scaffold the migration

Remove your migration files and recreate them on top of the current snapshot:

```bash
# Remove your migration (keep the snapshot from main)
rm src/FamilyHub.Api/Migrations/<YourMigrationTimestamp>_<YourMigrationName>.cs
rm src/FamilyHub.Api/Migrations/<YourMigrationTimestamp>_<YourMigrationName>.Designer.cs

# Re-create the migration
dotnet ef migrations add <YourMigrationName> --project src/FamilyHub.Api
```

### 4. Verify

```bash
dotnet build src/FamilyHub.Api/FamilyHub.slnx
dotnet test src/FamilyHub.Api/FamilyHub.slnx
```

### 5. Push and create PR

```bash
git push --force-with-lease
```

## Key Rules

- **Never merge migration snapshots** — always rebase and re-scaffold
- **One migration per feature branch** — makes re-scaffolding simple
- **Name migrations descriptively** — e.g., `AddMealsModule`, not `Migration_20260210`
- **Re-scaffold is safe** — EF Core generates the correct diff from the current snapshot state
