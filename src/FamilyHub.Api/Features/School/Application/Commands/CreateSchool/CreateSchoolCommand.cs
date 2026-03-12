using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.CreateSchool;

public sealed record CreateSchoolCommand(
    SchoolName Name,
    FederalStateId FederalStateId,
    string City,
    string PostalCode
) : ICommand<Result<CreateSchoolResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
