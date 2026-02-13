using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Dashboard.Data;

public class DashboardWidgetConfiguration : IEntityTypeConfiguration<DashboardWidget>
{
    public void Configure(EntityTypeBuilder<DashboardWidget> builder)
    {
        builder.ToTable("dashboard_widgets", "dashboard");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasConversion(
                id => id.Value,
                value => DashboardWidgetId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(w => w.DashboardId)
            .HasConversion(
                id => id.Value,
                value => DashboardId.From(value))
            .IsRequired();

        builder.Property(w => w.WidgetType)
            .HasConversion(
                wt => wt.Value,
                value => WidgetTypeId.From(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.X).IsRequired();
        builder.Property(w => w.Y).IsRequired();
        builder.Property(w => w.Width).IsRequired();
        builder.Property(w => w.Height).IsRequired();
        builder.Property(w => w.SortOrder).IsRequired().HasDefaultValue(0);

        builder.Property(w => w.ConfigJson)
            .HasColumnType("jsonb");

        builder.Property(w => w.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(w => w.UpdatedAt).IsRequired().HasDefaultValueSql("NOW()");

        builder.HasIndex(w => w.DashboardId)
            .HasDatabaseName("ix_dashboard_widgets_dashboard_id");
    }
}
