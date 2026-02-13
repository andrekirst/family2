using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Common.Infrastructure.Avatar.Data;

/// <summary>
/// EF Core configuration for StoredFile entity (binary file storage).
/// </summary>
public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("stored_files", "storage");

        builder.HasKey(f => f.StorageKey);
        builder.Property(f => f.StorageKey)
            .HasMaxLength(255);

        builder.Property(f => f.Data)
            .IsRequired();

        builder.Property(f => f.MimeType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
    }
}
