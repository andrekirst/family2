using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.School.Data;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students", "school");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => StudentId.From(value))
            .ValueGeneratedNever();

        builder.Property(s => s.FamilyMemberId)
            .HasConversion(
                id => id.Value,
                value => FamilyMemberId.From(value))
            .IsRequired();
        builder.HasIndex(s => s.FamilyMemberId).IsUnique();

        builder.Property(s => s.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .IsRequired();
        builder.HasIndex(s => s.FamilyId);

        builder.Property(s => s.MarkedByUserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(s => s.MarkedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
    }
}
