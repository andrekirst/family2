using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Messaging.Data;

/// <summary>
/// EF Core configuration for Conversation aggregate.
/// Maps to PostgreSQL messaging schema with owned ConversationMember collection.
/// </summary>
public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    private static Guid SafeGuid(ConversationId id)
    {
        try { return id.Value; }
        catch (Vogen.ValueObjectValidationException) { return Guid.Empty; }
    }

    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations", "messaging");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => ConversationId.From(value),
                new ValueComparer<ConversationId>(
                    (a, b) => SafeGuid(a) == SafeGuid(b),
                    v => SafeGuid(v).GetHashCode(),
                    v => v))
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasConversion(
                name => name.Value,
                value => ConversationName.From(value))
            .HasMaxLength(ConversationName.MaxLength)
            .IsRequired();

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(c => c.FolderId)
            .HasConversion(
                id => id!.Value.Value,
                value => FolderId.From(value))
            .IsRequired(false);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Composite index for fast family conversation lookup
        builder.HasIndex(c => new { c.FamilyId, c.Type });

        // Owned collection: conversation_members table in messaging schema
        builder.OwnsMany(c => c.Members, mb =>
        {
            mb.ToTable("conversation_members", "messaging");

            mb.Property(m => m.Id)
                .HasConversion(
                    id => id.Value,
                    value => ConversationMemberId.From(value))
                .HasColumnName("id");

            mb.Property(m => m.UserId)
                .HasConversion(
                    id => id.Value,
                    value => UserId.From(value))
                .HasColumnName("user_id")
                .IsRequired();

            mb.Property(m => m.Role)
                .HasColumnName("role")
                .HasMaxLength(50)
                .IsRequired();

            mb.Property(m => m.JoinedAt)
                .HasColumnName("joined_at")
                .IsRequired();

            mb.Property(m => m.LeftAt)
                .HasColumnName("left_at")
                .IsRequired(false);

            mb.WithOwner().HasForeignKey("conversation_id");
            mb.HasKey("Id");
            mb.HasIndex("conversation_id");
            mb.HasIndex(m => m.UserId);
        });

        builder.Navigation(c => c.Members).AutoInclude();
    }
}
