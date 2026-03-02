namespace FamilyHub.Api.Features.Messaging.Models;

/// <summary>
/// GraphQL input for fetching family messages with cursor pagination.
/// </summary>
public class GetFamilyMessagesRequest
{
    public int Limit { get; set; } = 50;
    public DateTime? Before { get; set; }
}
