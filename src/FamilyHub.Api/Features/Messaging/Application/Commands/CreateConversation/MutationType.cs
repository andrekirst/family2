using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
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
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IConversationRepository conversationRepository,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException("You must be part of a family to create conversations");
        }

        if (!Enum.TryParse<ConversationType>(input.Type, ignoreCase: true, out var conversationType))
        {
            throw new InvalidOperationException($"Invalid conversation type: {input.Type}");
        }

        var command = new CreateConversationCommand(
            user.FamilyId.Value,
            user.Id,
            ConversationName.From(input.Name),
            conversationType,
            input.MemberIds);

        var result = await commandBus.SendAsync(command, cancellationToken);

        var conversation = await conversationRepository.GetByIdAsync(result.ConversationId, cancellationToken);
        return ConversationMapper.ToDto(conversation!);
    }
}
