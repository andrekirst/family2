using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.EventChain.Data;

public class ChainScheduledJobConfiguration : IEntityTypeConfiguration<ChainScheduledJob>
{
    public void Configure(EntityTypeBuilder<ChainScheduledJob> builder)
    {
        builder.ToTable("chain_scheduled_jobs", "event_chain");

        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id)
            .HasColumnName("id");

        builder.Property(j => j.StepExecutionId)
            .HasColumnName("step_execution_id")
            .IsRequired();

        builder.Property(j => j.ChainExecutionId)
            .HasConversion(
                id => id.Value,
                value => ChainExecutionId.From(value))
            .HasColumnName("chain_execution_id")
            .IsRequired();

        builder.Property(j => j.ScheduledAt)
            .HasColumnName("scheduled_at")
            .IsRequired();

        builder.Property(j => j.PickedUpAt)
            .HasColumnName("picked_up_at");

        builder.Property(j => j.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(j => j.FailedAt)
            .HasColumnName("failed_at");

        builder.Property(j => j.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(j => j.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0);

        builder.Property(j => j.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        // Indexes (critical for scheduler performance)
        builder.HasIndex(j => j.ScheduledAt)
            .HasFilter("picked_up_at IS NULL AND completed_at IS NULL AND failed_at IS NULL")
            .HasDatabaseName("ix_chain_scheduled_jobs_ready");

        builder.HasIndex(j => j.PickedUpAt)
            .HasFilter("completed_at IS NULL AND failed_at IS NULL AND picked_up_at IS NOT NULL")
            .HasDatabaseName("ix_chain_scheduled_jobs_stale");

        // Navigation
        builder.HasOne(j => j.StepExecution)
            .WithMany()
            .HasForeignKey(j => j.StepExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(j => j.ChainExecution)
            .WithMany()
            .HasForeignKey(j => j.ChainExecutionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
