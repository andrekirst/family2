using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Messaging.Data;

/// <summary>
/// EF Core configuration for Message entity.
/// Maps to PostgreSQL messaging schema with composite index for efficient timeline queries.
/// </summary>
public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages", "messaging");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                value => MessageId.From(value))
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

        builder.Property(m => m.SentAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Composite index for efficient family timeline queries (ORDER BY sent_at DESC)
        builder.HasIndex(m => new { m.FamilyId, m.SentAt })
            .IsDescending(false, true);
    }
}
