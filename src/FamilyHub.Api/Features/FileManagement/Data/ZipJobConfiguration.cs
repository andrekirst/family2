using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Data;

public sealed class ZipJobConfiguration : IEntityTypeConfiguration<ZipJob>
{
    public void Configure(EntityTypeBuilder<ZipJob> builder)
    {
        builder.ToTable("zip_jobs", "file_management");

        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id)
            .HasConversion<ZipJobId.EfCoreValueConverter>();

        builder.Property(j => j.FamilyId)
            .HasConversion<FamilyId.EfCoreValueConverter>()
            .IsRequired();

        builder.Property(j => j.InitiatedBy)
            .HasConversion<UserId.EfCoreValueConverter>()
            .IsRequired();

        builder.Property(j => j.FileIds)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(j => j.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(j => j.Progress)
            .IsRequired();

        builder.Property(j => j.ZipStorageKey)
            .HasConversion<StorageKey.EfCoreValueConverter>();

        builder.Property(j => j.ZipSize);

        builder.Property(j => j.ErrorMessage);

        builder.Property(j => j.CreatedAt).IsRequired();
        builder.Property(j => j.CompletedAt);
        builder.Property(j => j.ExpiresAt).IsRequired();

        builder.HasIndex(j => j.FamilyId);
        builder.HasIndex(j => j.ExpiresAt);

        builder.Ignore(j => j.DomainEvents);
    }
}
