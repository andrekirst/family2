using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RevokeShareLink;

public sealed class RevokeShareLinkCommandHandler(
    IShareLinkRepository shareLinkRepository)
    : ICommandHandler<RevokeShareLinkCommand, RevokeShareLinkResult>
{
    public async ValueTask<RevokeShareLinkResult> Handle(
        RevokeShareLinkCommand command,
        CancellationToken cancellationToken)
    {
        var link = await shareLinkRepository.GetByIdAsync(command.ShareLinkId, cancellationToken)
            ?? throw new DomainException("Share link not found", DomainErrorCodes.ShareLinkNotFound);

        if (link.FamilyId != command.FamilyId)
            throw new DomainException("Share link not found", DomainErrorCodes.ShareLinkNotFound);

        link.Revoke(command.RevokedBy);

        return new RevokeShareLinkResult(true);
    }
}
