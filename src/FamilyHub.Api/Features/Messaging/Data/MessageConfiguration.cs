using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Messaging.Data;

/// <summary>
/// EF Core configuration for Message entity.
/// Maps to PostgreSQL messaging schema with composite index for efficient timeline queries.
/// </summary>
public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    /// <summary>
    /// Safely extracts the underlying Guid from a MessageId without triggering
    /// Vogen's ThrowWhenNotInitialized check on default/uninitialized structs.
    /// Required because EF Core's ValueComparer calls GetHashCode() on default
    /// values during change tracking of owned entity shadow FK properties.
    /// </summary>
    private static Guid SafeGuid(MessageId id)
    {
        try { return id.Value; }
        catch (Vogen.ValueObjectValidationException) { return Guid.Empty; }
    }

    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages", "messaging");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                value => MessageId.From(value),
                // Custom comparer that handles uninitialized Vogen VOs safely.
                // EF Core propagates this to owned entity shadow FK properties,
                // preventing ValueObjectValidationException during change tracking.
                new ValueComparer<MessageId>(
                    (a, b) => SafeGuid(a) == SafeGuid(b),
                    v => SafeGuid(v).GetHashCode(),
                    v => v))
            .ValueGeneratedOnAdd();

        builder.Property(m => m.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .IsRequired();

        builder.Property(m => m.SenderId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(m => m.Content)
            .HasConversion(
                content => content.Value,
                value => MessageContent.From(value))
            .HasMaxLength(MessageContent.MaxLength)
            .IsRequired();

        builder.Property(m => m.ConversationId)
            .HasConversion(
                id => id!.Value.Value,
                value => ConversationId.From(value))
            .IsRequired(false);

        builder.Property(m => m.SentAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Composite index for efficient family timeline queries (ORDER BY sent_at DESC)
        builder.HasIndex(m => new { m.FamilyId, m.SentAt })
            .IsDescending(false, true);

        // Index for conversation-scoped timeline queries
        builder.HasIndex(m => new { m.ConversationId, m.SentAt })
            .IsDescending(false, true);

        // Owned collection: message_attachments table in messaging schema
        builder.OwnsMany(m => m.Attachments, ab =>
        {
            ab.ToTable("message_attachments", "messaging");

            ab.Property(a => a.Id)
                .HasColumnName("id");

            ab.Property(a => a.FileId)
                .HasConversion(
                    id => id.Value,
                    value => FileId.From(value))
                .HasColumnName("file_id")
                .IsRequired();

            ab.Property(a => a.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255)
                .IsRequired();

            ab.Property(a => a.MimeType)
                .HasColumnName("mime_type")
                .HasMaxLength(127)
                .IsRequired();

            ab.Property(a => a.FileSize)
                .HasColumnName("file_size")
                .IsRequired();

            ab.Property(a => a.StorageKey)
                .HasColumnName("storage_key")
                .HasMaxLength(255)
                .IsRequired(false);

            ab.Property(a => a.AttachedAt)
                .HasColumnName("attached_at")
                .IsRequired();

            ab.WithOwner().HasForeignKey("message_id");
            ab.HasKey("Id");
            ab.HasIndex("message_id");
        });

        builder.Navigation(m => m.Attachments).AutoInclude();
    }
}
