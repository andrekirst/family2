using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.AccessShareLink;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    /// <summary>
    /// Public mutation -- no authentication required.
    /// Validates share link token, password, expiration, and download limits.
    /// </summary>
    public async Task<bool> AccessShareLink(
        string token,
        string? password,
        string ipAddress,
        string? userAgent,
        string action,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var parsedAction = Enum.Parse<ShareAccessAction>(action, ignoreCase: true);

        var command = new AccessShareLinkCommand(
            token,
            password,
            ipAddress,
            userAgent,
            parsedAction);

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
