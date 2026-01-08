using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.Auth.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Family entity.
///
/// PHASE 3 NOTE: This configuration currently specifies schema "auth" for backward compatibility.
/// The Family table remains in the auth schema to avoid database migration complexity during
/// the logical extraction phase. In Phase 5+, when we introduce a separate FamilyDbContext,
/// we will migrate this table to the "family" schema.
/// </summary>
public class FamilyConfiguration : IEntityTypeConfiguration<FamilyAggregate>
{
    public void Configure(EntityTypeBuilder<FamilyAggregate> builder)
    {
        // PHASE 3 COUPLING: Table remains in "auth" schema for now
        // TODO Phase 5+: Migrate to "family" schema when introducing FamilyDbContext
        builder.ToTable("families", "auth");

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
