using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.EventChain.Data;

public class ChainExecutionConfiguration : IEntityTypeConfiguration<ChainExecution>
{
    public void Configure(EntityTypeBuilder<ChainExecution> builder)
    {
        builder.ToTable("chain_executions", "event_chain");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => ChainExecutionId.From(value))
            .HasColumnName("id");

        builder.Property(e => e.ChainDefinitionId)
            .HasConversion(
                id => id.Value,
                value => ChainDefinitionId.From(value))
            .HasColumnName("chain_definition_id")
            .IsRequired();

        builder.Property(e => e.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(e => e.CorrelationId)
            .HasColumnName("correlation_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasConversion<string>()
            .HasDefaultValue(ChainExecutionStatus.Pending);

        builder.Property(e => e.TriggerEventType)
            .HasColumnName("trigger_event_type")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.TriggerEventId)
            .HasColumnName("trigger_event_id")
            .IsRequired();

        builder.Property(e => e.TriggerPayload)
            .HasColumnName("trigger_payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.Context)
            .HasColumnName("context")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(e => e.CurrentStepIndex)
            .HasColumnName("current_step_index")
            .HasDefaultValue(0);

        builder.Property(e => e.StartedAt)
            .HasColumnName("started_at")
            .HasDefaultValueSql("now()");

        builder.Property(e => e.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(e => e.FailedAt)
            .HasColumnName("failed_at");

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message");

        // Indexes
        builder.HasIndex(e => e.FamilyId)
            .HasDatabaseName("ix_chain_executions_family_id");

        builder.HasIndex(e => e.ChainDefinitionId)
            .HasDatabaseName("ix_chain_executions_definition_id");

        builder.HasIndex(e => e.CorrelationId)
            .HasDatabaseName("ix_chain_executions_correlation_id");

        builder.HasIndex(e => e.Status)
            .HasFilter("status IN ('Pending', 'Running', 'Compensating')")
            .HasDatabaseName("ix_chain_executions_status");

        // Navigation
        builder.HasOne(e => e.ChainDefinition)
            .WithMany()
            .HasForeignKey(e => e.ChainDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.StepExecutions)
            .WithOne()
            .HasForeignKey(s => s.ChainExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.StepExecutions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(e => e.DomainEvents);
    }
}
