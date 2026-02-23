using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class ProcessingLogEntryConfiguration : IEntityTypeConfiguration<ProcessingLogEntry>
{
    public void Configure(EntityTypeBuilder<ProcessingLogEntry> builder)
    {
        builder.ToTable("processing_log", "file_management");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => ProcessingLogEntryId.From(value));

        builder.Property(e => e.FileId)
            .HasConversion(id => id.Value, value => FileId.From(value))
            .IsRequired();

        builder.Property(e => e.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.MatchedRuleId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? OrganizationRuleId.From(value.Value) : null);

        builder.Property(e => e.MatchedRuleName)
            .HasMaxLength(200);

        builder.Property(e => e.DestinationFolderId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? FolderId.From(value.Value) : null);

        builder.Property(e => e.AppliedTagNames)
            .HasMaxLength(1000);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(e => e.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .IsRequired();

        builder.Property(e => e.ProcessedAt).IsRequired();

        builder.HasIndex(e => e.FamilyId);
        builder.HasIndex(e => e.ProcessedAt);
    }
}
