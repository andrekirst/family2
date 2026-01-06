# Family Hub Database

**⚠️ CRITICAL: Family Hub uses EF Core Code-First migrations for all schema management.**

See `/database/docs/MIGRATION_STRATEGY.md` for the official migration strategy.

---

## Directory Structure

```
database/
├── docs/                           # Database documentation
│   ├── MIGRATION_STRATEGY.md      # ✅ Official migration strategy (EF Core)
│   ├── AUTH_SCHEMA_DESIGN.md      # Auth schema design (reference)
│   ├── reference/
│   │   └── sql-design/            # Original SQL scripts (reference only)
│   │       ├── README.md
│   │       ├── migrations/auth/   # SQL design files
│   │       └── scripts/           # Utility scripts (obsolete)
│   └── ...
└── README.md                       # This file
```

---

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- EF Core CLI tools (`dotnet tool install --global dotnet-ef`)
- PostgreSQL 16+
- Database connection string

### Setup Development Database

```bash
# 1. Start PostgreSQL (Docker Compose)
cd infrastructure/docker
docker-compose up -d postgres

# 2. Run EF Core migrations for Auth module
cd ../../src/api
dotnet ef database update \
    --context AuthDbContext \
    --project Modules/FamilyHub.Modules.Auth \
    --startup-project FamilyHub.Api

# 3. Verify schema creation
docker exec familyhub-postgres psql -U familyhub -d familyhub \
    -c "SELECT schemaname, tablename FROM pg_tables WHERE schemaname = 'auth';"
```

---

## Migration Strategy

Family Hub uses **Entity Framework Core Code-First migrations** instead of custom SQL scripts.

### Why EF Core Migrations?

1. **Type Safety** - C# code is compiled and type-checked
2. **Consistent Naming** - Follows .NET conventions (snake_case for PostgreSQL)
3. **Database Agnostic** - Works with PostgreSQL, SQL Server, SQLite
4. **Automatic Schema Tracking** - EF Core manages migration history
5. **Vogen Integration** - Value converters for strongly-typed IDs
6. **CI/CD Ready** - Programmatic migration execution
7. **Rollback Support** - `Down()` methods for reverting changes

### Architecture

- **One DbContext per module** (AuthDbContext, CalendarDbContext, etc.)
- **Each DbContext targets its own PostgreSQL schema** (auth, calendar, etc.)
- **Fluent API configurations** in `IEntityTypeConfiguration<T>` classes
- **PostgreSQL-specific features** (RLS, triggers) via `migrationBuilder.Sql()`

See `/database/docs/MIGRATION_STRATEGY.md` for comprehensive guide including:

- Creating migrations
- Applying migrations
- Rollback strategies
- Multi-module migration orchestration
- PostgreSQL-specific features (RLS, triggers)
- Testing migrations
- CI/CD integration

---

## Module Schemas

Each of the 8 DDD modules owns its own PostgreSQL schema:

