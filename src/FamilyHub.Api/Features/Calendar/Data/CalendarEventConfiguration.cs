using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Calendar.Data;

public class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ToTable("calendar_events", "calendar");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => CalendarEventId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(e => e.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(e => e.Title)
            .HasConversion(
                title => title.Value,
                value => EventTitle.From(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Location)
            .HasMaxLength(500);

        builder.Property(e => e.StartTime).IsRequired();
        builder.Property(e => e.EndTime).IsRequired();
        builder.Property(e => e.IsAllDay).IsRequired();

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.IsCancelled).IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Indexes for efficient month queries
        builder.HasIndex(e => new { e.FamilyId, e.StartTime });
        builder.HasIndex(e => new { e.CreatedBy, e.StartTime });

        // Ignore domain events — not persisted
        builder.Ignore(e => e.DomainEvents);
    }
}
