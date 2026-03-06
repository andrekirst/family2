using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Events;

public sealed record FamilyMemberMarkedAsStudentEvent(
    StudentId StudentId,
    FamilyMemberId FamilyMemberId,
    FamilyId FamilyId,
    UserId MarkedByUserId,
    DateTime MarkedAt
) : DomainEvent;
