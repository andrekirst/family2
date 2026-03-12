using FamilyHub.Api.Features.School.Domain.Entities;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateClassAssignment;

public sealed record UpdateClassAssignmentResult(
    ClassAssignment UpdatedAssignment
);
