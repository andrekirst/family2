using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
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
    [HotChocolate.Types.UsePaging]
    public async Task<List<MessageDto>> Messages(
        int limit = 50,
        DateTime? before = null,
        [Service] IQueryBus queryBus = default!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFamilyMessagesQuery(limit, before);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
