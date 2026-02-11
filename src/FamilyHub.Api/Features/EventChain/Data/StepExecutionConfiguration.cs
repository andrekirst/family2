using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.EventChain.Data;

public class StepExecutionConfiguration : IEntityTypeConfiguration<StepExecution>
{
    public void Configure(EntityTypeBuilder<StepExecution> builder)
    {
        builder.ToTable("step_executions", "event_chain");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.ChainExecutionId)
            .HasConversion(
                id => id.Value,
                value => ChainExecutionId.From(value))
            .HasColumnName("chain_execution_id")
            .IsRequired();

        builder.Property(s => s.StepAlias)
            .HasColumnName("step_alias")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.StepName)
            .HasColumnName("step_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.ActionType)
            .HasColumnName("action_type")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasConversion<string>()
            .HasDefaultValue(StepExecutionStatus.Pending);

        builder.Property(s => s.InputPayload)
            .HasColumnName("input_payload")
            .HasColumnType("jsonb");

        builder.Property(s => s.OutputPayload)
            .HasColumnName("output_payload")
            .HasColumnType("jsonb");

        builder.Property(s => s.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(s => s.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0);

        builder.Property(s => s.MaxRetries)
            .HasColumnName("max_retries")
            .HasDefaultValue(3);

        builder.Property(s => s.StepOrder)
            .HasColumnName("step_order")
            .IsRequired();

        builder.Property(s => s.ScheduledAt)
            .HasColumnName("scheduled_at");

        builder.Property(s => s.PickedUpAt)
            .HasColumnName("picked_up_at");

        builder.Property(s => s.StartedAt)
            .HasColumnName("started_at");

        builder.Property(s => s.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(s => s.CompensatedAt)
            .HasColumnName("compensated_at");

        // Unique constraint
        builder.HasIndex(s => new { s.ChainExecutionId, s.StepAlias })
            .IsUnique()
            .HasDatabaseName("uq_step_execution_alias");

        // Indexes
        builder.HasIndex(s => s.ChainExecutionId)
            .HasDatabaseName("ix_step_executions_chain_execution_id");

        builder.HasIndex(s => s.ScheduledAt)
            .HasFilter("status = 'Pending' AND scheduled_at IS NOT NULL AND picked_up_at IS NULL")
            .HasDatabaseName("ix_step_executions_scheduled");

        builder.HasIndex(s => s.Status)
            .HasFilter("status IN ('Pending', 'Running')")
            .HasDatabaseName("ix_step_executions_status");
    }
}
