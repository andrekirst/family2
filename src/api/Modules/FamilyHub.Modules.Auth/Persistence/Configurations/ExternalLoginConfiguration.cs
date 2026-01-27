using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.Auth.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the ExternalLogin entity.
/// Prepared for future social login integration (Google, Apple, Microsoft).
/// </summary>
public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("external_logins", "auth");

        builder.HasKey(el => el.Id);
        builder.Property(el => el.Id)
            .HasConversion(new ExternalLoginId.EfCoreValueConverter())
            .HasColumnName("id")
            .IsRequired();

        builder.Property(el => el.UserId)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(el => el.UserId)
            .HasDatabaseName("ix_external_logins_user_id");

        builder.Property(el => el.Provider)
            .HasColumnName("provider")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(el => el.ProviderUserId)
            .HasColumnName("provider_user_id")
            .HasMaxLength(255)
            .IsRequired();

        // Unique constraint: one provider account can only be linked to one user
        builder.HasIndex(el => new { el.Provider, el.ProviderUserId })
            .HasDatabaseName("ix_external_logins_provider_user_id")
            .IsUnique();

        builder.Property(el => el.ProviderEmail)
            .HasColumnName("provider_email")
            .HasMaxLength(320)
            .IsRequired(false);

        builder.Property(el => el.ProviderDisplayName)
            .HasColumnName("provider_display_name")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(el => el.LinkedAt)
            .HasColumnName("linked_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(el => el.LastUsedAt)
            .HasColumnName("last_used_at")
            .IsRequired(false);

        // Audit fields
        builder.Property(el => el.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(el => el.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}
