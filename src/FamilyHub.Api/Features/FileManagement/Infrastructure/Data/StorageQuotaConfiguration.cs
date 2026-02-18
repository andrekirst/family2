using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

/// <summary>
/// EF Core configuration for StorageQuota entity.
/// Tracks per-family storage usage and limits.
/// </summary>
public class StorageQuotaConfiguration : IEntityTypeConfiguration<StorageQuota>
{
    public void Configure(EntityTypeBuilder<StorageQuota> builder)
    {
        builder.ToTable("storage_quotas", "file_management");

        builder.HasKey(q => q.FamilyId);

        builder.Property(q => q.UsedBytes)
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(q => q.MaxBytes)
            .IsRequired();

        builder.Property(q => q.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
    }
}
