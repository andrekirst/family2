using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Messaging.Application.Mappers;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.CreateConversation;

[ExtendObjectType(typeof(MessagingMutation))]
public class CreateConversationMutationType
{
    /// <summary>
    /// Create a new conversation (Direct, Group, or Family).
    /// </summary>
    [Authorize]
    public async Task<ConversationDto> CreateConversation(
        CreateConversationRequest input,
        [Service] ICommandBus commandBus,
        [Service] IConversationRepository conversationRepository,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ConversationType>(input.Type, ignoreCase: true, out var conversationType))
        {
            throw new InvalidOperationException($"Invalid conversation type: {input.Type}");
        }

        var command = new CreateConversationCommand(
            ConversationName.From(input.Name),
            conversationType,
            input.MemberIds);

        var result = await commandBus.SendAsync(command, cancellationToken);

        var conversation = await conversationRepository.GetByIdAsync(result.ConversationId, cancellationToken);
        return ConversationMapper.ToDto(conversation!);
    }
}
