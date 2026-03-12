using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.CreateSchool;

public sealed record CreateSchoolResult(
    SchoolId SchoolId,
    Domain.Entities.School CreatedSchool
);
