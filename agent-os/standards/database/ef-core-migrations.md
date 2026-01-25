# EF Core Migrations

One DbContext per module, each targeting its own PostgreSQL schema.

## Create Migration

```bash
dotnet ef migrations add MigrationName \
  --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api \
  --output-dir Persistence/Migrations
```

## Apply Migration

```bash
# Development
dotnet ef database update --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api

# Production (in Program.cs)
await context.Database.MigrateAsync();
```

## Schema Separation

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.HasDefaultSchema("auth");  // Each module has its own schema
}
```

## PostgreSQL RLS

```csharp
// In migration Up() method
migrationBuilder.Sql(@"
    ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

    CREATE POLICY user_isolation_policy ON auth.users
        USING (id = current_setting('app.current_user_id')::uuid);
");
```

## Rules

- Migration name format: `{Timestamp}_{Description}`
- Always test down migrations
- One DbContext per module
- Schema name = module name (lowercase)
- Enable RLS on tenant-isolated tables
