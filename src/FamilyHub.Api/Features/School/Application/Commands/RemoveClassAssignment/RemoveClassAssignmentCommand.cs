using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.RemoveClassAssignment;

public sealed record RemoveClassAssignmentCommand(
    ClassAssignmentId ClassAssignmentId
) : ICommand<Result<bool>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
