# Database Migration Strategy - EF Core Code-First

**Last Updated:** 2025-12-22
**Status:** ✅ Official Strategy (replaces custom SQL scripts)

---

## Overview

Family Hub uses **Entity Framework Core Code-First migrations** for all database schema management. This provides type safety, consistent naming conventions, and seamless integration with the .NET codebase.

## Why EF Core Migrations?

### Benefits Over Custom SQL Scripts

1. **Type Safety** - C# code is compiled and type-checked at build time
2. **Database Provider Agnostic** - Works with PostgreSQL, SQL Server, SQLite (testing)
3. **Automatic Schema Tracking** - EF Core manages `__EFMigrationsHistory` table
4. **Consistent Naming** - Follows .NET naming conventions
5. **Code-First DDD** - Entities and value objects drive schema
6. **CI/CD Integration** - Programmatic migration execution
7. **Rollback Support** - `Down()` methods for reverting changes
8. **Vogen Integration** - Value converters work seamlessly
9. **Version Control** - Migrations are C# files in git
10. **IntelliSense Support** - Full IDE support for schema changes

### Trade-offs

- PostgreSQL-specific features (RLS, triggers) require `migrationBuilder.Sql()`
- Initial learning curve for developers unfamiliar with EF Core
- Migration conflicts require manual resolution

---

## Architecture: Modular Monolith with Multiple DbContexts

Each of the 8 DDD modules has its own `DbContext`, targeting its own PostgreSQL schema:

```
/src/api/
├── Modules/
│   ├── FamilyHub.Modules.Auth/
│   │   ├── Domain/
│   │   │   ├── User.cs              # Aggregate root
│   │   │   ├── Family.cs            # Aggregate root
│   │   │   └── UserFamily.cs        # Relationship entity
│   │   ├── Persistence/
│   │   │   ├── AuthDbContext.cs     # EF Core DbContext for 'auth' schema
│   │   │   ├── Configurations/
│   │   │   │   ├── UserConfiguration.cs     # Fluent API config
│   │   │   │   ├── FamilyConfiguration.cs
│   │   │   │   └── UserFamilyConfiguration.cs
│   │   │   └── Migrations/          # EF Core migrations (auto-generated)
│   │   │       ├── 20250122_InitialCreate.cs
│   │   │       ├── 20250122_InitialCreate.Designer.cs
│   │   │       └── AuthDbContextModelSnapshot.cs
│   │   └── ...
│   ├── FamilyHub.Modules.Calendar/
│   │   ├── Persistence/
│   │   │   ├── CalendarDbContext.cs # Targets 'calendar' schema
│   │   │   └── Migrations/
│   └── ...
```

---

## DbContext Configuration

### Example: AuthDbContext

```csharp
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<UserFamily> UserFamilies => Set<UserFamily>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set PostgreSQL schema for this module
        modelBuilder.HasDefaultSchema("auth");

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
```

### Entity Configuration with Fluent API

```csharp
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.Auth.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "auth");

        // Primary key with Vogen value converter
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("id")
            .IsRequired();

        // Email with Vogen value converter
        builder.Property(u => u.Email)
            .HasConversion(new Email.EfCoreValueConverter())
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        // Password hash
        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        // Email verification
        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false);

        builder.Property(u => u.EmailVerifiedAt)
            .HasColumnName("email_verified_at")
            .IsRequired(false);

        // Soft delete
        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at")
            .IsRequired(false);

        builder.HasQueryFilter(u => u.DeletedAt == null);

        // Audit fields
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Relationships
        builder.HasMany(u => u.UserFamilies)
            .WithOne(uf => uf.User)
            .HasForeignKey(uf => uf.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

## Creating Migrations

### 1. Add a Migration

```bash
# Navigate to module folder
cd src/api/Modules/FamilyHub.Modules.Auth

# Create migration
dotnet ef migrations add InitialCreate \
    --context AuthDbContext \
    --project . \
    --startup-project ../../FamilyHub.Api \
    --output-dir Persistence/Migrations
