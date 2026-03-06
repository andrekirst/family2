# Database Development Guide

**Purpose:** Guide for PostgreSQL schema management, migrations, RLS policies, and database patterns in Family Hub.

**Tech Stack:** PostgreSQL 16, EF Core 10 (ORM only), DbUp (migrations), Row-Level Security (RLS), One schema per module

---

## Quick Reference

### Schema Organization

**One PostgreSQL schema per module:**

```
public schema     - Shared tables (cross-cutting concerns)
auth schema       - Auth module (users, families, invitations)
calendar schema   - Calendar module (events, recurrence)
task schema       - Task module (tasks, assignments)
shopping schema   - Shopping module (lists, items)
health schema     - Health module (appointments, prescriptions)
meal schema       - Meal Planning module (meals, recipes)
finance schema    - Finance module (budgets, expenses)
communication schema - Communication module (notifications)
```

**Rationale:** Schema-based separation provides logical boundaries while sharing single database (modular monolith pattern).

---

## Critical Patterns (3)

### 1. DbUp SQL Migrations

**Use DbUp SQL scripts for ALL schema changes.** Scripts are embedded as assembly resources and executed at startup.

**Create Migration:**

```bash
# Create a new SQL script in the appropriate module folder
# Use timestamp prefix for ordering: YYYYMMDDHHMMSS_kebab-case-description.sql

# Example: Add a new table to the auth module
touch src/FamilyHub.Api/Database/Migrations/auth/20260305120000_add-user-preferences.sql
```

**Script Organization:**

```
src/FamilyHub.Api/Database/Migrations/
  _bridge/            - Bridge script (EF Core → DbUp transition)
  auth/               - Auth module schemas
  family/             - Family module schemas
  calendar/           - Calendar module schemas
  event_chain/        - Event chain module schemas
  x_cross_schema/       - Cross-schema foreign keys
  rls/                - Row-Level Security policies
  ...                 - Other module folders
```

**Execution (automatic at startup in Program.cs):**

```csharp
var migrationResult = DatabaseMigrationRunner.Migrate(connectionString, app.Logger);
if (!migrationResult.Successful)
{
    throw migrationResult.Error;
}
```

**Idempotency:** All DDL must use `IF NOT EXISTS` patterns. DbUp tracks executed scripts in a `schemaversions` table.

**See:** [MIGRATION_REBASE_PROTOCOL.md](../development/MIGRATION_REBASE_PROTOCOL.md) for full protocol.

**Vogen Value Object Integration (EF Core ORM):**

```csharp
// In IEntityTypeConfiguration<User>
public void Configure(EntityTypeBuilder<User> builder)
{
    builder.ToTable("users", "auth");

    builder.Property(u => u.Id)
        .HasConversion(new UserId.EfCoreValueConverter())
        .IsRequired();

    builder.Property(u => u.Email)
        .HasConversion(new Email.EfCoreValueConverter())
        .HasMaxLength(320)
        .IsRequired();

    builder.Property(u => u.FamilyId)
        .HasConversion(new FamilyId.EfCoreValueConverter());
}
```

---

### 2. Row-Level Security (RLS) Policies

**Use PostgreSQL RLS** for multi-tenant isolation. Each module applies RLS to its tables.

**Implementation (in DbUp SQL script):**

```sql
-- src/FamilyHub.Api/Database/Migrations/rls/20260211103239_add-rls-policies.sql

ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS user_isolation_policy ON auth.users;
CREATE POLICY user_isolation_policy ON auth.users
    USING (id = current_setting('app.current_user_id', true)::uuid);

DROP POLICY IF EXISTS family_isolation_policy ON auth.users;
CREATE POLICY family_isolation_policy ON auth.users
    USING (family_id = current_setting('app.current_family_id', true)::uuid);
```

**Setting Session Variables (in DbContext or interceptor):**

```csharp
// Before executing queries
await connection.ExecuteAsync(
    "SELECT set_config('app.current_user_id', @userId, false)",
    new { userId = currentUserId.ToString() }
);

await connection.ExecuteAsync(
    "SELECT set_config('app.current_family_id', @familyId, false)",
    new { familyId = currentFamilyId.ToString() }
);
```

**See:** [docs/security/SECURITY.md](../docs/security/SECURITY.md) for comprehensive RLS patterns.

---

### 3. Schema Design Patterns

**Naming Conventions:**

- Tables: `snake_case` (e.g., `family_invitations`)
- Columns: `snake_case` (e.g., `created_at`, `family_id`)
- Primary keys: `id` (not `table_name_id`)
- Foreign keys: `referenced_table_id` (e.g., `family_id`, `user_id`)
- Indexes: `idx_table_column` (e.g., `idx_users_email`)

**Standard Columns (all tables):**

```csharp
public DateTime CreatedAt { get; private set; }
public DateTime? UpdatedAt { get; private set; }
```

