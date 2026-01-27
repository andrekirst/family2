using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.Auth.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the AuthAuditLog entity.
/// </summary>
public class AuthAuditLogConfiguration : IEntityTypeConfiguration<AuthAuditLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuthAuditLog> builder)
    {
        builder.ToTable("auth_audit_logs", "auth");

        builder.HasKey(aal => aal.Id);
        builder.Property(aal => aal.Id)
            .HasConversion(new AuthAuditLogId.EfCoreValueConverter())
            .HasColumnName("id")
            .IsRequired();

        // UserId can be null for failed login attempts with unknown email
        builder.Property(aal => aal.UserId)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (Guid?)null,
                v => v.HasValue ? UserId.From(v.Value) : null)
            .HasColumnName("user_id")
            .IsRequired(false);

        builder.HasIndex(aal => aal.UserId)
            .HasDatabaseName("ix_auth_audit_logs_user_id");

        // Email for tracking failed attempts
        builder.Property(aal => aal.Email)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : null,
                v => !string.IsNullOrEmpty(v) ? Email.From(v) : null)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired(false);

        builder.Property(aal => aal.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(50)
            .HasConversion<string>() // Store enum as string
            .IsRequired();

        builder.HasIndex(aal => aal.EventType)
            .HasDatabaseName("ix_auth_audit_logs_event_type");

        builder.Property(aal => aal.OccurredAt)
            .HasColumnName("occurred_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(aal => aal.OccurredAt)
            .HasDatabaseName("ix_auth_audit_logs_occurred_at");

        builder.Property(aal => aal.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45)
            .IsRequired(false);

        builder.Property(aal => aal.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(aal => aal.Details)
            .HasColumnName("details")
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(aal => aal.Success)
            .HasColumnName("success")
            .IsRequired();

        builder.Property(aal => aal.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(500)
            .IsRequired(false);

        // Audit fields
        builder.Property(aal => aal.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(aal => aal.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}
