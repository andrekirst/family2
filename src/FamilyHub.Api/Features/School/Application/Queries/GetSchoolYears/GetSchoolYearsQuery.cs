using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Models;

namespace FamilyHub.Api.Features.School.Application.Queries.GetSchoolYears;

public sealed record GetSchoolYearsQuery : IReadOnlyQuery<List<SchoolYearDto>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
