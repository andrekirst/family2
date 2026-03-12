using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.DeleteSchoolYear;

public sealed record DeleteSchoolYearCommand(
    SchoolYearId SchoolYearId
) : ICommand<Result<bool>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
