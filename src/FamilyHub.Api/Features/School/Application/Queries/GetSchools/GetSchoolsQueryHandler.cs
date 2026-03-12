using FamilyHub.Common.Application;
using FamilyHub.Api.Features.School.Application.Mappers;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Models;

namespace FamilyHub.Api.Features.School.Application.Queries.GetSchools;

public sealed class GetSchoolsQueryHandler(
    ISchoolRepository schoolRepository)
    : IQueryHandler<GetSchoolsQuery, List<SchoolDto>>
{
    public async ValueTask<List<SchoolDto>> Handle(
        GetSchoolsQuery query,
        CancellationToken cancellationToken)
    {
        var schools = await schoolRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);

        return schools.Select(SchoolMapper.ToDto).ToList();
    }
}
