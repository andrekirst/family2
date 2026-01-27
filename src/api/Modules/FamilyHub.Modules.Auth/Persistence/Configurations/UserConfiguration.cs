using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.Auth.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the User entity.
/// Supports local email/password authentication with future social provider support.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "auth");

        #region Primary Key & Core Properties

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("id")
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(new Email.EfCoreValueConverter())
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        #endregion

        #region Email Verification

        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.EmailVerifiedAt)
            .HasColumnName("email_verified_at")
            .IsRequired(false);

        builder.Property(u => u.EmailVerificationToken)
            .HasColumnName("email_verification_token")
            .HasMaxLength(128)
            .IsRequired(false);

        builder.Property(u => u.EmailVerificationTokenExpiresAt)
            .HasColumnName("email_verification_token_expires_at")
            .IsRequired(false);

        #endregion

        #region Password Authentication

        builder.Property(u => u.PasswordHash)
            .HasConversion(new PasswordHash.EfCoreValueConverter())
            .HasColumnName("password_hash")
            .HasMaxLength(256)
            .IsRequired(false); // Null for social-only accounts (future)

        builder.Property(u => u.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.LockoutEndTime)
            .HasColumnName("lockout_end_time")
            .IsRequired(false);

        #endregion

        #region Password Reset

        builder.Property(u => u.PasswordResetToken)
            .HasColumnName("password_reset_token")
            .HasMaxLength(128)
            .IsRequired(false);

        builder.Property(u => u.PasswordResetTokenExpiresAt)
            .HasColumnName("password_reset_token_expires_at")
            .IsRequired(false);

        builder.Property(u => u.PasswordResetCode)
            .HasColumnName("password_reset_code")
            .HasMaxLength(6)
            .IsRequired(false);

        builder.Property(u => u.PasswordResetCodeExpiresAt)
            .HasColumnName("password_reset_code_expires_at")
            .IsRequired(false);

        #endregion

        #region Family & Role

        builder.Property(u => u.FamilyId)
            .HasConversion(new FamilyId.EfCoreValueConverter())
            .HasColumnName("family_id")
            .IsRequired();

        builder.HasIndex(u => u.FamilyId)
            .HasDatabaseName("ix_users_family_id");

        builder.Property(u => u.Role)
            .HasConversion(new FamilyRole.EfCoreValueConverter())
            .HasColumnName("role")
            .HasMaxLength(50)
            .IsRequired();

        #endregion

        #region Soft Delete & Audit

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at")
            .IsRequired(false);

        builder.HasQueryFilter(u => u.DeletedAt == null);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        #endregion

        #region Navigation Properties

        // One-to-many: User -> RefreshTokens
        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: User -> ExternalLogins
        builder.HasMany(u => u.ExternalLogins)
            .WithOne(el => el.User)
            .HasForeignKey(el => el.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion

        // Ignore domain events collection
        builder.Ignore(u => u.DomainEvents);
    }
}
