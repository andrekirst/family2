using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.School.Data;

public class SchoolConfiguration : IEntityTypeConfiguration<Domain.Entities.School>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.School> builder)
    {
        builder.ToTable("schools", "school");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => SchoolId.From(value))
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .HasConversion(
                name => name.Value,
                value => SchoolName.From(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .IsRequired();
        builder.HasIndex(s => s.FamilyId);

        builder.Property(s => s.FederalStateId)
            .HasConversion(
                id => id.Value,
                value => FederalStateId.From(value))
            .IsRequired();

        builder.Property(s => s.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.PostalCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(s => s.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
    }
}
