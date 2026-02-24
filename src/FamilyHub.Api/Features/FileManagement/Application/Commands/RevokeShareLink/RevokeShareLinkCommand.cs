using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RevokeShareLink;

public sealed record RevokeShareLinkCommand(
    ShareLinkId ShareLinkId,
    FamilyId FamilyId,
    UserId RevokedBy
) : ICommand<RevokeShareLinkResult>;
