using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RevokeShareLink;

public sealed record RevokeShareLinkCommand(
    ShareLinkId ShareLinkId
) : ICommand<RevokeShareLinkResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
