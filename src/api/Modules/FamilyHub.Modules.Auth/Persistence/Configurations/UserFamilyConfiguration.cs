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

        // Is active
        builder.Property(uf => uf.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(uf => uf.IsActive)
            .HasDatabaseName("ix_user_families_is_active");

        // Invited by with Vogen value converter
        builder.Property(uf => uf.InvitedBy)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("invited_by")
            .IsRequired(false);

        // Timestamps
        builder.Property(uf => uf.JoinedAt)
            .HasColumnName("joined_at")
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
