using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RevokeShareLink;

public sealed class RevokeShareLinkCommandHandler(
    IShareLinkRepository shareLinkRepository)
    : ICommandHandler<RevokeShareLinkCommand, Result<RevokeShareLinkResult>>
{
    public async ValueTask<Result<RevokeShareLinkResult>> Handle(
        RevokeShareLinkCommand command,
        CancellationToken cancellationToken)
    {
        var link = await shareLinkRepository.GetByIdAsync(command.ShareLinkId, cancellationToken);
        if (link is null)
        {
            return DomainError.NotFound(DomainErrorCodes.ShareLinkNotFound, "Share link not found");
        }

        if (link.FamilyId != command.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.ShareLinkNotFound, "Share link not found");
        }

        link.Revoke(command.UserId);

        return new RevokeShareLinkResult(true);
    }
}
