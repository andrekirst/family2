using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Events;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Domain.Entities;

/// <summary>
/// Aggregate root representing a conversation (Family/Direct/Group).
/// Each conversation gets a dedicated folder for file attachments.
/// </summary>
public sealed class Conversation : AggregateRoot<ConversationId>
{
#pragma warning disable CS8618
    private Conversation() { }
#pragma warning restore CS8618

    private readonly List<ConversationMember> _members = [];

    public ConversationName Name { get; private set; }
    public ConversationType Type { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public UserId CreatedBy { get; private set; }
    public FolderId? FolderId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<ConversationMember> Members => _members.AsReadOnly();

    /// <summary>
    /// Creates a new Direct or Group conversation.
    /// </summary>
    public static Conversation Create(
        ConversationName name,
        ConversationType type,
        FamilyId familyId,
        UserId createdBy,
        IReadOnlyList<UserId> memberIds,
        DateTimeOffset utcNow)
    {
        var now = utcNow;
        var conversation = new Conversation
        {
            Id = ConversationId.New(),
            Name = name,
            Type = type,
            FamilyId = familyId,
            CreatedBy = createdBy,
            CreatedAt = now.UtcDateTime
        };

        // Creator is always an Owner
        conversation._members.Add(ConversationMember.Create(createdBy, now, "Owner"));

        // Add other members
        foreach (var memberId in memberIds.Where(id => id != createdBy))
        {
            conversation._members.Add(ConversationMember.Create(memberId, now));
        }

        conversation.RaiseDomainEvent(new ConversationCreatedEvent(
            conversation.Id,
            conversation.FamilyId,
            conversation.Name,
            conversation.Type,
            conversation.CreatedBy));

        return conversation;
    }

    /// <summary>
    /// Creates the default "General" family conversation.
    /// </summary>
    public static Conversation CreateFamily(FamilyId familyId, UserId createdBy, DateTimeOffset utcNow)
    {
        var now = utcNow;
        var conversation = new Conversation
        {
            Id = ConversationId.New(),
            Name = ConversationName.From("General"),
            Type = ConversationType.Family,
            FamilyId = familyId,
            CreatedBy = createdBy,
            CreatedAt = now.UtcDateTime
        };

        conversation._members.Add(ConversationMember.Create(createdBy, now, "Owner"));

        conversation.RaiseDomainEvent(new ConversationCreatedEvent(
            conversation.Id,
            conversation.FamilyId,
            conversation.Name,
            conversation.Type,
            conversation.CreatedBy));

        return conversation;
    }

    public void AddMember(UserId userId, DateTimeOffset utcNow)
    {
        if (_members.Any(m => m.UserId == userId && m.IsActive))
            return;

        _members.Add(ConversationMember.Create(userId, utcNow));

        RaiseDomainEvent(new ConversationMemberAddedEvent(Id, userId, FamilyId));
    }

    public void RemoveMember(UserId userId, DateTimeOffset utcNow)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
        if (member is null)
            return;

        member.Leave(utcNow);

        RaiseDomainEvent(new ConversationMemberRemovedEvent(Id, userId, FamilyId));
    }

    public void SetFolderId(FolderId folderId)
    {
        FolderId = folderId;
    }

    public bool HasActiveMember(UserId userId)
    {
        return _members.Any(m => m.UserId == userId && m.IsActive);
    }
}
