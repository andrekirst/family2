using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFile;

public sealed class DeleteFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    IFileManagementStorageService storageService)
    : ICommandHandler<DeleteFileCommand, DeleteFileResult>
{
    public async ValueTask<DeleteFileResult> Handle(
        DeleteFileCommand command,
        CancellationToken cancellationToken)
    {
        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.NotFound);

        if (file.FamilyId != command.FamilyId)
        {
            throw new DomainException("File belongs to a different family", DomainErrorCodes.Forbidden);
        }

        // Delete binary data from storage and decrement quota
        await storageService.DeleteFileAsync(command.FamilyId, file.StorageKey.Value, file.Size.Value, cancellationToken);

        // Raise domain event and remove metadata
        file.MarkDeleted(command.DeletedBy);
        await storedFileRepository.RemoveAsync(file, cancellationToken);

        return new DeleteFileResult(true);
    }
}
