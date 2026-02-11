using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.EventChain.Data;

public class ChainDefinitionStepConfiguration : IEntityTypeConfiguration<ChainDefinitionStep>
{
    public void Configure(EntityTypeBuilder<ChainDefinitionStep> builder)
    {
        builder.ToTable("chain_definition_steps", "event_chain");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.ChainDefinitionId)
            .HasConversion(
                id => id.Value,
                value => ChainDefinitionId.From(value))
            .HasColumnName("chain_definition_id")
            .IsRequired();

        builder.Property(s => s.Alias)
            .HasConversion(
                alias => alias.Value,
                value => StepAlias.From(value))
            .HasColumnName("alias")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.ActionType)
            .HasColumnName("action_type")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.ActionVersion)
            .HasConversion(
                v => v.Value,
                value => ActionVersion.From(value))
            .HasColumnName("action_version")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Module)
            .HasColumnName("module")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.InputMappings)
            .HasColumnName("input_mappings")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(s => s.ConditionExpression)
            .HasColumnName("condition_expression");

        builder.Property(s => s.IsCompensatable)
            .HasColumnName("is_compensatable")
            .HasDefaultValue(false);

        builder.Property(s => s.CompensationActionType)
            .HasColumnName("compensation_action_type")
            .HasMaxLength(500);

        builder.Property(s => s.StepOrder)
            .HasColumnName("step_order")
            .IsRequired();

        // Unique constraints
        builder.HasIndex(s => new { s.ChainDefinitionId, s.Alias })
            .IsUnique()
            .HasDatabaseName("uq_chain_step_alias");

        builder.HasIndex(s => new { s.ChainDefinitionId, s.StepOrder })
            .IsUnique()
            .HasDatabaseName("uq_chain_step_order");

        builder.HasIndex(s => s.ChainDefinitionId)
            .HasDatabaseName("ix_chain_definition_steps_definition_id");
    }
}
