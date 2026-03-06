using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Models;

namespace FamilyHub.Api.Features.School.Application.Queries.GetStudents;

public sealed record GetStudentsQuery(
    FamilyId FamilyId
) : IReadOnlyQuery<List<StudentDto>>;
