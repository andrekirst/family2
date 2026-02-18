using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteTag;

public sealed class DeleteTagCommandHandler(
    ITagRepository tagRepository,
    IFileTagRepository fileTagRepository)
    : ICommandHandler<DeleteTagCommand, DeleteTagResult>
{
    public async ValueTask<DeleteTagResult> Handle(
        DeleteTagCommand command,
        CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(command.TagId, cancellationToken)
            ?? throw new DomainException("Tag not found", DomainErrorCodes.TagNotFound);

        if (tag.FamilyId != command.FamilyId)
        {
            throw new DomainException("Tag belongs to a different family", DomainErrorCodes.Forbidden);
        }

        // Remove all file-tag associations first
        await fileTagRepository.RemoveByTagIdAsync(command.TagId, cancellationToken);

        // Remove the tag itself
        await tagRepository.RemoveAsync(tag, cancellationToken);

        return new DeleteTagResult(true);
    }
}
