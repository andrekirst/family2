using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Modules.Family.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Family aggregate.
///
/// PHASE 5 STATE: Table resides in "family" schema after migration from "auth" schema.
///
/// CROSS-SCHEMA REFERENCES:
/// - OwnerId references auth.users.id but without FK constraint
/// - Consistency maintained via application-level validation (IUserLookupService)
/// - This approach enables proper bounded context separation
/// </summary>
public class FamilyConfiguration : IEntityTypeConfiguration<FamilyAggregate>
{
    public void Configure(EntityTypeBuilder<FamilyAggregate> builder)
    {
        // Table in "family" schema
        builder.ToTable("families", "family");

        // Primary key with Vogen value converter
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(new FamilyId.EfCoreValueConverter())
            .HasColumnName("id")
            .IsRequired();

        // Family name with Vogen value converter
        builder.Property(f => f.Name)
            .HasConversion(new FamilyName.EfCoreValueConverter())
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        // Owner ID with Vogen value converter
        // NOTE: No FK constraint - cross-schema reference to auth.users
        // Consistency maintained via IUserLookupService
        builder.Property(f => f.OwnerId)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("owner_id")
            .IsRequired();

        builder.HasIndex(f => f.OwnerId)
            .HasDatabaseName("ix_families_owner_id");

        // Soft delete
        builder.Property(f => f.DeletedAt)
            .HasColumnName("deleted_at")
            .IsRequired(false);

        builder.HasQueryFilter(f => f.DeletedAt == null);

        // Audit fields
        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Ignore domain events collection
        builder.Ignore(f => f.DomainEvents);
    }
}
