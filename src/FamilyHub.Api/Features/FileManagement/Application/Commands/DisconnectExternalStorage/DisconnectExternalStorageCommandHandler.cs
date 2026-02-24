using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DisconnectExternalStorage;

public sealed class DisconnectExternalStorageCommandHandler(
    IExternalConnectionRepository connectionRepository)
    : ICommandHandler<DisconnectExternalStorageCommand, DisconnectExternalStorageResult>
{
    public async ValueTask<DisconnectExternalStorageResult> Handle(
        DisconnectExternalStorageCommand command,
        CancellationToken cancellationToken)
    {
        var connection = await connectionRepository.GetByIdAsync(command.ConnectionId, cancellationToken)
            ?? throw new DomainException("External connection not found", DomainErrorCodes.ExternalConnectionNotFound);

        if (connection.FamilyId != command.FamilyId)
            throw new DomainException("External connection not found", DomainErrorCodes.ExternalConnectionNotFound);

        connection.Disconnect();
        await connectionRepository.RemoveAsync(connection, cancellationToken);

        return new DisconnectExternalStorageResult(true);
    }
}
