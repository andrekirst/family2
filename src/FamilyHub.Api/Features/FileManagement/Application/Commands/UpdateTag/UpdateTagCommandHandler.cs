using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateTag;

public sealed class UpdateTagCommandHandler(
    ITagRepository tagRepository)
    : ICommandHandler<UpdateTagCommand, Result<UpdateTagResult>>
{
    public async ValueTask<Result<UpdateTagResult>> Handle(
        UpdateTagCommand command,
        CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(command.TagId, cancellationToken);
        if (tag is null)
        {
            return DomainError.NotFound(DomainErrorCodes.TagNotFound, "Tag not found");
        }

        if (tag.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Tag belongs to a different family");
        }

        if (command.NewName.HasValue)
        {
            var existing = await tagRepository.GetByNameAsync(command.NewName.Value, command.FamilyId, cancellationToken);
            if (existing is not null && existing.Id != tag.Id)
            {
                return DomainError.Conflict(DomainErrorCodes.Conflict, "A tag with this name already exists");
            }

            tag.Rename(command.NewName.Value);
        }

        if (command.NewColor.HasValue)
        {
            tag.ChangeColor(command.NewColor.Value);
        }

        return new UpdateTagResult(tag.Id, tag);
    }
}
