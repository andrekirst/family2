using FamilyHub.Common.Application;
using FamilyHub.Api.Features.School.Application.Mappers;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Models;

namespace FamilyHub.Api.Features.School.Application.Queries.GetSchoolYears;

public sealed class GetSchoolYearsQueryHandler(
    ISchoolYearRepository schoolYearRepository)
    : IQueryHandler<GetSchoolYearsQuery, List<SchoolYearDto>>
{
    public async ValueTask<List<SchoolYearDto>> Handle(
        GetSchoolYearsQuery query,
        CancellationToken cancellationToken)
    {
        var schoolYears = await schoolYearRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return schoolYears.Select(sy => SchoolYearMapper.ToDto(sy, today)).ToList();
    }
}
