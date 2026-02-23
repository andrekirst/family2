using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class FilePermissionConfiguration : IEntityTypeConfiguration<FilePermission>
{
    public void Configure(EntityTypeBuilder<FilePermission> builder)
    {
        builder.ToTable("file_permissions", "file_management");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => FilePermissionId.From(value));

        builder.Property(p => p.ResourceType)
            .IsRequired();

        builder.Property(p => p.ResourceId)
            .IsRequired();

        builder.Property(p => p.MemberId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value));

        builder.Property(p => p.PermissionLevel)
            .IsRequired();

        builder.Property(p => p.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value));

        builder.Property(p => p.GrantedBy)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value));

        builder.HasIndex(p => new { p.ResourceType, p.ResourceId });
        builder.HasIndex(p => new { p.MemberId, p.FamilyId });
        builder.HasIndex(p => new { p.MemberId, p.ResourceType, p.ResourceId }).IsUnique();
    }
}
