using FamilyHub.Api.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Auth.Data;

/// <summary>
/// EF Core configuration for User entity
/// Maps to PostgreSQL auth schema with RLS policies applied via migration
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table mapping to auth schema
        builder.ToTable("users", "auth");

        // Primary key
        builder.HasKey(u => u.Id);

        // Email - unique, required
        builder.Property(u => u.Email)
            .HasMaxLength(320)
            .IsRequired();
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Name - required
        builder.Property(u => u.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Username - optional (for child accounts)
        builder.Property(u => u.Username)
            .HasMaxLength(100);

        // External OAuth provider fields
        builder.Property(u => u.ExternalUserId)
            .HasMaxLength(255)
            .IsRequired();
        builder.HasIndex(u => u.ExternalUserId)
            .IsUnique();

        builder.Property(u => u.ExternalProvider)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("KEYCLOAK");

        // Family relationship
        builder.Property(u => u.FamilyId)
            .IsRequired(false);
        builder.HasIndex(u => u.FamilyId);

        // Metadata fields
        builder.Property(u => u.EmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.LastLoginAt)
            .IsRequired(false);

        builder.Property(u => u.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Relationships
        builder.HasOne(u => u.Family)
            .WithMany(f => f.Members)
            .HasForeignKey(u => u.FamilyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
