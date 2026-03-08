using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;

public sealed record MarkAsStudentResult(
    StudentId StudentId,
    Student CreatedStudent
);
