using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ConnectExternalStorage;

public sealed class ConnectExternalStorageCommandHandler(
    IExternalConnectionRepository connectionRepository)
    : ICommandHandler<ConnectExternalStorageCommand, ConnectExternalStorageResult>
{
    public async ValueTask<ConnectExternalStorageResult> Handle(
        ConnectExternalStorageCommand command,
        CancellationToken cancellationToken)
    {
        var existing = await connectionRepository.GetByFamilyAndProviderAsync(
            command.FamilyId, command.ProviderType, cancellationToken);

        if (existing is not null)
            throw new DomainException(
                "Connection to this provider already exists",
                DomainErrorCodes.ExternalConnectionAlreadyExists);

        var connection = ExternalConnection.Create(
            command.FamilyId,
            command.ProviderType,
            command.DisplayName,
            command.EncryptedAccessToken,
            command.EncryptedRefreshToken,
            command.TokenExpiresAt,
            command.ConnectedBy);

        await connectionRepository.AddAsync(connection, cancellationToken);

        return new ConnectExternalStorageResult(connection.Id.Value);
    }
}
