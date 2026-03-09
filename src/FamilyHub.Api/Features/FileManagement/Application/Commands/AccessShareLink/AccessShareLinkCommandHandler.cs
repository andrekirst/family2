using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.AccessShareLink;

public sealed class AccessShareLinkCommandHandler(
    IShareLinkRepository shareLinkRepository,
    IShareLinkAccessLogRepository accessLogRepository,
    TimeProvider timeProvider)
    : ICommandHandler<AccessShareLinkCommand, Result<AccessShareLinkResult>>
{
    public async ValueTask<Result<AccessShareLinkResult>> Handle(
        AccessShareLinkCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var link = await shareLinkRepository.GetByTokenAsync(command.Token, cancellationToken);
        if (link is null)
        {
            return DomainError.NotFound(DomainErrorCodes.ShareLinkNotFound, "Share link not found");
        }

        if (link.IsRevoked)
        {
            return DomainError.BusinessRule(DomainErrorCodes.ShareLinkRevoked, "Share link has been revoked");
        }

        if (link.IsExpired(utcNow))
        {
            return DomainError.BusinessRule(DomainErrorCodes.ShareLinkExpired, "Share link has expired");
        }

        if (link.IsDownloadLimitReached)
        {
            return DomainError.BusinessRule(DomainErrorCodes.ShareLinkDownloadLimitReached, "Download limit reached");
        }

        if (link.HasPassword)
        {
            if (string.IsNullOrEmpty(command.Password))
            {
                return DomainError.Validation(DomainErrorCodes.ShareLinkPasswordRequired, "Password required");
            }

            if (!BCrypt.Net.BCrypt.Verify(command.Password, link.PasswordHash))
            {
                return DomainError.Validation(DomainErrorCodes.ShareLinkPasswordIncorrect, "Incorrect password");
            }
        }

        if (command.Action == ShareAccessAction.Download)
        {
            link.IncrementDownloadCount();
        }

        var accessLog = ShareLinkAccessLog.Create(
            link.Id,
            command.IpAddress,
            command.UserAgent,
            command.Action,
            utcNow);

        await accessLogRepository.AddAsync(accessLog, cancellationToken);

        return new AccessShareLinkResult(
            true,
            link.ResourceType.ToString(),
            link.ResourceId,
            link.FamilyId.Value);
    }
}
