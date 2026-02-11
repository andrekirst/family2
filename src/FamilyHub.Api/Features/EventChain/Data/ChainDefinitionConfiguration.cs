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
                value => ChainDefinitionId.From(value));

        builder.Property(d => d.FamilyId)
            .HasConversion(
                familyId => familyId.Value,
                value => FamilyId.From(value))
            .IsRequired();

        builder.Property(d => d.Name)
            .HasConversion(
                name => name.Value,
                value => ChainName.From(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.Description);

        builder.Property(d => d.IsEnabled)
            .HasDefaultValue(true);

        builder.Property(d => d.IsTemplate)
            .HasDefaultValue(false);

        builder.Property(d => d.TemplateName)
            .HasMaxLength(100);

        builder.Property(d => d.TriggerEventType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.TriggerModule)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.TriggerDescription)
            .HasMaxLength(500);

        builder.Property(d => d.TriggerOutputSchema)
            .HasColumnType("jsonb");

        builder.Property(d => d.CreatedByUserId)
            .HasConversion(
                userId => userId.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(d => d.Version)
            .HasDefaultValue(1);

        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(d => d.UpdatedAt)
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
