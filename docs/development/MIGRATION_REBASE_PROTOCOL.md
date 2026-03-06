# DbUp Migration Protocol

## Overview

Family Hub uses **DbUp** (dbup-postgresql) for database migrations instead of EF Core migrations. SQL scripts are embedded as resources in the API assembly and executed in order at application startup via `DatabaseMigrationRunner.Migrate()`.

This eliminates the EF Core `AppDbContextModelSnapshot.cs` merge conflicts that occurred during parallel agent development.

## Script Organization

```
src/FamilyHub.Api/Database/Migrations/
  _bridge/          - Bridge script (EF Core → DbUp transition)
  auth/             - Auth module schemas
  family/           - Family module schemas
  calendar/         - Calendar module schemas
  event_chain/      - Event chain module schemas
  avatar/           - Avatar module schemas
  storage/          - Storage module schemas
  dashboard/        - Dashboard module schemas
  google_integration/ - Google integration schemas
  file_management/  - File management schemas
  photos/           - Photos module schemas
  messaging/        - Messaging module schemas
  x_cross_schema/     - Cross-schema foreign keys
  rls/              - Row-Level Security policies
```

## Naming Convention

```
{YYYYMMDDHHMMSS}_{kebab-case-description}.sql
```

Example: `20260211103143_create-auth-schema.sql`

## Adding a New Migration

1. Create a `.sql` file in the appropriate module folder under `Database/Migrations/`
2. Use the naming convention above (timestamp ensures ordering)
3. Use `IF NOT EXISTS` for all `CREATE TABLE`, `CREATE INDEX`, etc. statements
4. The script is automatically embedded via the `<EmbeddedResource>` glob in `FamilyHub.Api.csproj`

```bash
# Example: Add a new migration for the auth module
touch src/FamilyHub.Api/Database/Migrations/auth/20260305120000_add-user-preferences.sql
```

## Idempotency Rules

- All `CREATE TABLE` → use `IF NOT EXISTS`
- All `CREATE INDEX` → use `IF NOT EXISTS`
- All `ALTER TABLE ADD COLUMN` → wrap in `DO $$ ... IF NOT EXISTS ... $$`
- All `CREATE POLICY` → drop first with `IF EXISTS`, then create
- Cross-schema FKs → wrap in `DO $$ ... IF NOT EXISTS ... $$`

## Parallel Development

DbUp eliminates the EF Core snapshot merge conflict problem:

- **No snapshot file** — Each migration is a standalone SQL script
- **No re-scaffolding** — Scripts are additive and idempotent
- **Simple merging** — New scripts in different folders don't conflict
- **Deterministic ordering** — Timestamp prefixes ensure consistent execution order

## Execution

DbUp runs at application startup in `Program.cs`:

```csharp
var migrationResult = DatabaseMigrationRunner.Migrate(connectionString, app.Logger);
if (!migrationResult.Successful)
{
    throw migrationResult.Error;
}
```

DbUp tracks executed scripts in a `schemaversions` table and only runs new scripts.

## Key Rules

- **Never modify an already-executed script** — Create a new script instead
- **One concern per script** — Keep scripts focused on a single schema change
- **Always test locally** — Run the application to verify migrations before pushing
- **Idempotent by default** — All DDL statements must use IF NOT EXISTS patterns
