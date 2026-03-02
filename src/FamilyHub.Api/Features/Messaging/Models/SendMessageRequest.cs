namespace FamilyHub.Api.Features.Messaging.Models;

/// <summary>
/// GraphQL input for sending a message. Uses primitives (ADR-003).
/// </summary>
public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
}
