using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateTag;

public sealed class UpdateTagCommandHandler(
    ITagRepository tagRepository)
    : ICommandHandler<UpdateTagCommand, UpdateTagResult>
{
    public async ValueTask<UpdateTagResult> Handle(
        UpdateTagCommand command,
        CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(command.TagId, cancellationToken)
            ?? throw new DomainException("Tag not found", DomainErrorCodes.TagNotFound);

        if (tag.FamilyId != command.FamilyId)
        {
            throw new DomainException("Tag belongs to a different family", DomainErrorCodes.Forbidden);
        }

        if (command.NewName.HasValue)
        {
            // Check for duplicate name
            var existing = await tagRepository.GetByNameAsync(command.NewName.Value, command.FamilyId, cancellationToken);
            if (existing is not null && existing.Id != tag.Id)
            {
                throw new DomainException("A tag with this name already exists", DomainErrorCodes.Conflict);
            }

            tag.Rename(command.NewName.Value);
        }

        if (command.NewColor.HasValue)
        {
            tag.ChangeColor(command.NewColor.Value);
        }

        return new UpdateTagResult(tag.Id);
    }
}
