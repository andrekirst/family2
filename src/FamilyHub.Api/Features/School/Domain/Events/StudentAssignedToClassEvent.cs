using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Events;

public sealed record StudentAssignedToClassEvent(
    ClassAssignmentId ClassAssignmentId,
    StudentId StudentId,
    SchoolId SchoolId,
    SchoolYearId SchoolYearId,
    ClassName ClassName,
    FamilyId FamilyId,
    UserId AssignedByUserId,
    DateTime AssignedAt
) : DomainEvent;
