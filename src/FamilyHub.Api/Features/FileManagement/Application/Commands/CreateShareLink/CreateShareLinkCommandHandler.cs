using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateShareLink;

public sealed class CreateShareLinkCommandHandler(
    IShareLinkRepository shareLinkRepository)
    : ICommandHandler<CreateShareLinkCommand, CreateShareLinkResult>
{
    public async ValueTask<CreateShareLinkResult> Handle(
        CreateShareLinkCommand command,
        CancellationToken cancellationToken)
    {
        string? passwordHash = null;
        if (!string.IsNullOrEmpty(command.Password))
        {
            passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
        }

        var link = ShareLink.Create(
            command.ResourceType,
            command.ResourceId,
            command.FamilyId,
            command.CreatedBy,
            command.ExpiresAt,
            passwordHash,
            command.MaxDownloads);

        await shareLinkRepository.AddAsync(link, cancellationToken);

        return new CreateShareLinkResult(true, link.Id.Value, link.Token);
    }
}
