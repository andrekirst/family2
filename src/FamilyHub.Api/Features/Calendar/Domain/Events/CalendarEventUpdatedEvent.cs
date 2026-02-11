using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Domain.Events;

public sealed record CalendarEventUpdatedEvent(
    CalendarEventId CalendarEventId,
    FamilyId FamilyId,
    EventTitle Title,
    DateTime StartTime,
    DateTime EndTime,
    EventType Type,
    DateTime UpdatedAt
) : DomainEvent;
