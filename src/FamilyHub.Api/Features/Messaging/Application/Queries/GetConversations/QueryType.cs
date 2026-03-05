using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Messaging.Application.Queries.GetConversations;

[ExtendObjectType(typeof(MessagingQuery))]
public class GetConversationsQueryType
{
    /// <summary>
    /// Get all conversations the current user belongs to.
    /// </summary>
    [Authorize]
    public async Task<List<ConversationDto>> Conversations(
        ClaimsPrincipal claimsPrincipal = default!,
        [Service] IQueryBus queryBus = default!,
        [Service] IUserRepository userRepository = default!,
        [Service] IUserService userService = default!,
        CancellationToken cancellationToken = default)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException("You must be part of a family to list conversations");
        }

        var query = new GetConversationsQuery(user.FamilyId.Value, user.Id);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