```

### 2. Review Generated Migration

```csharp
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
                password_hash = table.Column<string>(maxLength: 255, nullable: false),
                email_verified = table.Column<bool>(nullable: false, defaultValue: false),
                email_verified_at = table.Column<DateTime>(nullable: true),
                deleted_at = table.Column<DateTime>(nullable: true),
                created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                updated_at = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_users_email",
            schema: "auth",
            table: "users",
            column: "email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "users", schema: "auth");
    }
}
```

### 3. Add PostgreSQL-Specific Features (RLS, Triggers)

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ... table creation ...

        // Enable Row-Level Security
        migrationBuilder.Sql(@"
            ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;
        ");

        // Create RLS helper function
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION auth.current_user_id()
            RETURNS UUID AS $$
            BEGIN
                RETURN NULLIF(current_setting('app.current_user_id', TRUE), '')::UUID;
            END;
            $$ LANGUAGE plpgsql SECURITY DEFINER;
        ");

        // Create RLS policy
        migrationBuilder.Sql(@"
            CREATE POLICY users_isolation_policy ON auth.users
                USING (id = auth.current_user_id());
        ");

        // Create trigger for updated_at
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
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP POLICY IF EXISTS users_isolation_policy ON auth.users;");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS auth.current_user_id();");
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS update_users_updated_at ON auth.users;");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS auth.update_updated_at_column();");

        // ... table deletion ...
    }
}
```

---

## Applying Migrations

### Development (Local)

```bash
# Apply all pending migrations for Auth module
dotnet ef database update \
    --context AuthDbContext \
    --project src/api/Modules/FamilyHub.Modules.Auth \
    --startup-project src/api/FamilyHub.Api

# Apply specific migration
dotnet ef database update 20250122_InitialCreate \
    --context AuthDbContext \
    --project src/api/Modules/FamilyHub.Modules.Auth \
    --startup-project src/api/FamilyHub.Api
```

### Production (Programmatic)

```csharp
// In Program.cs or startup code
using var scope = app.Services.CreateScope();
var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
await authDbContext.Database.MigrateAsync(); // Apply pending migrations
```

### CI/CD Pipeline (GitHub Actions)

```yaml
- name: Apply Auth Module Migrations
  run: |
    dotnet ef database update \
      --context AuthDbContext \
      --project src/api/Modules/FamilyHub.Modules.Auth \
      --startup-project src/api/FamilyHub.Api \
      --no-build
  env:
    ConnectionStrings__FamilyHubDb: ${{ secrets.DATABASE_CONNECTION_STRING }}
```

---

## Rollback Migrations

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName \
    --context AuthDbContext \
    --project src/api/Modules/FamilyHub.Modules.Auth \
    --startup-project src/api/FamilyHub.Api

# Rollback all migrations (reset schema)
dotnet ef database update 0 \
    --context AuthDbContext \
    --project src/api/Modules/FamilyHub.Modules.Auth \
    --startup-project src/api/FamilyHub.Api
```

---

## Naming Conventions

EF Core uses **snake_case** for PostgreSQL by default when configured:

```csharp
// In Program.cs
services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention(); // Requires EFCore.NamingConventions NuGet package
});
```

### Naming Rules

- **Tables**: Plural, snake_case (e.g., `users`, `user_families`)
- **Columns**: snake_case (e.g., `email_verified_at`, `password_hash`)
- **Indexes**: `ix_{table}_{column}` (e.g., `ix_users_email`)
- **Primary Keys**: `pk_{table}` (e.g., `pk_users`)
- **Foreign Keys**: `fk_{table}_{referenced_table}_{column}` (e.g., `fk_user_families_users_user_id`)

---

## Seed Data

### Using HasData (Simple Data)

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ... configuration ...

        builder.HasData(
            new User
            {
                Id = UserId.From(Guid.Parse("...")),
                Email = Email.From("admin@familyhub.com"),
                PasswordHash = "...", // bcrypt hash
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
```

### Using Custom Migration (Complex Data)

