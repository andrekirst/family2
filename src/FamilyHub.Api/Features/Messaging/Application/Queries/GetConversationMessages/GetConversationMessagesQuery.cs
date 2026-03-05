using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Models;

namespace FamilyHub.Api.Features.Messaging.Application.Queries.GetConversationMessages;

/// <summary>
/// Query to get messages for a specific conversation with cursor pagination.
/// </summary>
public sealed record GetConversationMessagesQuery(
    ConversationId ConversationId,
    int Limit = 50,
    DateTime? Before = null
) : IQuery<List<MessageDto>>;
