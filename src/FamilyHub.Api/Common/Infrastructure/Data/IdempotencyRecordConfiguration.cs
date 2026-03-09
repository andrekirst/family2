using FamilyHub.Api.Common.Infrastructure.Behaviors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Common.Infrastructure.Data;

public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_keys");

        builder.HasKey(r => r.KeyHash);

        builder.Property(r => r.KeyHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(r => r.ResultJson)
            .HasColumnType("jsonb");

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .IsRequired();

        builder.HasIndex(r => r.ExpiresAt)
            .HasDatabaseName("ix_idempotency_keys_expires_at");
    }
}
