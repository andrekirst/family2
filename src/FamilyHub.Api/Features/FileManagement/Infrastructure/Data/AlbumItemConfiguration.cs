using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class AlbumItemConfiguration : IEntityTypeConfiguration<AlbumItem>
{
    public void Configure(EntityTypeBuilder<AlbumItem> builder)
    {
        builder.ToTable("album_items", "file_management");

        builder.HasKey(ai => new { ai.AlbumId, ai.FileId });

        builder.Property(ai => ai.AlbumId)
            .HasConversion(
                id => id.Value,
                value => AlbumId.From(value));

        builder.Property(ai => ai.FileId)
            .HasConversion(
                id => id.Value,
                value => FileId.From(value));

        builder.Property(ai => ai.AddedBy)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value));

        builder.HasIndex(ai => ai.AlbumId);
        builder.HasIndex(ai => ai.FileId);
    }
}
