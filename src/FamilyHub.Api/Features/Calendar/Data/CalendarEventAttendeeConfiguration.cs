using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Calendar.Data;

public class CalendarEventAttendeeConfiguration : IEntityTypeConfiguration<CalendarEventAttendee>
{
    public void Configure(EntityTypeBuilder<CalendarEventAttendee> builder)
    {
        builder.ToTable("calendar_event_attendees", "calendar");

        builder.HasKey(a => new { a.CalendarEventId, a.UserId });

        builder.Property(a => a.CalendarEventId)
            .HasConversion(
                id => id.Value,
                value => CalendarEventId.From(value));

        builder.Property(a => a.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value));

        builder.HasOne(a => a.CalendarEvent)
            .WithMany(e => e.Attendees)
            .HasForeignKey(a => a.CalendarEventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
