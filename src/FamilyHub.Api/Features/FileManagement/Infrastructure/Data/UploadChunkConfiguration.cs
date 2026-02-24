using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

/// <summary>
/// EF Core configuration for UploadChunk entity.
/// Temporary storage for chunked file uploads.
/// </summary>
public class UploadChunkConfiguration : IEntityTypeConfiguration<UploadChunk>
{
    public void Configure(EntityTypeBuilder<UploadChunk> builder)
    {
        builder.ToTable("upload_chunks", "file_management");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.UploadId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.ChunkIndex)
            .IsRequired();

        builder.Property(c => c.Data)
            .IsRequired();

        builder.Property(c => c.Size)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(c => new { c.UploadId, c.ChunkIndex })
            .IsUnique();
    }
}
