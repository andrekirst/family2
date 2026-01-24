
using FamilyHub.Modules.Family.Domain;
using FamilyHub.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.Family.Persistence.Configurations;

internal sealed class EmailOutboxConfiguration : IEntityTypeConfiguration<EmailOutbox>
{
    public void Configure(EntityTypeBuilder<EmailOutbox> builder)
    {
        builder.ToTable("email_outbox", "family");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(new EmailOutboxId.EfCoreValueConverter())
            .IsRequired();

        builder.Property(e => e.OutboxEventId)
            .HasConversion(new OutboxEventId.EfCoreValueConverter())
            .IsRequired();

        builder.Property(e => e.To)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(e => e.ToName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Subject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.HtmlBody)
            .IsRequired();

        builder.Property(e => e.TextBody);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.SentAt);

        builder.Property(e => e.LastAttemptAt);

        builder.Property(e => e.RetryCount)
            .IsRequired();

        builder.Property(e => e.ErrorMessage);

        // Indexes for queries
        builder.HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("ix_email_outbox_status_created_at");

        builder.HasIndex(e => e.OutboxEventId)
            .HasDatabaseName("ix_email_outbox_outbox_event_id");
    }
}
