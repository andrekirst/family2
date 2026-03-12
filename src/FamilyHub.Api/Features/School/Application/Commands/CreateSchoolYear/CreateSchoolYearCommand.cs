using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.CreateSchoolYear;

public sealed record CreateSchoolYearCommand(
    FederalStateId FederalStateId,
    int StartYear,
    int EndYear,
    DateOnly StartDate,
    DateOnly EndDate
) : ICommand<Result<CreateSchoolYearResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
