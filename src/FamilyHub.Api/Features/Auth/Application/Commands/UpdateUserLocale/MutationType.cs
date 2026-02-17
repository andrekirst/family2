using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateUserLocale;

/// <summary>
/// GraphQL input for updating user locale preference.
/// </summary>
public record UpdateUserLocaleRequest(string Locale);

[ExtendObjectType(typeof(RootMutation))]
public class MutationType
{
    /// <summary>
    /// Update the current user's preferred locale (e.g. "en", "de").
    /// Persists to database for cross-device sync.
    /// </summary>
    [Authorize]
    public async Task<bool> UpdateMyLocale(
        UpdateUserLocaleRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException("Invalid token: missing sub claim");

        var command = new UpdateUserLocaleCommand(
            ExternalUserId.From(externalUserIdString),
            input.Locale);

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Success;
    }
}
