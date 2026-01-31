# Database Development Guide

**Purpose:** Guide for PostgreSQL schema management, migrations, RLS policies, and database patterns in Family Hub.

**Tech Stack:** PostgreSQL 16, EF Core 10, Row-Level Security (RLS), One schema per module

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

### 1. EF Core Migrations (Code-First)

**Use Code-First migrations for ALL schema changes.** Never write raw SQL scripts.

**Create Migration:**

```bash
# General pattern
dotnet ef migrations add <MigrationName> \
  --context <ModuleDbContext> \
  --project Modules/FamilyHub.Modules.<Module> \
  --startup-project FamilyHub.Api \
  --output-dir Persistence/Migrations

# Example: Auth module
dotnet ef migrations add AddFamilyInvitations \
  --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api \
  --output-dir Persistence/Migrations
```

**Apply Migration:**

```bash
# Development
dotnet ef database update --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api

# Production (in Program.cs)
await context.Database.MigrateAsync();
```

**Vogen Value Object Integration:**

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

**Implementation (in migration Up() method):**

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Create table
    migrationBuilder.CreateTable(
        name: "users",
        schema: "auth",
        columns: table => new
        {
            id = table.Column<Guid>(nullable: false),
            email = table.Column<string>(maxLength: 320, nullable: false),
            family_id = table.Column<Guid>(nullable: true)
        });

    // Enable RLS
    migrationBuilder.Sql(@"
        ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

        CREATE POLICY user_isolation_policy ON auth.users
            USING (id = current_setting('app.current_user_id', true)::uuid);

        CREATE POLICY family_isolation_policy ON auth.users
            USING (family_id = current_setting('app.current_family_id', true)::uuid);
    ");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
        DROP POLICY IF EXISTS user_isolation_policy ON auth.users;
        DROP POLICY IF EXISTS family_isolation_policy ON auth.users;
        ALTER TABLE auth.users DISABLE ROW LEVEL SECURITY;
    ");

    migrationBuilder.DropTable(name: "users", schema: "auth");
}
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
3. Add DbSet to DbContext
4. Create migration: `dotnet ef migrations add AddTableName`
5. Apply migration: `dotnet ef database update`

### Add Column to Existing Table

1. Add property to entity
2. Update configuration (if needed)
3. Create migration: `dotnet ef migrations add AddColumnName`
4. Review generated migration
5. Apply migration

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

Before applying migrations:

- [ ] Review generated SQL (check `Migrations/<Timestamp>_<Name>.cs`)
- [ ] Test on local database
- [ ] Verify no data loss (check Down() method)
- [ ] Test RLS policies (if applicable)
- [ ] Check foreign key constraints
- [ ] Verify indexes created
- [ ] Test rollback (Down() method)
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
# Error: "Migration X already applied"
# Solution: Check __EFMigrationsHistory table
SELECT * FROM auth.__EFMigrationsHistory;

# Error: "Sequence contains no elements"
# Solution: Verify DbContext registered in Program.cs
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

**Purpose:** Historical reference only (NOT executed). These scripts were used for initial schema design. Actual schema is managed via EF Core migrations.

**Files:**

- `00_schema_setup.sql` - Schema creation
- `01_core_tables.sql` - Core domain tables
- `02_module_tables.sql` - Module-specific tables
- `03_rls_policies.sql` - RLS policy examples

**IMPORTANT:** Do not execute these scripts directly. Use EF Core migrations instead.

---

## Related Documentation

- **Workflows:** [docs/development/WORKFLOWS.md](../docs/development/WORKFLOWS.md#database-migrations-with-ef-core) - Migration commands
- **Patterns:** [docs/development/PATTERNS.md](../docs/development/PATTERNS.md) - Domain patterns
- **Security:** [docs/security/SECURITY.md](../docs/security/SECURITY.md) - RLS implementation
- **Backend Guide:** [src/api/CLAUDE.md](../src/api/CLAUDE.md) - EF Core patterns

---

**Last Updated:** 2026-01-09
**Derived from:** Root CLAUDE.md v5.0.0
**Canonical Sources:**

- docs/development/WORKFLOWS.md (EF Core migration commands)
- docs/security/SECURITY.md (RLS policies and implementation)
- src/api/Persistence/Configurations/ (Entity configurations)

**Sync Checklist:**

- [ ] Migration commands match WORKFLOWS.md
- [ ] RLS patterns match SECURITY.md
- [ ] Naming conventions match CODING_STANDARDS.md
- [ ] Schema organization aligns with modular monolith architecture
