using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Domain.Events;

public sealed record CalendarEventCreatedEvent(
    CalendarEventId CalendarEventId,
    FamilyId FamilyId,
    UserId CreatedBy,
    EventTitle Title,
    DateTime StartTime,
    DateTime EndTime,
    EventType Type,
    DateTime CreatedAt
) : DomainEvent;
