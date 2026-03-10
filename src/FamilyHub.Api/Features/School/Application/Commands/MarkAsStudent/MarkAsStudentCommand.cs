using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;

public sealed record MarkAsStudentCommand(
    FamilyMemberId FamilyMemberId
) : ICommand<Result<MarkAsStudentResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
