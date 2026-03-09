using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFile;

public sealed class MoveFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    IFolderRepository folderRepository,
    TimeProvider timeProvider)
    : ICommandHandler<MoveFileCommand, Result<MoveFileResult>>
{
    public async ValueTask<Result<MoveFileResult>> Handle(
        MoveFileCommand command,
        CancellationToken cancellationToken)
    {
        var file = (await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken))!;

        if (file.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "File belongs to a different family");
        }

        var targetFolder = (await folderRepository.GetByIdAsync(command.TargetFolderId, cancellationToken))!;

        if (targetFolder.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Target folder belongs to a different family");
        }

        var utcNow = timeProvider.GetUtcNow();
        file.MoveTo(command.TargetFolderId, command.UserId, utcNow);

        return new MoveFileResult(file.Id, file);
    }
}
