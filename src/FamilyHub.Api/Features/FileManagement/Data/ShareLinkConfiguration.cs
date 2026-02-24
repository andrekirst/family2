using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Data;

public class ShareLinkConfiguration : IEntityTypeConfiguration<ShareLink>
{
    public void Configure(EntityTypeBuilder<ShareLink> builder)
    {
        builder.ToTable("share_links", "file_management");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => ShareLinkId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Token)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(s => s.ResourceType).IsRequired();
        builder.Property(s => s.ResourceId).IsRequired();

        builder.Property(s => s.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(s => s.ExpiresAt);
        builder.Property(s => s.PasswordHash).HasMaxLength(255);
        builder.Property(s => s.MaxDownloads);
        builder.Property(s => s.DownloadCount).IsRequired();
        builder.Property(s => s.IsRevoked).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasIndex(s => s.Token).IsUnique();
        builder.HasIndex(s => s.FamilyId);
        builder.HasIndex(s => s.ResourceId);
    }
}
