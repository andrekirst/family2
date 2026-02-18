using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Dashboard.Data;

public class DashboardLayoutConfiguration : IEntityTypeConfiguration<DashboardLayout>
{
    public void Configure(EntityTypeBuilder<DashboardLayout> builder)
    {
        builder.ToTable("dashboard_layouts", "dashboard");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasConversion(
                id => id.Value,
                value => DashboardId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(d => d.Name)
            .HasConversion(
                name => name.Value,
                value => DashboardLayoutName.From(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.UserId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                value => value == null ? null : (UserId?)UserId.From(value.Value));

        builder.Property(d => d.FamilyId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                value => value == null ? null : (FamilyId?)FamilyId.From(value.Value));

        builder.Property(d => d.IsShared)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasMany(d => d.Widgets)
            .WithOne()
            .HasForeignKey(w => w.DashboardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Widgets)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(d => d.UserId)
            .HasFilter("\"user_id\" IS NOT NULL")
            .HasDatabaseName("ix_dashboard_layouts_user_id");

        builder.HasIndex(d => d.FamilyId)
            .HasFilter("\"family_id\" IS NOT NULL")
            .HasDatabaseName("ix_dashboard_layouts_family_id");

        builder.Ignore(d => d.DomainEvents);
    }
}
