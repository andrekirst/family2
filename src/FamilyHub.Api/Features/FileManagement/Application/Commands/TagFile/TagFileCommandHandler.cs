using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.TagFile;

public sealed class TagFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    ITagRepository tagRepository,
    IFileTagRepository fileTagRepository)
    : ICommandHandler<TagFileCommand, TagFileResult>
{
    public async ValueTask<TagFileResult> Handle(
        TagFileCommand command,
        CancellationToken cancellationToken)
    {
        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.FileNotFound);

        if (file.FamilyId != command.FamilyId)
        {
            throw new DomainException("File belongs to a different family", DomainErrorCodes.Forbidden);
        }

        var tag = await tagRepository.GetByIdAsync(command.TagId, cancellationToken)
            ?? throw new DomainException("Tag not found", DomainErrorCodes.TagNotFound);

        if (tag.FamilyId != command.FamilyId)
        {
            throw new DomainException("Tag belongs to a different family", DomainErrorCodes.Forbidden);
        }

        // Check if already tagged
        var alreadyTagged = await fileTagRepository.ExistsAsync(command.FileId, command.TagId, cancellationToken);
        if (alreadyTagged)
        {
            return new TagFileResult(true); // Idempotent
        }

        var fileTag = FileTag.Create(command.FileId, command.TagId);
        await fileTagRepository.AddAsync(fileTag, cancellationToken);

        return new TagFileResult(true);
    }
}
