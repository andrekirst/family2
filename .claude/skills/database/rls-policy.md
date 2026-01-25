---
name: rls-policy
description: Create PostgreSQL Row-Level Security policy in EF Core migration
category: database
module-aware: true
inputs:
  - tableName: Table to secure
  - module: DDD module name
  - isolationField: Field for tenant isolation (e.g., family_id, user_id)
---

# RLS Policy Skill

Create Row-Level Security policy for multi-tenant data isolation.

## Context

Load module profile: `agent-os/profiles/modules/{module}.yaml`

## Steps

### 1. Create Migration

```bash
dotnet ef migrations add Add{TableName}RlsPolicy \
  --context {Module}DbContext \
  --project Modules/FamilyHub.Modules.{Module} \
  --startup-project FamilyHub.Api \
  --output-dir Persistence/Migrations
```

### 2. Add RLS Policy in Migration

**Location:** `Modules/FamilyHub.Modules.{Module}/Persistence/Migrations/{timestamp}_Add{TableName}RlsPolicy.cs`

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

namespace FamilyHub.Modules.{Module}.Persistence.Migrations;

public partial class Add{TableName}RlsPolicy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Enable RLS on the table
        migrationBuilder.Sql(@"
            ALTER TABLE {schema}.{table_name} ENABLE ROW LEVEL SECURITY;
        ");

        // Create policy for SELECT, UPDATE, DELETE
        migrationBuilder.Sql(@"
            CREATE POLICY {table_name}_isolation_policy ON {schema}.{table_name}
                FOR ALL
                USING ({isolation_field} = current_setting('app.current_{isolation_type}_id')::uuid);
        ");

        // Create policy for INSERT (allows inserting own data)
        migrationBuilder.Sql(@"
            CREATE POLICY {table_name}_insert_policy ON {schema}.{table_name}
                FOR INSERT
                WITH CHECK ({isolation_field} = current_setting('app.current_{isolation_type}_id')::uuid);
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP POLICY IF EXISTS {table_name}_insert_policy ON {schema}.{table_name};
            DROP POLICY IF EXISTS {table_name}_isolation_policy ON {schema}.{table_name};
            ALTER TABLE {schema}.{table_name} DISABLE ROW LEVEL SECURITY;
        ");
    }
}
```

### 3. Example: Family Isolation

```csharp
// Enable RLS
migrationBuilder.Sql(@"
    ALTER TABLE family.calendar_events ENABLE ROW LEVEL SECURITY;
");

// Policy: Only see events from your family
migrationBuilder.Sql(@"
    CREATE POLICY calendar_events_family_policy ON family.calendar_events
        FOR ALL
        USING (family_id = current_setting('app.current_family_id')::uuid);
");

// Insert policy
migrationBuilder.Sql(@"
    CREATE POLICY calendar_events_insert_policy ON family.calendar_events
        FOR INSERT
        WITH CHECK (family_id = current_setting('app.current_family_id')::uuid);
");
```

### 4. Set Context in DbContext

```csharp
public class {Module}DbContext : DbContext
{
    private readonly IUserContext _userContext;

    public {Module}DbContext(
        DbContextOptions<{Module}DbContext> options,
        IUserContext userContext) : base(options)
    {
        _userContext = userContext;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await SetRlsContextAsync();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task SetRlsContextAsync()
    {
        var familyId = _userContext.GetCurrentFamilyId();
        if (familyId.HasValue)
        {
            await Database.ExecuteSqlRawAsync(
                $"SET LOCAL app.current_family_id = '{familyId.Value}'");
        }
    }
}
```

### 5. Create Interceptor (Alternative)

```csharp
public class RlsInterceptor : DbConnectionInterceptor
{
    private readonly IUserContext _userContext;

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);

        if (connection.State == ConnectionState.Open)
        {
            await SetRlsContextAsync(connection);
        }

        return result;
    }

    private async Task SetRlsContextAsync(DbConnection connection)
    {
        var familyId = _userContext.GetCurrentFamilyId();
        if (familyId.HasValue)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SET LOCAL app.current_family_id = '{familyId.Value}'";
            await command.ExecuteNonQueryAsync();
        }
    }
}
```

## Common Policies

**User-based isolation:**

```sql
CREATE POLICY users_isolation_policy ON auth.users
    USING (id = current_setting('app.current_user_id')::uuid);
```

**Family-based isolation:**

```sql
CREATE POLICY family_isolation_policy ON family.tasks
    USING (family_id = current_setting('app.current_family_id')::uuid);
```

**Role-based access:**

```sql
CREATE POLICY admin_full_access ON family.settings
    FOR ALL
    USING (
        family_id = current_setting('app.current_family_id')::uuid
        AND current_setting('app.current_user_role') IN ('OWNER', 'ADMIN')
    );
```

## Validation

- [ ] RLS enabled on table
- [ ] Policy uses correct isolation field
- [ ] Insert policy uses WITH CHECK
- [ ] Down migration removes policies
- [ ] Context set before queries execute
