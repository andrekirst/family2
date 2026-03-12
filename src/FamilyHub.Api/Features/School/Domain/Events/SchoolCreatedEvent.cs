using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Events;

public sealed record SchoolCreatedEvent(
    SchoolId SchoolId,
    SchoolName SchoolName,
    FamilyId FamilyId
) : DomainEvent;
