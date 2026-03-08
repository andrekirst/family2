using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Models;

namespace FamilyHub.Api.Features.Messaging.Application.Queries.GetConversationMessages;

/// <summary>
/// Query to get messages for a specific conversation with cursor pagination.
/// </summary>
public sealed record GetConversationMessagesQuery(
    ConversationId ConversationId,
    FamilyId FamilyId,
    int Limit = 50,
    DateTime? Before = null
) : IReadOnlyQuery<List<MessageDto>>, IFamilyScoped;
