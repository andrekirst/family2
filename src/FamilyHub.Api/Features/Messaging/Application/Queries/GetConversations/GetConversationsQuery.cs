using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Models;

namespace FamilyHub.Api.Features.Messaging.Application.Queries.GetConversations;

/// <summary>
/// Query to get all conversations the user belongs to in a family.
/// </summary>
public sealed record GetConversationsQuery : IReadOnlyQuery<List<ConversationDto>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
