using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Messaging.Application.Queries.GetConversationMessages;

[ExtendObjectType(typeof(MessagingQuery))]
public class GetConversationMessagesQueryType
{
    /// <summary>
    /// Get messages for a specific conversation.
    /// Validates that the current user is a member of the conversation.
    /// </summary>
    [Authorize]
    [HotChocolate.Types.UsePaging]
    public async Task<List<MessageDto>> ConversationMessages(
        Guid conversationId,
        int limit = 50,
        DateTime? before = null,
        [Service] IQueryBus queryBus = default!,
        [Service] IConversationRepository conversationRepository = default!,
        CancellationToken cancellationToken = default)
    {
        var convId = ConversationId.From(conversationId);

        var query = new GetConversationMessagesQuery(convId, limit, before);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
