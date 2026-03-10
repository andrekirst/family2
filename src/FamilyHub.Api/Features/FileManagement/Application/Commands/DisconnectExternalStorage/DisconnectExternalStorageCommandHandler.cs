using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DisconnectExternalStorage;

public sealed class DisconnectExternalStorageCommandHandler(
    IExternalConnectionRepository connectionRepository)
    : ICommandHandler<DisconnectExternalStorageCommand, Result<DisconnectExternalStorageResult>>
{
    public async ValueTask<Result<DisconnectExternalStorageResult>> Handle(
        DisconnectExternalStorageCommand command,
        CancellationToken cancellationToken)
    {
        var connection = await connectionRepository.GetByIdAsync(command.ConnectionId, cancellationToken);
        if (connection is null)
        {
            return DomainError.NotFound(DomainErrorCodes.ExternalConnectionNotFound, "External connection not found");
        }

        if (connection.FamilyId != command.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.ExternalConnectionNotFound, "External connection not found");
        }

        connection.Disconnect();
        await connectionRepository.RemoveAsync(connection, cancellationToken);

        return new DisconnectExternalStorageResult(true);
    }
}
