using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateClassAssignment;

public sealed record UpdateClassAssignmentCommand(
    ClassAssignmentId ClassAssignmentId,
    SchoolId SchoolId,
    SchoolYearId SchoolYearId,
    ClassName ClassName
) : ICommand<Result<UpdateClassAssignmentResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
