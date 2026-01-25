# PostgreSQL Row-Level Security

RLS enforces data isolation at database level. Defense in depth for multi-tenancy.

## Enable RLS

```sql
ALTER TABLE auth.families ENABLE ROW LEVEL SECURITY;
```

## User Isolation Policy

```sql
CREATE POLICY user_isolation_policy ON auth.users
    USING (id = current_setting('app.current_user_id', true)::uuid);
```

## Family Isolation Policy

```sql
CREATE POLICY family_isolation_policy ON auth.family_members
    USING (family_id = current_setting('app.current_family_id', true)::uuid);
```

## Middleware Setup

```csharp
// PostgresRlsContextMiddleware
app.Use(async (context, next) =>
{
    var userId = context.User.FindFirstValue("sub");
    await dbConnection.ExecuteAsync(
        "SET app.current_user_id = @userId",
        new { userId });
    await next();
});
```

## Rules

- Enable RLS on all tenant-specific tables
- Use `current_setting(..., true)` for safe NULL handling
- Set session variables in middleware before queries
- Transaction-scoped variables for fail-secure behavior
- Test RLS with integration tests
