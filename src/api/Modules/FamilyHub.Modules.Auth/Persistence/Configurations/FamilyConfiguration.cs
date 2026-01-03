using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.Auth.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Family entity.
/// </summary>
public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
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
