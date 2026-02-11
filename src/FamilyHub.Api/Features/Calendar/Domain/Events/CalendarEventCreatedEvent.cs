using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

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
