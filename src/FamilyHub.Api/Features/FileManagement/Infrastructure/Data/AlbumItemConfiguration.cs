using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class AlbumItemConfiguration : IEntityTypeConfiguration<AlbumItem>
{
    public void Configure(EntityTypeBuilder<AlbumItem> builder)
    {
        builder.ToTable("album_items", "file_management");

        builder.HasKey(ai => new { ai.AlbumId, ai.FileId });

        builder.HasIndex(ai => ai.AlbumId);
        builder.HasIndex(ai => ai.FileId);
    }
}
