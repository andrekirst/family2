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
        builder.Property(s => s.Id);

        builder.Property(s => s.ChainExecutionId)
            .HasConversion(
                id => id.Value,
                value => ChainExecutionId.From(value))
            .IsRequired();

        builder.Property(s => s.StepAlias)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.StepName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.ActionType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasMaxLength(30)
            .HasConversion<string>()
            .HasDefaultValue(StepExecutionStatus.Pending);

        builder.Property(s => s.InputPayload)
            .HasColumnType("jsonb");

        builder.Property(s => s.OutputPayload)
            .HasColumnType("jsonb");

        builder.Property(s => s.ErrorMessage);

        builder.Property(s => s.RetryCount)
            .HasDefaultValue(0);

        builder.Property(s => s.MaxRetries)
            .HasDefaultValue(3);

        builder.Property(s => s.StepOrder)
            .IsRequired();

        builder.Property(s => s.ScheduledAt);

        builder.Property(s => s.PickedUpAt);

        builder.Property(s => s.StartedAt);

        builder.Property(s => s.CompletedAt);

        builder.Property(s => s.CompensatedAt);

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
