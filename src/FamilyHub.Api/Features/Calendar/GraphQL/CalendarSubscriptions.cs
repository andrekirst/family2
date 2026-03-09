using FamilyHub.Api.Features.Calendar.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Calendar.GraphQL;

/// <summary>
/// GraphQL subscriptions for real-time calendar event updates.
/// Topic is family-scoped: "CalendarEventChanged_{familyId}" ensures clients only receive
/// calendar events for their own family.
/// </summary>
[ExtendObjectType("Subscription")]
public class CalendarSubscriptions
{
    /// <summary>
    /// Subscribe to calendar event changes (create, update, cancel) for a family.
    /// </summary>
    [Authorize]
    [Subscribe]
    [Topic("CalendarEventChanged_{familyId}")]
    public CalendarEventDto CalendarEventChanged(
        Guid familyId,
        [EventMessage] CalendarEventDto calendarEvent)
        => calendarEvent;
}
