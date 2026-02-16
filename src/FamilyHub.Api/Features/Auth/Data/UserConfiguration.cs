using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
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

        // Primary key with Vogen converter
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .ValueGeneratedOnAdd();

        // Email - unique, required (Vogen value object)
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.From(value))
            .HasMaxLength(320)
            .IsRequired();
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Name - required (Vogen value object)
        builder.Property(u => u.Name)
            .HasConversion(
                name => name.Value,
                value => UserName.From(value))
            .HasMaxLength(200)
            .IsRequired();

        // Username - optional (for child accounts)
        builder.Property(u => u.Username)
            .HasMaxLength(100);

        // External OAuth provider fields (Vogen value object)
        builder.Property(u => u.ExternalUserId)
            .HasConversion(
                externalId => externalId.Value,
                value => ExternalUserId.From(value))
            .HasMaxLength(255)
            .IsRequired();
        builder.HasIndex(u => u.ExternalUserId)
            .IsUnique();

        builder.Property(u => u.ExternalProvider)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("KEYCLOAK");

        // Family relationship (Vogen value object - nullable)
        builder.Property(u => u.FamilyId)
            .HasConversion(
                familyId => familyId.HasValue ? familyId.Value.Value : (Guid?)null,
                value => value.HasValue ? FamilyId.From(value.Value) : null)
            .IsRequired(false);
        builder.HasIndex(u => u.FamilyId);

        // Avatar (nullable - Vogen value object)
        builder.Property(u => u.AvatarId)
            .HasConversion(
                avatarId => avatarId.HasValue ? avatarId.Value.Value : (Guid?)null,
                value => value.HasValue ? AvatarId.From(value.Value) : null)
            .IsRequired(false);

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
