using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
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
    [HotChocolate.Types.UsePaging]
    public async Task<List<ConversationDto>> Conversations(
        [Service] IQueryBus queryBus = default!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetConversationsQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
