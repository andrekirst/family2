using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.EventChain.Data;

public class ChainEntityMappingConfiguration : IEntityTypeConfiguration<ChainEntityMapping>
{
    public void Configure(EntityTypeBuilder<ChainEntityMapping> builder)
    {
        builder.ToTable("chain_entity_mappings", "event_chain");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("id");

        builder.Property(m => m.ChainExecutionId)
            .HasConversion(
                id => id.Value,
                value => ChainExecutionId.From(value))
            .HasColumnName("chain_execution_id")
            .IsRequired();

        builder.Property(m => m.StepAlias)
            .HasColumnName("step_alias")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.EntityId)
            .HasColumnName("entity_id")
            .IsRequired();

        builder.Property(m => m.Module)
            .HasColumnName("module")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        // Indexes
        builder.HasIndex(m => m.ChainExecutionId)
            .HasDatabaseName("ix_chain_entity_mappings_execution_id");

        builder.HasIndex(m => new { m.EntityId, m.EntityType })
            .HasDatabaseName("ix_chain_entity_mappings_entity");

        builder.HasIndex(m => new { m.Module, m.EntityType })
            .HasDatabaseName("ix_chain_entity_mappings_module");
    }
}
