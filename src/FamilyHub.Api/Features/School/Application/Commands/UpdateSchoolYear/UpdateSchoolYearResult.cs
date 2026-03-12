using FamilyHub.Api.Features.School.Domain.Entities;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateSchoolYear;

public sealed record UpdateSchoolYearResult(
    SchoolYear UpdatedSchoolYear
);
