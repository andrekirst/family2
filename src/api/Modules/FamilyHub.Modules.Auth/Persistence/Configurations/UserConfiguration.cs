using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.Auth.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the User entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "auth");

        // Primary key with Vogen value converter
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("id")
            .IsRequired();

        // Email with Vogen value converter
        builder.Property(u => u.Email)
            .HasConversion(new Email.EfCoreValueConverter())
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        // Email verification
        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.EmailVerifiedAt)
            .HasColumnName("email_verified_at")
            .IsRequired(false);

        // External OAuth provider (required - all users authenticate via OAuth)
        builder.Property(u => u.ExternalUserId)
            .HasColumnName("external_user_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.ExternalProvider)
            .HasColumnName("external_provider")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(u => new { u.ExternalProvider, u.ExternalUserId })
            .HasDatabaseName("ix_users_external_provider_user_id")
            .IsUnique();

        // Soft delete
        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at")
            .IsRequired(false);

        builder.HasQueryFilter(u => u.DeletedAt == null);

        // Audit fields
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Family relationship - User belongs to one Family
        // PHASE 5 STATE: FamilyId references family.families.id (cross-schema, no FK constraint)
        // Application-level validation via IUserLookupService maintains consistency
        // NOTE: FamilyId uses Guid.Empty to represent "no family" rather than null
        // (Vogen value types cannot be null; future refactoring could use FamilyId? for cleaner semantics)
        builder.Property(u => u.FamilyId)
            .HasConversion(new FamilyId.EfCoreValueConverter())
            .HasColumnName("family_id")
            .IsRequired();

        builder.HasIndex(u => u.FamilyId)
            .HasDatabaseName("ix_users_family_id");

        // Role with Vogen value converter
        builder.Property(u => u.Role)
            .HasConversion(new FamilyRole.EfCoreValueConverter())
            .HasColumnName("role")
            .HasMaxLength(50)
            .IsRequired();


        // Ignore domain events collection (handled by base class)
        builder.Ignore(u => u.DomainEvents);
    }
}
