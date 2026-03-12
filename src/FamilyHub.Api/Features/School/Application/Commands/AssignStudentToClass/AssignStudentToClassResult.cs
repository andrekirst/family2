using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.AssignStudentToClass;

public sealed record AssignStudentToClassResult(
    ClassAssignmentId ClassAssignmentId,
    ClassAssignment CreatedAssignment
);
