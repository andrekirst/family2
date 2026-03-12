using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateSchoolYear;

public sealed record UpdateSchoolYearCommand(
    SchoolYearId SchoolYearId,
    FederalStateId FederalStateId,
    int StartYear,
    int EndYear,
    DateOnly StartDate,
    DateOnly EndDate
) : ICommand<Result<UpdateSchoolYearResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
