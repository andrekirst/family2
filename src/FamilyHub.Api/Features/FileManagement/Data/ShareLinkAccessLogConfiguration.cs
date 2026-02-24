using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Data;

public class ShareLinkAccessLogConfiguration : IEntityTypeConfiguration<ShareLinkAccessLog>
{
    public void Configure(EntityTypeBuilder<ShareLinkAccessLog> builder)
    {
        builder.ToTable("share_link_access_log", "file_management");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasConversion(id => id.Value, value => ShareLinkAccessLogId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(l => l.ShareLinkId)
            .HasConversion(id => id.Value, value => ShareLinkId.From(value))
            .IsRequired();

        builder.Property(l => l.IpAddress)
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(l => l.UserAgent).HasMaxLength(512);
        builder.Property(l => l.Action).IsRequired();
        builder.Property(l => l.AccessedAt).IsRequired();

        builder.HasIndex(l => l.ShareLinkId);
    }
}
