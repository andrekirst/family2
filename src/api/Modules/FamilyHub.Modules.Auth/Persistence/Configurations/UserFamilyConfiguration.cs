using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.Auth.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the UserFamily entity.
/// </summary>
public class UserFamilyConfiguration : IEntityTypeConfiguration<UserFamily>
{
    public void Configure(EntityTypeBuilder<UserFamily> builder)
    {
        builder.ToTable("user_families", "auth");

        // Primary key
        builder.HasKey(uf => uf.Id);
        builder.Property(uf => uf.Id)
            .HasColumnName("id")
            .IsRequired();

        // User ID with Vogen value converter
        builder.Property(uf => uf.UserId)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("user_id")
            .IsRequired();

        // Family ID with Vogen value converter
        builder.Property(uf => uf.FamilyId)
            .HasConversion(new FamilyId.EfCoreValueConverter())
            .HasColumnName("family_id")
            .IsRequired();

        // Unique constraint: user can only be in a family once
        builder.HasIndex(uf => new { uf.UserId, uf.FamilyId })
            .IsUnique()
            .HasDatabaseName("ix_user_families_user_family");

        // Role with Vogen value converter
        builder.Property(uf => uf.Role)
            .HasConversion(new UserRole.EfCoreValueConverter())
            .HasColumnName("role")
            .HasMaxLength(20)
            .IsRequired();

        // Is current family
        builder.Property(uf => uf.IsCurrentFamily)
            .HasColumnName("is_current_family")
            .HasDefaultValue(false)
            .IsRequired();

        // Composite index for efficient current family lookups
        builder.HasIndex(uf => new { uf.UserId, uf.IsCurrentFamily })
            .HasDatabaseName("ix_user_families_user_id_is_current_family")
            .HasFilter("is_current_family = true");

        // Timestamps
        builder.Property(uf => uf.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(uf => uf.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Relationships are already defined in User and Family configurations
        // via HasMany().WithOne()
    }
}
