---
name: entity-config
description: Create EF Core entity configuration with Vogen support
category: database
module-aware: true
inputs:
  - entityName: Entity to configure
  - module: DDD module name
  - tableName: Database table name
---

# EF Core Entity Configuration Skill

Create EF Core entity configuration with Vogen value converters and proper column mappings.

## Context

Load module profile: `agent-os/profiles/modules/{module}.yaml`

## Steps

### 1. Create Configuration Class

**Location:** `Modules/FamilyHub.Modules.{Module}/Persistence/Configurations/{Entity}Configuration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyHub.Modules.{Module}.Domain.Entities;
using FamilyHub.Modules.{Module}.Domain.ValueObjects;

namespace FamilyHub.Modules.{Module}.Persistence.Configurations;

public class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
    public void Configure(EntityTypeBuilder<{Entity}> builder)
    {
        // Table
        builder.ToTable("{table_name}", "{schema}");

        // Primary Key
        builder.HasKey(e => e.Id);

        // Vogen Value Object Columns
        builder.Property(e => e.Id)
            .HasConversion(new {EntityId}.EfCoreValueConverter())
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasConversion(new {EntityName}.EfCoreValueConverter())
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasConversion(new Email.EfCoreValueConverter())
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        // Standard Columns
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("ix_{table_name}_email");

        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);
    }
}
```

### 2. Configure Relationships

**One-to-Many:**

```csharp
// In FamilyConfiguration
builder.HasMany(f => f.Members)
    .WithOne(m => m.Family)
    .HasForeignKey(m => m.FamilyId)
    .OnDelete(DeleteBehavior.Cascade);
```

**Many-to-Many:**

```csharp
// With join entity
builder.HasMany(e => e.Tags)
    .WithMany(t => t.Entities)
    .UsingEntity<EntityTag>(
        j => j.HasOne(et => et.Tag).WithMany().HasForeignKey(et => et.TagId),
        j => j.HasOne(et => et.Entity).WithMany().HasForeignKey(et => et.EntityId),
        j => j.ToTable("entity_tags", "{schema}")
    );
```

**One-to-One:**

```csharp
builder.HasOne(u => u.Profile)
    .WithOne(p => p.User)
    .HasForeignKey<UserProfile>(p => p.UserId);
```

### 3. Configure Owned Types

```csharp
// Value object as owned type (stored in same table)
builder.OwnsOne(e => e.Address, address =>
{
    address.Property(a => a.Street)
        .HasColumnName("address_street")
        .HasMaxLength(200);

    address.Property(a => a.City)
        .HasColumnName("address_city")
        .HasMaxLength(100);

    address.Property(a => a.PostalCode)
        .HasColumnName("address_postal_code")
        .HasMaxLength(20);
});
```

### 4. Configure Soft Delete

```csharp
builder.Property(e => e.IsDeleted)
    .HasColumnName("is_deleted")
    .HasDefaultValue(false);

builder.Property(e => e.DeletedAt)
    .HasColumnName("deleted_at");

// Global query filter
builder.HasQueryFilter(e => !e.IsDeleted);
```

### 5. Configure Audit Columns

```csharp
builder.Property(e => e.CreatedAt)
    .HasColumnName("created_at")
    .IsRequired()
    .ValueGeneratedOnAdd()
    .HasDefaultValueSql("NOW()");

builder.Property(e => e.CreatedBy)
    .HasConversion(new UserId.EfCoreValueConverter())
    .HasColumnName("created_by");

builder.Property(e => e.UpdatedAt)
    .HasColumnName("updated_at")
    .ValueGeneratedOnUpdate();

builder.Property(e => e.UpdatedBy)
    .HasConversion(new UserId.EfCoreValueConverter())
    .HasColumnName("updated_by");
```

### 6. Register in DbContext

```csharp
public class {Module}DbContext : DbContext
{
    public DbSet<{Entity}> {Entities} => Set<{Entity}>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new {Entity}Configuration());

        // Or apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof({Module}DbContext).Assembly);
    }
}
```

## Naming Conventions

| C# | PostgreSQL |
|----|------------|
| `FamilyId` | `family_id` |
| `CreatedAt` | `created_at` |
| `IsActive` | `is_active` |

## Validation

- [ ] Configuration in Persistence/Configurations/
- [ ] Table and schema specified
- [ ] All Vogen types use EfCoreValueConverter
- [ ] Column names use snake_case
- [ ] Indexes created for frequently queried fields
- [ ] Relationships configured with proper delete behavior
- [ ] DomainEvents ignored