| Module | Schema | DbContext | Status |
|--------|--------|-----------|--------|
| Auth | `auth` | `AuthDbContext` | 🚧 In Progress (Issue #12) |
| Calendar | `calendar` | `CalendarDbContext` | ⏳ Pending |
| Task | `tasks` | `TaskDbContext` | ⏳ Pending |
| Shopping | `shopping` | `ShoppingDbContext` | ⏳ Pending |
| Health | `health` | `HealthDbContext` | ⏳ Pending |
| Meal Planning | `meal_planning` | `MealPlanningDbContext` | ⏳ Pending |
| Finance | `finance` | `FinanceDbContext` | ⏳ Pending |
| Communication | `communication` | `CommunicationDbContext` | ⏳ Pending |

---

## Example: Auth Module

### Entity (Domain Layer)

```csharp
// /src/api/Modules/FamilyHub.Modules.Auth/Domain/User.cs
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

public class User : AggregateRoot<UserId>
{
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool EmailVerified { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Constructor, methods...
}
```

### Entity Configuration (Persistence Layer)

```csharp
// /src/api/Modules/FamilyHub.Modules.Auth/Persistence/Configurations/UserConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "auth");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("id");

        builder.Property(u => u.Email)
            .HasConversion(new Email.EfCoreValueConverter())
            .HasColumnName("email")
            .HasMaxLength(320);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        // ... more configuration
    }
}
```

### DbContext

```csharp
// /src/api/Modules/FamilyHub.Modules.Auth/Persistence/AuthDbContext.cs
using Microsoft.EntityFrameworkCore;

public class AuthDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Family> Families => Set<Family>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
```

### Creating Migration

```bash
dotnet ef migrations add InitialCreate \
    --context AuthDbContext \
    --project Modules/FamilyHub.Modules.Auth \
    --startup-project FamilyHub.Api \
    --output-dir Persistence/Migrations
```

### Generated Migration

```csharp
// Auto-generated: /src/api/Modules/FamilyHub.Modules.Auth/Persistence/Migrations/20250122_InitialCreate.cs
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "auth");

        migrationBuilder.CreateTable(
            name: "users",
            schema: "auth",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                email = table.Column<string>(maxLength: 320, nullable: false),
                // ... more columns
            });

        // Add PostgreSQL-specific features
        migrationBuilder.Sql(@"
            ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;
            CREATE POLICY users_isolation_policy ON auth.users
                USING (id = auth.current_user_id());
        ");
    }
}
```

---

## Reference Documentation

### SQL Design Scripts

The original SQL migration scripts created during Phase 0 planning are preserved in:

```
/database/docs/reference/sql-design/
```

**These scripts are NOT executed** - they serve as **reference documentation** for:

- RLS policy patterns
- Trigger logic
- Database constraints
- Seed data examples

See `/database/docs/reference/sql-design/README.md` for details.

### Schema Design

Comprehensive schema design documentation:

- **Auth Module**: `/database/docs/AUTH_SCHEMA_DESIGN.md`

These documents describe the intended database structure, relationships, and features that are implemented via EF Core migrations.

---

## Database Configuration

### Connection String

```json
{
  "ConnectionStrings": {
    "FamilyHubDb": "Host=localhost;Port=5432;Database=familyhub;Username=familyhub;Password=Dev123!"
  }
}
```

### DbContext Registration

```csharp
// Program.cs
services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString("FamilyHubDb"))
        .UseSnakeCaseNamingConvention(); // Requires EFCore.NamingConventions
});
```

---

## Common Operations

### Apply Migrations (Development)

```bash
# Auth module
dotnet ef database update --context AuthDbContext \
    --project Modules/FamilyHub.Modules.Auth \
    --startup-project FamilyHub.Api

# All modules (run script)
./database/scripts/apply-all-migrations.sh
```

### Apply Migrations (Production)

```csharp
// Programmatic migration in Program.cs
using var scope = app.Services.CreateScope();
var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
await authDbContext.Database.MigrateAsync();
```

### Rollback Migration

```bash
dotnet ef database update PreviousMigrationName \
    --context AuthDbContext \
    --project Modules/FamilyHub.Modules.Auth \
    --startup-project FamilyHub.Api
```

### Generate SQL Script (for review)

```bash
dotnet ef migrations script \
    --context AuthDbContext \
    --project Modules/FamilyHub.Modules.Auth \
    --startup-project FamilyHub.Api \
    --output auth_migrations.sql
```

---

## Key Features

### Row-Level Security (RLS)

RLS policies are added to migrations using `migrationBuilder.Sql()`:

```csharp
migrationBuilder.Sql(@"
    ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;
    CREATE POLICY users_isolation_policy ON auth.users
        USING (id = auth.current_user_id());
");
```

### Triggers

Automated workflows implemented via triggers:

```csharp
migrationBuilder.Sql(@"
    CREATE OR REPLACE FUNCTION auth.update_updated_at_column()
    RETURNS TRIGGER AS $$
    BEGIN
        NEW.updated_at = CURRENT_TIMESTAMP;
        RETURN NEW;
    END;
    $$ LANGUAGE plpgsql;

    CREATE TRIGGER update_users_updated_at
        BEFORE UPDATE ON auth.users
        FOR EACH ROW
        EXECUTE FUNCTION auth.update_updated_at_column();
");
```

### Soft Deletes

Query filters for soft delete support:

```csharp
builder.HasQueryFilter(u => u.DeletedAt == null);
```

### Audit Fields

Default values for audit timestamps:

```csharp
builder.Property(u => u.CreatedAt)
    .HasDefaultValueSql("CURRENT_TIMESTAMP");

builder.Property(u => u.UpdatedAt)
    .HasDefaultValueSql("CURRENT_TIMESTAMP");
```

---

## Testing

### Unit Tests

```csharp
[Fact]
public void Should_Configure_Email_As_Unique()
{
    var options = new DbContextOptionsBuilder<AuthDbContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;

    using var context = new AuthDbContext(options);
    var entityType = context.Model.FindEntityType(typeof(User));
    var emailIndex = entityType.FindIndex(entityType.FindProperty(nameof(User.Email)));

    Assert.NotNull(emailIndex);
    Assert.True(emailIndex.IsUnique);
}
```

### Integration Tests (Testcontainers)

```csharp
public class AuthDbContextIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();
        await _postgres.StartAsync();
    }

    [Fact]
    public async Task Should_Apply_Migrations()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        await using var context = new AuthDbContext(options);
        await context.Database.MigrateAsync();

        var tables = await context.Database.ExecuteSqlRawAsync(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'auth'"
        );
        Assert.True(tables > 0);
    }
}
```

---

## Troubleshooting

### Issue: "No migrations found"

```bash
# Ensure you're in the correct project directory
cd src/api/Modules/FamilyHub.Modules.Auth

# List all migrations
dotnet ef migrations list --context AuthDbContext
```

### Issue: "The context class could not be found"

```bash
# Specify both project and startup project
dotnet ef migrations add MigrationName \
    --context AuthDbContext \
    --project Modules/FamilyHub.Modules.Auth \
    --startup-project FamilyHub.Api
```

### Issue: Migration conflicts

```bash
# Remove conflicting migration
dotnet ef migrations remove --context AuthDbContext

# Regenerate
dotnet ef migrations add MigrationName --context AuthDbContext
```

---

## References

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [PostgreSQL 16 Documentation](https://www.postgresql.org/docs/16/)
- [Migration Strategy](/database/docs/MIGRATION_STRATEGY.md) - Comprehensive guide
- [Auth Schema Design](/database/docs/AUTH_SCHEMA_DESIGN.md) - Reference documentation
- [Family Hub Architecture](../docs/domain-model-microservices-map.md)

---

**Last Updated:** 2025-12-22
**Database:** PostgreSQL 16
**Migration Strategy:** EF Core Code-First
**Status:** ✅ Official Strategy
