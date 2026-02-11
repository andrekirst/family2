using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Domain.Events;

public sealed record CalendarEventCancelledEvent(
    CalendarEventId CalendarEventId,
    FamilyId FamilyId,
    DateTime CancelledAt
) : DomainEvent;
