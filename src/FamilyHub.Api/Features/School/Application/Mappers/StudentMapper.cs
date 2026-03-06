using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Models;

namespace FamilyHub.Api.Features.School.Application.Mappers;

public static class StudentMapper
{
    public static StudentDto ToDto(Student student)
    {
        return new StudentDto
        {
            Id = student.Id.Value,
            FamilyMemberId = student.FamilyMemberId.Value,
            FamilyId = student.FamilyId.Value,
            MarkedByUserId = student.MarkedByUserId.Value,
            MarkedAt = student.MarkedAt
        };
    }
}
