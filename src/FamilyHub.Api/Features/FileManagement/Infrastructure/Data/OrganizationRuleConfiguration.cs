using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class OrganizationRuleConfiguration : IEntityTypeConfiguration<OrganizationRule>
{
    public void Configure(EntityTypeBuilder<OrganizationRule> builder)
    {
        builder.ToTable("organization_rules", "file_management");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(r => r.ConditionsJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(r => r.ConditionLogic)
            .IsRequired();

        builder.Property(r => r.ActionType)
            .IsRequired();

        builder.Property(r => r.ActionsJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(r => r.Priority)
            .IsRequired();

        builder.Property(r => r.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.FamilyId);
        builder.HasIndex(r => new { r.FamilyId, r.Priority });
    }
}
