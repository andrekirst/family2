using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UntagFile;

public sealed class UntagFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    IFileTagRepository fileTagRepository)
    : ICommandHandler<UntagFileCommand, Result<UntagFileResult>>
{
    public async ValueTask<Result<UntagFileResult>> Handle(
        UntagFileCommand command,
        CancellationToken cancellationToken)
    {
        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken);
        if (file is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        if (file.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "File belongs to a different family");
        }

        var fileTags = await fileTagRepository.GetByFileIdAsync(command.FileId, cancellationToken);
        var fileTag = fileTags.FirstOrDefault(ft => ft.TagId == command.TagId);

        if (fileTag is null)
        {
            return new UntagFileResult(true);
        }

        await fileTagRepository.RemoveAsync(fileTag, cancellationToken);

        return new UntagFileResult(true);
    }
}
