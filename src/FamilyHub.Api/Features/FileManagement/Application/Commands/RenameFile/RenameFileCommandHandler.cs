using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFile;

public sealed class RenameFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    TimeProvider timeProvider)
    : ICommandHandler<RenameFileCommand, Result<RenameFileResult>>
{
    public async ValueTask<Result<RenameFileResult>> Handle(
        RenameFileCommand command,
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

        var utcNow = timeProvider.GetUtcNow();
        file.Rename(command.NewName, command.UserId, utcNow);

        return new RenameFileResult(file.Id, file);
    }
}
