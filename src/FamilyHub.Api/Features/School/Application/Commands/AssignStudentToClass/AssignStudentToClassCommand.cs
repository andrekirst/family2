using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.AssignStudentToClass;

public sealed record AssignStudentToClassCommand(
    StudentId StudentId,
    SchoolId SchoolId,
    SchoolYearId SchoolYearId,
    ClassName ClassName
) : ICommand<Result<AssignStudentToClassResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
