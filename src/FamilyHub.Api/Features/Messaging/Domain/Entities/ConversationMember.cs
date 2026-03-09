using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Domain.Entities;

/// <summary>
/// Owned entity representing a user's membership in a conversation.
/// Tracks join/leave lifecycle for access control.
/// </summary>
public sealed class ConversationMember
{
#pragma warning disable CS8618
    private ConversationMember() { }
#pragma warning restore CS8618

    public ConversationMemberId Id { get; private set; }
    public UserId UserId { get; private set; }
    public string Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }

    public bool IsActive => LeftAt is null;

    public static ConversationMember Create(UserId userId, string role = "Member", DateTimeOffset? utcNow = null)
    {
        var now = utcNow ?? DateTimeOffset.UtcNow;
        return new ConversationMember
        {
            Id = ConversationMemberId.New(),
            UserId = userId,
            Role = role,
            JoinedAt = now.UtcDateTime
        };
    }

    public void Leave(DateTimeOffset? utcNow = null)
    {
        var now = utcNow ?? DateTimeOffset.UtcNow;
        LeftAt = now.UtcDateTime;
    }
}
