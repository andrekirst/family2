using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ConnectExternalStorage;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<bool> ConnectExternalStorage(
        string providerType,
        string displayName,
        string encryptedAccessToken,
        string? encryptedRefreshToken,
        DateTime? tokenExpiresAt,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var provider = Enum.Parse<ExternalProviderType>(providerType, ignoreCase: true);

        var command = new ConnectExternalStorageCommand(
            provider,
            displayName,
            encryptedAccessToken,
            encryptedRefreshToken,
            tokenExpiresAt);

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match(
            success => true,
            error => throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Message)
                    .SetCode(error.ErrorCode)
                    .Build()));
    }
}
