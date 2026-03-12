using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Models;

namespace FamilyHub.Api.Features.School.Application.Mappers;

public static class ClassAssignmentMapper
{
    public static ClassAssignmentDto ToDto(
        ClassAssignment assignment,
        string schoolName,
        bool isCurrent)
    {
        return new ClassAssignmentDto
        {
            Id = assignment.Id.Value,
            StudentId = assignment.StudentId.Value,
            SchoolId = assignment.SchoolId.Value,
            SchoolName = schoolName,
            SchoolYearId = assignment.SchoolYearId.Value,
            ClassName = assignment.ClassName.Value,
            FamilyId = assignment.FamilyId.Value,
            AssignedByUserId = assignment.AssignedByUserId.Value,
            AssignedAt = assignment.AssignedAt,
            IsCurrent = isCurrent
        };
    }
}