```csharp
public partial class SeedTestData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            schema: "auth",
            table: "users",
            columns: new[] { "id", "email", "password_hash", "email_verified", "created_at", "updated_at" },
            values: new object[,]
            {
                { Guid.NewGuid(), "test1@example.com", "$2a$12$...", false, DateTime.UtcNow, DateTime.UtcNow },
                { Guid.NewGuid(), "test2@example.com", "$2a$12$...", true, DateTime.UtcNow, DateTime.UtcNow }
            }
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "auth",
            table: "users",
            keyColumn: "email",
            keyValues: new[] { "test1@example.com", "test2@example.com" }
        );
    }
}
```

---

## Multi-Module Migration Strategy

### Applying All Modules in Order

```bash
#!/bin/bash
# apply-all-migrations.sh

MODULES=(
    "FamilyHub.Modules.Auth:AuthDbContext"
    "FamilyHub.Modules.Calendar:CalendarDbContext"
    "FamilyHub.Modules.Task:TaskDbContext"
    "FamilyHub.Modules.Shopping:ShoppingDbContext"
    "FamilyHub.Modules.Health:HealthDbContext"
    "FamilyHub.Modules.MealPlanning:MealPlanningDbContext"
    "FamilyHub.Modules.Finance:FinanceDbContext"
    "FamilyHub.Modules.Communication:CommunicationDbContext"
)

for module_context in "${MODULES[@]}"; do
    IFS=':' read -r module context <<< "$module_context"
    echo "Applying migrations for $module ($context)..."

    dotnet ef database update \
        --context "$context" \
        --project "src/api/Modules/$module" \
        --startup-project "src/api/FamilyHub.Api" \
        --no-build

    if [ $? -ne 0 ]; then
        echo "❌ Failed to apply migrations for $module"
        exit 1
    fi

    echo "✅ Migrations applied for $module"
done

echo "🎉 All migrations applied successfully"
```

---

## Testing Migrations

### Unit Tests for Entity Configurations

```csharp
public class UserConfigurationTests
{
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
}
```

### Integration Tests with Testcontainers

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
    public async Task Should_Create_Schema_And_Tables()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        await using var context = new AuthDbContext(options);
        await context.Database.MigrateAsync();

        // Verify schema exists
        var schemaExists = await context.Database.ExecuteSqlRawAsync(
            "SELECT 1 FROM information_schema.schemata WHERE schema_name = 'auth'"
        );
        Assert.Equal(1, schemaExists);
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
```

---

## Troubleshooting

### Common Issues

**Issue: "No migrations configuration type was found"**
```bash
# Ensure you're specifying the correct context
dotnet ef migrations add InitialCreate --context AuthDbContext
```

**Issue: "The context class 'AuthDbContext' could not be found"**
```bash
# Ensure you're in the correct project directory
cd src/api/Modules/FamilyHub.Modules.Auth
```

**Issue: Migration conflicts**
```bash
# Remove conflicting migration
dotnet ef migrations remove --context AuthDbContext

# Regenerate migration
dotnet ef migrations add MigrationName --context AuthDbContext
```

---

## Reference: SQL Scripts as Design Documentation

The original SQL migration scripts created by the database administrator agent are preserved in `/database/docs/reference/sql-design/` as **design documentation**. They serve as:

- Reference for RLS policy design
- Reference for trigger logic
- Documentation of database constraints
- Examples of PostgreSQL-specific features

**These scripts are NOT executed** - they are reference material only. The actual schema is created via EF Core migrations.

---

## Summary

✅ **Use EF Core Code-First migrations** for all schema changes
✅ **One DbContext per module**, targeting separate PostgreSQL schemas
✅ **Fluent API configurations** in dedicated `IEntityTypeConfiguration<T>` classes
✅ **Vogen value converters** for strongly-typed IDs and value objects
✅ **PostgreSQL-specific features** via `migrationBuilder.Sql()`
✅ **Snake_case naming** with EFCore.NamingConventions
✅ **Programmatic migration** execution in production
✅ **SQL scripts preserved** as reference documentation

This strategy provides type safety, consistency, and maintainability while leveraging the full power of EF Core and PostgreSQL.
