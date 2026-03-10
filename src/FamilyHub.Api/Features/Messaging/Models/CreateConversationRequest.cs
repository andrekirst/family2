using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Models;

/// <summary>
/// GraphQL input for creating a conversation. Uses primitives (ADR-003).
/// </summary>
public class CreateConversationRequest
{
    public string Name { get; set; } = string.Empty;
    public ConversationType Type { get; set; } = ConversationType.Group;
    public List<Guid> MemberIds { get; set; } = [];
}
