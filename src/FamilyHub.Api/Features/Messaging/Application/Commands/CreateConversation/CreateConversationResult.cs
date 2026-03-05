using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.CreateConversation;

/// <summary>
/// Result of creating a conversation.
/// </summary>
public sealed record CreateConversationResult(
    ConversationId ConversationId
);
