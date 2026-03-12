using FamilyHub.Common.Application;
using FamilyHub.Api.Features.School.Application.Mappers;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Models;

namespace FamilyHub.Api.Features.School.Application.Queries.GetStudentClassAssignments;

public sealed class GetStudentClassAssignmentsQueryHandler(
    IClassAssignmentRepository classAssignmentRepository,
    ISchoolRepository schoolRepository,
    ISchoolYearRepository schoolYearRepository,
    TimeProvider timeProvider)
    : IQueryHandler<GetStudentClassAssignmentsQuery, List<ClassAssignmentDto>>
{
    public async ValueTask<List<ClassAssignmentDto>> Handle(
        GetStudentClassAssignmentsQuery query,
        CancellationToken cancellationToken)
    {
        var assignments = await classAssignmentRepository.GetByStudentIdAsync(query.StudentId, cancellationToken);
        if (assignments.Count == 0)
        {
            return [];
        }

        var schools = await schoolRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);
        var schoolNameMap = schools.ToDictionary(s => s.Id, s => s.Name.Value);

        var schoolYears = await schoolYearRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var currentSchoolYearIds = schoolYears
            .Where(sy => sy.IsCurrent(today))
            .Select(sy => sy.Id)
            .ToHashSet();

        return assignments.Select(a => ClassAssignmentMapper.ToDto(
            a,
            schoolNameMap.GetValueOrDefault(a.SchoolId, string.Empty),
            currentSchoolYearIds.Contains(a.SchoolYearId)
        )).ToList();
    }
}
