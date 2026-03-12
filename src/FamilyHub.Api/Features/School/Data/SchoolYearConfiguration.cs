using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.School.Data;

public class SchoolYearConfiguration : IEntityTypeConfiguration<SchoolYear>
{
    public void Configure(EntityTypeBuilder<SchoolYear> builder)
    {
        builder.ToTable("school_years", "school");

        builder.HasKey(sy => sy.Id);
        builder.Property(sy => sy.Id)
            .HasConversion(
                id => id.Value,
                value => SchoolYearId.From(value))
            .ValueGeneratedNever();

        builder.Property(sy => sy.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .IsRequired();
        builder.HasIndex(sy => sy.FamilyId);

        builder.Property(sy => sy.FederalStateId)
            .HasConversion(
                id => id.Value,
                value => FederalStateId.From(value))
            .IsRequired();

        builder.Property(sy => sy.StartYear)
            .IsRequired();

        builder.Property(sy => sy.EndYear)
            .IsRequired();

        builder.Property(sy => sy.StartDate)
            .IsRequired();

        builder.Property(sy => sy.EndDate)
            .IsRequired();

        builder.Property(sy => sy.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(sy => sy.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Unique: one school year per federal state per year range per family
        builder.HasIndex(sy => new { sy.FamilyId, sy.FederalStateId, sy.StartYear, sy.EndYear })
            .IsUnique();
    }
}
