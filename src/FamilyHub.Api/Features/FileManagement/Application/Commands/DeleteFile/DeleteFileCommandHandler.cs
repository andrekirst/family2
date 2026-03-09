using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFile;

public sealed class DeleteFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    IFileManagementStorageService storageService)
    : ICommandHandler<DeleteFileCommand, Result<DeleteFileResult>>
{
    public async ValueTask<Result<DeleteFileResult>> Handle(
        DeleteFileCommand command,
        CancellationToken cancellationToken)
    {
        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken);
        if (file is null)
        {
            return DomainError.NotFound(DomainErrorCodes.NotFound, "File not found");
        }

        if (file.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "File belongs to a different family");
        }

        await storageService.DeleteFileAsync(command.FamilyId, file.StorageKey.Value, file.Size.Value, cancellationToken);

        file.MarkDeleted(command.UserId);
        await storedFileRepository.RemoveAsync(file, cancellationToken);

        return new DeleteFileResult(true);
    }
}
