using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Family.Data;

/// <summary>
/// EF Core configuration for Family entity
/// Maps to PostgreSQL family schema with RLS policies applied via migration
/// </summary>
public class FamilyConfiguration : IEntityTypeConfiguration<Models.Family>
{
    public void Configure(EntityTypeBuilder<Models.Family> builder)
    {
        // Table mapping to family schema
        builder.ToTable("families", "family");

        // Primary key
        builder.HasKey(f => f.Id);

        // Name - required
        builder.Property(f => f.Name)
            .HasMaxLength(100)
            .IsRequired();

        // Owner - required
        builder.Property(f => f.OwnerId)
            .IsRequired();
        builder.HasIndex(f => f.OwnerId);

        // Metadata fields
        builder.Property(f => f.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(f => f.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Relationships
        builder.HasOne(f => f.Owner)
            .WithMany()
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Members)
            .WithOne(u => u.Family)
            .HasForeignKey(u => u.FamilyId);
    }
}
