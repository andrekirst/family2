using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFile;

public sealed class MoveFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    IFolderRepository folderRepository)
    : ICommandHandler<MoveFileCommand, MoveFileResult>
{
    public async ValueTask<MoveFileResult> Handle(
        MoveFileCommand command,
        CancellationToken cancellationToken)
    {
        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.NotFound);

        if (file.FamilyId != command.FamilyId)
        {
            throw new DomainException("File belongs to a different family", DomainErrorCodes.Forbidden);
        }

        // Validate target folder exists and belongs to the same family
        var targetFolder = await folderRepository.GetByIdAsync(command.TargetFolderId, cancellationToken)
            ?? throw new DomainException("Target folder not found", DomainErrorCodes.NotFound);

        if (targetFolder.FamilyId != command.FamilyId)
        {
            throw new DomainException("Target folder belongs to a different family", DomainErrorCodes.Forbidden);
        }

        file.MoveTo(command.TargetFolderId, command.MovedBy);

        return new MoveFileResult(file.Id);
    }
}
