using FamilyHub.Api.Features.BaseData.Domain.Entities;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.BaseData.Data;

public class FederalStateConfiguration : IEntityTypeConfiguration<FederalState>
{
    public void Configure(EntityTypeBuilder<FederalState> builder)
    {
        builder.ToTable("federal_states", "base_data");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(
                id => id.Value,
                value => FederalStateId.From(value))
            .ValueGeneratedNever();

        builder.Property(f => f.Name)
            .HasConversion(
                name => name.Value,
                value => FederalStateName.From(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.Iso3166Code)
            .HasColumnName("iso3166_code")
            .HasConversion(
                code => code.Value,
                value => Iso3166Code.From(value))
            .HasMaxLength(6)
            .IsRequired();

        builder.HasIndex(f => f.Iso3166Code).IsUnique();
    }
}
