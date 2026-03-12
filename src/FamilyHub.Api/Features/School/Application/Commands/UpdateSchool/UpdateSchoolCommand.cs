using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateSchool;

public sealed record UpdateSchoolCommand(
    SchoolId SchoolId,
    SchoolName Name,
    FederalStateId FederalStateId,
    string City,
    string PostalCode
) : ICommand<Result<UpdateSchoolResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
