using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Models;

namespace FamilyHub.Api.Features.School.Application.Queries.GetStudents;

public sealed class GetStudentsQueryHandler(
    IStudentRepository studentRepository,
    IFamilyMemberRepository familyMemberRepository,
    IClassAssignmentRepository classAssignmentRepository,
    ISchoolRepository schoolRepository,
    ISchoolYearRepository schoolYearRepository)
    : IQueryHandler<GetStudentsQuery, List<StudentDto>>
{
    public async ValueTask<List<StudentDto>> Handle(
        GetStudentsQuery query,
        CancellationToken cancellationToken)
    {
        var students = await studentRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);
        if (students.Count == 0)
        {
            return [];
        }

        var members = await familyMemberRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);
        var memberNameMap = members.ToDictionary(m => m.Id, m => m.User?.Name.Value ?? string.Empty);

        // Load class assignments and related data for current school/class enrichment
        var assignments = await classAssignmentRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);
        var schools = await schoolRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);
        var schoolNameMap = schools.ToDictionary(s => s.Id, s => s.Name.Value);
        var schoolYears = await schoolYearRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentSchoolYearIds = schoolYears
            .Where(sy => sy.IsCurrent(today))
            .Select(sy => sy.Id)
            .ToHashSet();

        // Build a lookup of current assignments per student
        var currentAssignmentByStudent = assignments
            .Where(a => currentSchoolYearIds.Contains(a.SchoolYearId))
            .GroupBy(a => a.StudentId)
            .ToDictionary(g => g.Key, g => g.First());

        return students.Select(s =>
        {
            var dto = new StudentDto
            {
                Id = s.Id.Value,
                FamilyMemberId = s.FamilyMemberId.Value,
                MemberName = memberNameMap.GetValueOrDefault(s.FamilyMemberId, string.Empty),
                FamilyId = s.FamilyId.Value,
                MarkedByUserId = s.MarkedByUserId.Value,
                MarkedAt = s.MarkedAt
            };

            if (currentAssignmentByStudent.TryGetValue(s.Id, out var currentAssignment))
            {
                dto.CurrentSchoolName = schoolNameMap.GetValueOrDefault(currentAssignment.SchoolId);
                dto.CurrentClassName = currentAssignment.ClassName.Value;
            }

            return dto;
        }).ToList();
    }
}
