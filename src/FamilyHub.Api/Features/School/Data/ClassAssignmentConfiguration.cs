using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.School.Data;

public class ClassAssignmentConfiguration : IEntityTypeConfiguration<ClassAssignment>
{
    public void Configure(EntityTypeBuilder<ClassAssignment> builder)
    {
        builder.ToTable("class_assignments", "school");

        builder.HasKey(ca => ca.Id);
        builder.Property(ca => ca.Id)
            .HasConversion(
                id => id.Value,
                value => ClassAssignmentId.From(value))
            .ValueGeneratedNever();

        builder.Property(ca => ca.StudentId)
            .HasConversion(
                id => id.Value,
                value => StudentId.From(value))
            .IsRequired();
        builder.HasIndex(ca => ca.StudentId);

        builder.Property(ca => ca.SchoolId)
            .HasConversion(
                id => id.Value,
                value => SchoolId.From(value))
            .IsRequired();

        builder.Property(ca => ca.SchoolYearId)
            .HasConversion(
                id => id.Value,
                value => SchoolYearId.From(value))
            .IsRequired();

        builder.Property(ca => ca.ClassName)
            .HasConversion(
                name => name.Value,
                value => ClassName.From(value))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ca => ca.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .IsRequired();
        builder.HasIndex(ca => ca.FamilyId);

        builder.Property(ca => ca.AssignedByUserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(ca => ca.AssignedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Unique: one assignment per student per school year
        builder.HasIndex(ca => new { ca.StudentId, ca.SchoolYearId })
            .IsUnique();
    }
}
