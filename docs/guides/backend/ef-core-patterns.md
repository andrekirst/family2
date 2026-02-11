# EF Core Patterns

**Purpose:** Guide for Entity Framework Core usage, database configuration, and migration management in Family Hub.

**Stack:** EF Core 10, PostgreSQL 16, single `AppDbContext`, Vogen value converters.

---

## AppDbContext

Family Hub uses a **single `AppDbContext`** (not per-module). It lives in `Common/Database/` and publishes domain events after `SaveChangesAsync`:

```csharp
// src/FamilyHub.Api/Common/Database/AppDbContext.cs

public class AppDbContext : DbContext
{
    private readonly IMessageBus? _messageBus;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public AppDbContext(DbContextOptions<AppDbContext> options, IMessageBus messageBus)
        : base(options)
    {
        _messageBus = messageBus;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Family> Families { get; set; }
    public DbSet<FamilyMember> FamilyMembers { get; set; }
    public DbSet<FamilyInvitation> FamilyInvitations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

### Domain Event Publishing

`SaveChangesAsync` automatically collects domain events from all tracked aggregates and publishes them through Wolverine's `IMessageBus` after the database transaction commits:

1. Collects `DomainEvents` from all tracked `AggregateRoot<TId>` entities.
2. Calls `base.SaveChangesAsync()` to persist.
3. Publishes each event via `_messageBus.PublishAsync()`.
4. Clears events from aggregates via `ClearDomainEvents()`.

This ensures events are only published after successful persistence.

---

## Entity Configurations

Each entity has an `IEntityTypeConfiguration<T>` implementation in the module's `Data/` folder:

```
src/FamilyHub.Api/Features/{Module}/Data/
  {Entity}Configuration.cs
```

### Vogen Value Converter Integration

Every Vogen value object needs an explicit conversion in the entity configuration:

```csharp
// src/FamilyHub.Api/Features/Family/Data/FamilyInvitationConfiguration.cs

public class FamilyInvitationConfiguration : IEntityTypeConfiguration<FamilyInvitation>
{
    public void Configure(EntityTypeBuilder<FamilyInvitation> builder)
    {
        builder.ToTable("family_invitations", "family");

        // Primary key with Vogen conversion
        builder.HasKey(fi => fi.Id);
        builder.Property(fi => fi.Id)
            .HasConversion(
                id => id.Value,          // Vogen -> primitive
                value => InvitationId.From(value))  // primitive -> Vogen
            .ValueGeneratedOnAdd();

        // String Vogen value object
        builder.Property(fi => fi.InviteeEmail)
            .HasConversion(
                email => email.Value,
                value => Email.From(value))
            .HasMaxLength(320)
            .IsRequired();

        // Guid Vogen value object
        builder.Property(fi => fi.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .IsRequired();

        // Nullable Vogen value object
        builder.Property(fi => fi.AcceptedByUserId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? UserId.From(value.Value) : null)
            .IsRequired(false);
    }
}
```

### Common Configuration Patterns

**Table and schema:**

```csharp
builder.ToTable("family_invitations", "family");
```

**String value objects:**

```csharp
builder.Property(fi => fi.Role)
    .HasConversion(
        role => role.Value,
        value => FamilyRole.From(value))
    .HasMaxLength(20)
    .IsRequired();
```

**Default values:**

```csharp
builder.Property(fi => fi.Status)
    .HasDefaultValue(InvitationStatus.Pending);

builder.Property(fi => fi.CreatedAt)
    .HasDefaultValueSql("NOW()");
```

**Indexes:**

```csharp
// Unique index
builder.HasIndex(fi => fi.TokenHash)
    .IsUnique();

// Composite index
builder.HasIndex(fi => new { fi.FamilyId, fi.Status });
```

**Relationships:**

```csharp
builder.HasOne(fi => fi.Family)
    .WithMany()
    .HasForeignKey(fi => fi.FamilyId)
    .OnDelete(DeleteBehavior.Cascade);

builder.HasOne(fi => fi.InvitedByUser)
    .WithMany()
    .HasForeignKey(fi => fi.InvitedByUserId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

## Repository Pattern

Repositories wrap `AppDbContext` and expose domain-specific operations:

```csharp
// src/FamilyHub.Api/Features/Family/Infrastructure/Repositories/FamilyRepository.cs

public sealed class FamilyRepository(AppDbContext context) : IFamilyRepository
{
    public async Task<Family?> GetByIdAsync(FamilyId id, CancellationToken ct = default)
    {
        return await context.Families.FindAsync([id], cancellationToken: ct);
    }

    public async Task<Family?> GetByIdWithMembersAsync(FamilyId id, CancellationToken ct = default)
    {
        return await context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task AddAsync(Family family, CancellationToken ct = default)
    {
        await context.Families.AddAsync(family, ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}
```

### Repository Conventions

- Repository classes are `sealed` and use primary constructors.
- Located in `Infrastructure/Repositories/`.
- Interfaces are in `Domain/Repositories/`.
- Each repository exposes `SaveChangesAsync()` which delegates to `AppDbContext.SaveChangesAsync()`, triggering domain event publishing.
- Use `.Include()` for navigation properties when needed.

---

## PostgreSQL Schema Organization

Tables are organized into schemas by module:

| Schema | Module | Tables |
|--------|--------|--------|
| `auth`   | Auth   | `users` |
| `family` | Family | `families`, `family_members`, `family_invitations` |

---

## Migration Commands

### Create Migration

```bash
dotnet ef migrations add MigrationName \
  --context AppDbContext \
  --project src/FamilyHub.Api/FamilyHub.Api.csproj \
  --output-dir Common/Database/Migrations
```

### Apply Migration

```bash
# Development
dotnet ef database update \
  --context AppDbContext \
  --project src/FamilyHub.Api/FamilyHub.Api.csproj

# Production (in Program.cs)
await context.Database.MigrateAsync();
```

### Remove Last Migration

```bash
dotnet ef migrations remove \
  --context AppDbContext \
  --project src/FamilyHub.Api/FamilyHub.Api.csproj
```

---

## PostgreSQL RLS Policies

Row-Level Security can be applied in migration `Up()` methods for multi-tenant isolation:

```csharp
migrationBuilder.Sql(@"
    ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

    CREATE POLICY user_isolation_policy ON auth.users
        USING (id = current_setting('app.current_user_id')::uuid);
");
```

The `PostgresRlsMiddleware` sets the current user context on each request:

```
src/FamilyHub.Api/Common/Middleware/PostgresRlsMiddleware.cs
```

---

## Include Patterns for Navigation Properties

When querying entities with related data, use `.Include()`:

```csharp
// Single include
await context.Families
    .Include(f => f.Members)
    .FirstOrDefaultAsync(f => f.Id == id, ct);

// Avoid N+1 by including upfront
await context.FamilyInvitations
    .Include(fi => fi.Family)
    .Include(fi => fi.InvitedByUser)
    .FirstOrDefaultAsync(fi => fi.Id == id, ct);
```

---

## Related Guides

- [Vogen Value Objects](vogen-value-objects.md) -- the value objects configured with converters
- [Handler Patterns](handler-patterns.md) -- handlers that use repositories
- [Domain Events](domain-events.md) -- events published by SaveChangesAsync
- [Testing Patterns](testing-patterns.md) -- fake repositories for testing

---

**Last Updated:** 2026-02-09
