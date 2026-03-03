using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Messaging.Application.Queries.GetFamilyMessages;

[ExtendObjectType(typeof(MessagingQuery))]
public class QueryType
{
    /// <summary>
    /// Get messages for the current user's family channel.
    /// Supports cursor-based pagination via 'before' timestamp.
    /// </summary>
    [Authorize]
    public async Task<List<MessageDto>> Messages(
        int limit = 50,
        DateTime? before = null,
        ClaimsPrincipal claimsPrincipal = default!,
        [Service] IQueryBus queryBus = default!,
        [Service] IUserRepository userRepository = default!,
        [Service] IUserService userService = default!,
        CancellationToken cancellationToken = default)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException("You must be part of a family to read messages");
        }

        var query = new GetFamilyMessagesQuery(user.FamilyId.Value, limit, before);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
