using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.EventChain.Data;

public class ChainDefinitionConfiguration : IEntityTypeConfiguration<ChainDefinition>
{
    public void Configure(EntityTypeBuilder<ChainDefinition> builder)
    {
        builder.ToTable("chain_definitions", "event_chain");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasConversion(
                id => id.Value,
                value => ChainDefinitionId.From(value))
            .HasColumnName("id");

        builder.Property(d => d.FamilyId)
            .HasConversion(
                familyId => familyId.Value,
                value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(d => d.Name)
            .HasConversion(
                name => name.Value,
                value => ChainName.From(value))
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.Description)
            .HasColumnName("description");

        builder.Property(d => d.IsEnabled)
            .HasColumnName("is_enabled")
            .HasDefaultValue(true);

        builder.Property(d => d.IsTemplate)
            .HasColumnName("is_template")
            .HasDefaultValue(false);

        builder.Property(d => d.TemplateName)
            .HasColumnName("template_name")
            .HasMaxLength(100);

        builder.Property(d => d.TriggerEventType)
            .HasColumnName("trigger_event_type")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.TriggerModule)
            .HasColumnName("trigger_module")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.TriggerDescription)
            .HasColumnName("trigger_description")
            .HasMaxLength(500);

        builder.Property(d => d.TriggerOutputSchema)
            .HasColumnName("trigger_output_schema")
            .HasColumnType("jsonb");

        builder.Property(d => d.CreatedByUserId)
            .HasConversion(
                userId => userId.Value,
                value => UserId.From(value))
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(d => d.Version)
            .HasColumnName("version")
            .HasDefaultValue(1);

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        // Indexes
        builder.HasIndex(d => d.FamilyId)
            .HasDatabaseName("ix_chain_definitions_family_id");

        builder.HasIndex(d => d.TriggerEventType)
            .HasFilter("is_enabled = true")
            .HasDatabaseName("ix_chain_definitions_trigger_event_type");

        builder.HasIndex(d => d.TemplateName)
            .HasFilter("is_template = true")
            .HasDatabaseName("ix_chain_definitions_template_name");

        // Navigation
        builder.HasMany(d => d.Steps)
            .WithOne()
            .HasForeignKey(s => s.ChainDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Steps)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(d => d.DomainEvents);
    }
}
