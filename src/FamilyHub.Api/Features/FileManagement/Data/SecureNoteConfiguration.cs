using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Data;

public sealed class SecureNoteConfiguration : IEntityTypeConfiguration<SecureNote>
{
    public void Configure(EntityTypeBuilder<SecureNote> builder)
    {
        builder.ToTable("secure_notes", "file_management");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id)
            .HasConversion<SecureNoteId.EfCoreValueConverter>();

        builder.Property(n => n.FamilyId)
            .HasConversion<FamilyId.EfCoreValueConverter>()
            .IsRequired();

        builder.Property(n => n.UserId)
            .HasConversion<UserId.EfCoreValueConverter>()
            .IsRequired();

        builder.Property(n => n.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.EncryptedTitle)
            .IsRequired();

        builder.Property(n => n.EncryptedContent)
            .IsRequired();

        builder.Property(n => n.Iv)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.Salt)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.Sentinel)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .IsRequired();

        builder.HasIndex(n => new { n.UserId, n.FamilyId });
        builder.HasIndex(n => new { n.UserId, n.FamilyId, n.Category });

        builder.Ignore(n => n.DomainEvents);
    }
}
