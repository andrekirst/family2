using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Models;

namespace FamilyHub.Api.Features.School.Application.Queries.GetStudents;

public sealed class GetStudentsQueryHandler(
    IStudentRepository studentRepository,
    IFamilyMemberRepository familyMemberRepository)
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

        return students.Select(s => new StudentDto
        {
            Id = s.Id.Value,
            FamilyMemberId = s.FamilyMemberId.Value,
            MemberName = memberNameMap.GetValueOrDefault(s.FamilyMemberId, string.Empty),
            FamilyId = s.FamilyId.Value,
            MarkedByUserId = s.MarkedByUserId.Value,
            MarkedAt = s.MarkedAt
        }).ToList();
    }
}
