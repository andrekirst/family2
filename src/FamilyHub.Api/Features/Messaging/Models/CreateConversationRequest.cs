namespace FamilyHub.Api.Features.Messaging.Models;

/// <summary>
/// GraphQL input for creating a conversation. Uses primitives (ADR-003).
/// </summary>
public class CreateConversationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Group";
    public List<Guid> MemberIds { get; set; } = [];
}
