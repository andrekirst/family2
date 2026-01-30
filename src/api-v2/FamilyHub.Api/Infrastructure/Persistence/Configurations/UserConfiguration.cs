using FamilyHub.Api.Domain.Entities;
using FamilyHub.Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EmailVO = FamilyHub.Api.Domain.ValueObjects.Email;

namespace FamilyHub.Api.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("id")
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(new EmailVO.EfCoreValueConverter())
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasConversion(new PasswordHash.EfCoreValueConverter())
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .IsRequired();

        builder.Property(u => u.EmailVerifiedAt)
            .HasColumnName("email_verified_at");

        builder.Property(u => u.EmailVerificationToken)
            .HasColumnName("email_verification_token")
            .HasMaxLength(100);

        builder.Property(u => u.EmailVerificationTokenExpiresAt)
            .HasColumnName("email_verification_token_expires_at");

        builder.Property(u => u.PasswordResetToken)
            .HasColumnName("password_reset_token")
            .HasMaxLength(100);

        builder.Property(u => u.PasswordResetTokenExpiresAt)
            .HasColumnName("password_reset_token_expires_at");

        builder.Property(u => u.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .IsRequired();

        builder.Property(u => u.LockoutEndTime)
            .HasColumnName("lockout_end_time");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at");

        // Soft delete filter
        builder.HasQueryFilter(u => u.DeletedAt == null);

        // Relationship to RefreshTokens
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
