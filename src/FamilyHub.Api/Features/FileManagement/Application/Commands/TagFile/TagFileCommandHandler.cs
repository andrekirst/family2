using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.TagFile;

public sealed class TagFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    ITagRepository tagRepository,
    IFileTagRepository fileTagRepository,
    TimeProvider timeProvider)
    : ICommandHandler<TagFileCommand, Result<TagFileResult>>
{
    public async ValueTask<Result<TagFileResult>> Handle(
        TagFileCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken);
        if (file is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        if (file.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "File belongs to a different family");
        }

        var tag = await tagRepository.GetByIdAsync(command.TagId, cancellationToken);
        if (tag is null)
        {
            return DomainError.NotFound(DomainErrorCodes.TagNotFound, "Tag not found");
        }

        if (tag.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Tag belongs to a different family");
        }

        var alreadyTagged = await fileTagRepository.ExistsAsync(command.FileId, command.TagId, cancellationToken);
        if (alreadyTagged)
        {
            return new TagFileResult(true);
        }

        var fileTag = FileTag.Create(command.FileId, command.TagId, utcNow);
        await fileTagRepository.AddAsync(fileTag, cancellationToken);

        return new TagFileResult(true);
    }
}
