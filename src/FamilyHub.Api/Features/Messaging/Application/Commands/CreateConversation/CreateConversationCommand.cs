using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.CreateConversation;

/// <summary>
/// Command to create a new conversation (Direct, Group, or Family).
/// </summary>
public sealed record CreateConversationCommand(
    ConversationName Name,
    ConversationType Type,
    IReadOnlyList<Guid> MemberIds
) : ICommand<CreateConversationResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
