using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.CreateSchoolYear;

public sealed record CreateSchoolYearResult(
    SchoolYearId SchoolYearId,
    SchoolYear CreatedSchoolYear
);
