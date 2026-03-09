using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFile;

public sealed class RenameFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    TimeProvider timeProvider)
    : ICommandHandler<RenameFileCommand, RenameFileResult>
{
    public async ValueTask<RenameFileResult> Handle(
        RenameFileCommand command,
        CancellationToken cancellationToken)
    {
        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.NotFound);

        if (file.FamilyId != command.FamilyId)
        {
            throw new DomainException("File belongs to a different family", DomainErrorCodes.Forbidden);
        }

        var utcNow = timeProvider.GetUtcNow();
        file.Rename(command.NewName, command.UserId, utcNow);

        return new RenameFileResult(file.Id, file);
    }
}
