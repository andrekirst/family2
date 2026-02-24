using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.AccessShareLink;

public sealed class AccessShareLinkCommandHandler(
    IShareLinkRepository shareLinkRepository,
    IShareLinkAccessLogRepository accessLogRepository)
    : ICommandHandler<AccessShareLinkCommand, AccessShareLinkResult>
{
    public async ValueTask<AccessShareLinkResult> Handle(
        AccessShareLinkCommand command,
        CancellationToken cancellationToken)
    {
        var link = await shareLinkRepository.GetByTokenAsync(command.Token, cancellationToken)
            ?? throw new DomainException("Share link not found", DomainErrorCodes.ShareLinkNotFound);

        if (link.IsRevoked)
            throw new DomainException("Share link has been revoked", DomainErrorCodes.ShareLinkRevoked);

        if (link.IsExpired)
            throw new DomainException("Share link has expired", DomainErrorCodes.ShareLinkExpired);

        if (link.IsDownloadLimitReached)
            throw new DomainException("Download limit reached", DomainErrorCodes.ShareLinkDownloadLimitReached);

        if (link.HasPassword)
        {
            if (string.IsNullOrEmpty(command.Password))
                throw new DomainException("Password required", DomainErrorCodes.ShareLinkPasswordRequired);

            if (!BCrypt.Net.BCrypt.Verify(command.Password, link.PasswordHash))
                throw new DomainException("Incorrect password", DomainErrorCodes.ShareLinkPasswordIncorrect);
        }

        if (command.Action == ShareAccessAction.Download)
            link.IncrementDownloadCount();

        var accessLog = ShareLinkAccessLog.Create(
            link.Id,
            command.IpAddress,
            command.UserAgent,
            command.Action);

        await accessLogRepository.AddAsync(accessLog, cancellationToken);

        return new AccessShareLinkResult(
            true,
            link.ResourceType.ToString(),
            link.ResourceId,
            link.FamilyId.Value);
    }
}
