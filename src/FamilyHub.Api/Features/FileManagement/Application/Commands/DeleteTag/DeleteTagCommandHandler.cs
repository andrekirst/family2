using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteTag;

public sealed class DeleteTagCommandHandler(
    ITagRepository tagRepository,
    IFileTagRepository fileTagRepository)
    : ICommandHandler<DeleteTagCommand, Result<DeleteTagResult>>
{
    public async ValueTask<Result<DeleteTagResult>> Handle(
        DeleteTagCommand command,
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

        await fileTagRepository.RemoveByTagIdAsync(command.TagId, cancellationToken);

        await tagRepository.RemoveAsync(tag, cancellationToken);

        return new DeleteTagResult(true);
    }
}
