using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UntagFile;

public sealed class UntagFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    IFileTagRepository fileTagRepository)
    : ICommandHandler<UntagFileCommand, UntagFileResult>
{
    public async ValueTask<UntagFileResult> Handle(
        UntagFileCommand command,
        CancellationToken cancellationToken)
    {
        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.FileNotFound);

        if (file.FamilyId != command.FamilyId)
        {
            throw new DomainException("File belongs to a different family", DomainErrorCodes.Forbidden);
        }

        var fileTags = await fileTagRepository.GetByFileIdAsync(command.FileId, cancellationToken);
        var fileTag = fileTags.FirstOrDefault(ft => ft.TagId == command.TagId);

        if (fileTag is null)
        {
            return new UntagFileResult(true); // Idempotent
        }

        await fileTagRepository.RemoveAsync(fileTag, cancellationToken);

        return new UntagFileResult(true);
    }
}
