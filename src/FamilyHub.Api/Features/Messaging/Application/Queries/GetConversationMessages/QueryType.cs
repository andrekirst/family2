using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
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
    public async Task<List<MessageDto>> ConversationMessages(
        Guid conversationId,
        int limit = 50,
        DateTime? before = null,
        ClaimsPrincipal claimsPrincipal = default!,
        [Service] IQueryBus queryBus = default!,
        [Service] IConversationRepository conversationRepository = default!,
        [Service] IUserRepository userRepository = default!,
        [Service] IUserService userService = default!,
        CancellationToken cancellationToken = default)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException("You must be part of a family to read messages");
        }

        var convId = ConversationId.From(conversationId);
        var conversation = await conversationRepository.GetByIdAsync(convId, cancellationToken)
            ?? throw new InvalidOperationException("Conversation not found");

        // Validate membership
        if (!conversation.HasActiveMember(user.Id))
        {
            throw new UnauthorizedAccessException("You are not a member of this conversation");
        }

        var query = new GetConversationMessagesQuery(convId, user.FamilyId.Value, limit, before);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
