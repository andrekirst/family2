---
name: ef-migration
description: Create EF Core migration for a module
category: database
module-aware: true
inputs:
  - migrationName: Migration name (e.g., AddUserTable)
  - module: DDD module name (e.g., auth, calendar)
---

# EF Core Migration Skill

Creates an EF Core migration for a specific module with schema separation.

## Context Loading

Load module profile from: `agent-os/profiles/modules/{module}.yaml`
Get schema name from profile.

## Step 1: Create Migration

Run migration command:

```bash
dotnet ef migrations add {MigrationName} \
  --context {Module}DbContext \
  --project Modules/FamilyHub.Modules.{Module} \
  --startup-project FamilyHub.Api \
  --output-dir Persistence/Migrations
```

## Step 2: Verify Migration File

Location: `Modules/FamilyHub.Modules.{Module}/Persistence/Migrations/{Timestamp}_{MigrationName}.cs`

Check the generated migration:

```csharp
public partial class {MigrationName} : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Verify schema is correct
        migrationBuilder.CreateTable(
            name: "tablename",
            schema: "{module}",  // Should match module schema
            columns: table => new { ... }
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Verify rollback is correct
    }
}
```

## Step 3: Add RLS (if table is tenant-isolated)

If the table needs Row-Level Security, add to the Up method:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ... table creation ...

    // Enable RLS
    migrationBuilder.Sql(@"
        ALTER TABLE {schema}.{tablename} ENABLE ROW LEVEL SECURITY;

        CREATE POLICY {tablename}_user_policy ON {schema}.{tablename}
            USING (user_id = current_setting('app.current_user_id', true)::uuid);
    ");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
        DROP POLICY IF EXISTS {tablename}_user_policy ON {schema}.{tablename};
        ALTER TABLE {schema}.{tablename} DISABLE ROW LEVEL SECURITY;
    ");

    // ... table deletion ...
}
```

## Step 4: Apply Migration (Development)

```bash
dotnet ef database update \
  --context {Module}DbContext \
  --project Modules/FamilyHub.Modules.{Module} \
  --startup-project FamilyHub.Api
```

## Step 5: Verify in Database

```sql
-- Check table exists
SELECT * FROM information_schema.tables
WHERE table_schema = '{module}' AND table_name = '{tablename}';

-- Check RLS is enabled
SELECT relrowsecurity FROM pg_class
WHERE relname = '{tablename}';
```

## Module Schema Reference

| Module | Schema |
|--------|--------|
| Auth | auth |
| Family | family |
| Calendar | calendar |
| Task | task |
| Shopping | shopping |
| Health | health |
| MealPlanning | meal |
| Finance | finance |
| Communication | communication |

## Vogen Value Object Configuration

For columns using Vogen types, add EF Core converter:

```csharp
// In EntityConfiguration
builder.Property(e => e.Id)
    .HasConversion(new {VogenType}.EfCoreValueConverter())
    .IsRequired();
```

## Verification

- [ ] Migration created in correct folder
- [ ] Schema matches module schema
- [ ] Down migration reverses Up correctly
- [ ] RLS enabled for tenant-isolated tables
- [ ] Vogen converters configured
- [ ] Migration applies without errors