**Soft Deletes (when needed):**

```csharp
public DateTime? DeletedAt { get; private set; }
public bool IsDeleted => DeletedAt.HasValue;
```

**Audit Columns (sensitive tables):**

```csharp
public UserId CreatedBy { get; private set; }
public UserId? UpdatedBy { get; private set; }
```

---

## Common Database Tasks

### Add New Table

1. Create entity in `Domain/Entities/`
2. Create configuration in `Persistence/Configurations/`
3. Add DbSet to AppDbContext
4. Create SQL script in `Database/Migrations/{module}/` with `CREATE TABLE IF NOT EXISTS`
5. Run the application — DbUp executes new scripts automatically

### Add Column to Existing Table

1. Add property to entity
2. Update configuration (if needed)
3. Create SQL script with `ALTER TABLE ... ADD COLUMN IF NOT EXISTS`
4. Run the application — DbUp executes new scripts automatically

### Add Index

```csharp
// In IEntityTypeConfiguration<T>
builder.HasIndex(e => e.Email)
    .IsUnique()
    .HasDatabaseName("idx_users_email");
```

### Add Foreign Key

```csharp
builder.HasOne(e => e.Family)
    .WithMany()
    .HasForeignKey(e => e.FamilyId)
    .OnDelete(DeleteBehavior.Cascade);
```

---

## Migration Safety Checklist

Before merging migration scripts:

- [ ] Review SQL script for correctness
- [ ] Ensure all DDL uses `IF NOT EXISTS` / idempotent patterns
- [ ] Test on local database (run the application)
- [ ] Verify no data loss
- [ ] Test RLS policies (if applicable)
- [ ] Check foreign key constraints
- [ ] Verify indexes created
- [ ] Backup production database (before production deploy)

---

## Debugging Database Issues

### Connection Errors

```bash
# Verify PostgreSQL running
docker ps | grep postgres

# Check connection string
echo $ConnectionStrings__DefaultConnection

# Test connection
psql -h localhost -U familyhub -d familyhub
```

### Migration Errors

```bash
# Error: "Script X already executed"
# Solution: Check the schemaversions table
SELECT * FROM public.schemaversions;

# Error: Script fails at startup
# Solution: Check application logs for DbUp error output
# Fix the SQL script and restart the application
```

### RLS Policy Issues

```sql
-- Check policy exists
SELECT * FROM pg_policies WHERE tablename = 'users';

-- Check session variables set
SHOW app.current_user_id;
SHOW app.current_family_id;

-- Test policy manually
SET app.current_user_id = '00000000-0000-0000-0000-000000000001';
SELECT * FROM auth.users;
```

**See:** [docs/development/DEBUGGING_GUIDE.md](../docs/development/DEBUGGING_GUIDE.md#database-issues) for comprehensive troubleshooting.

---

## Reference Documentation

### SQL Design Scripts

**Location:** `database/docs/reference/sql-design/`

**Purpose:** Historical reference only (NOT executed by DbUp). These scripts were used for initial schema design. Actual schema is managed via DbUp SQL scripts in `src/FamilyHub.Api/Database/Migrations/`.

**Files:**

- `00_schema_setup.sql` - Schema creation
- `01_core_tables.sql` - Core domain tables
- `02_module_tables.sql` - Module-specific tables
- `03_rls_policies.sql` - RLS policy examples

**IMPORTANT:** Do not execute these scripts directly. Use DbUp migration scripts instead.

---

## Related Documentation

- **Migration Protocol:** [docs/development/MIGRATION_REBASE_PROTOCOL.md](../development/MIGRATION_REBASE_PROTOCOL.md) - DbUp migration protocol
- **Workflows:** [docs/development/WORKFLOWS.md](../docs/development/WORKFLOWS.md) - Development workflows
- **Patterns:** [docs/development/PATTERNS.md](../docs/development/PATTERNS.md) - Domain patterns
- **Security:** [docs/security/SECURITY.md](../docs/security/SECURITY.md) - RLS implementation
- **Backend Guide:** [docs/guides/backend/ef-core-patterns.md](backend/ef-core-patterns.md) - EF Core ORM patterns

---

**Last Updated:** 2026-03-05
**Derived from:** Root CLAUDE.md v9.0.0
**Canonical Sources:**

- docs/development/MIGRATION_REBASE_PROTOCOL.md (DbUp migration protocol)
- docs/security/SECURITY.md (RLS policies and implementation)
- src/FamilyHub.Api/Database/Migrations/ (SQL migration scripts)

**Sync Checklist:**

- [ ] Migration commands match WORKFLOWS.md
- [ ] RLS patterns match SECURITY.md
- [ ] Naming conventions match CODING_STANDARDS.md
- [ ] Schema organization aligns with modular monolith architecture
