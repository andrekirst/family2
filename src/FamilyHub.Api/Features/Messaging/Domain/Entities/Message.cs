using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Events;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Domain.Entities;

/// <summary>
/// Message aggregate root representing a chat message in a family channel.
/// Messages are immutable after creation — no editing or deletion in MVP.
/// </summary>
public sealed class Message : AggregateRoot<MessageId>
{
#pragma warning disable CS8618
    private Message() { }
#pragma warning restore CS8618

    private readonly List<MessageAttachment> _attachments = [];

    /// <summary>
    /// The family this message belongs to.
    /// </summary>
    public FamilyId FamilyId { get; private set; }

    /// <summary>
    /// The user who sent this message.
    /// </summary>
    public UserId SenderId { get; private set; }

    /// <summary>
    /// The message content (plain text, max 4000 characters).
    /// </summary>
    public MessageContent Content { get; private set; }

    /// <summary>
    /// The conversation this message belongs to (null for legacy messages).
    /// </summary>
    public ConversationId? ConversationId { get; private set; }

    /// <summary>
    /// When the message was sent.
    /// </summary>
    public DateTime SentAt { get; private set; }

    /// <summary>
    /// File attachments on this message.
    /// </summary>
    public IReadOnlyList<MessageAttachment> Attachments => _attachments.AsReadOnly();

    /// <summary>
    /// Factory method to create a new message. Raises MessageSentEvent.
    /// </summary>
    public static Message Create(
        FamilyId familyId,
        UserId senderId,
        MessageContent content,
        DateTimeOffset utcNow,
        IReadOnlyList<MessageAttachment>? attachments = null,
        ConversationId? conversationId = null)
    {
        var now = utcNow;
        var message = new Message
        {
            Id = MessageId.New(),
            FamilyId = familyId,
            SenderId = senderId,
            Content = content,
            ConversationId = conversationId,
            SentAt = now.UtcDateTime
        };

        message.RaiseDomainEvent(new MessageSentEvent(
            message.Id,
            message.FamilyId,
            message.SenderId,
            message.Content,
            message.SentAt
        ));

        if (attachments is not null)
        {
            foreach (var attachment in attachments)
            {
                message._attachments.Add(attachment);
                message.RaiseDomainEvent(new MessageAttachmentAddedEvent(
                    message.Id,
                    attachment.FileId,
                    message.FamilyId,
                    message.SenderId,
                    attachment.AttachedAt
                ));
            }
        }

        return message;
    }

    /// <summary>
    /// Add an attachment to this message (for future use).
    /// </summary>
    public void AddAttachment(MessageAttachment attachment)
    {
        _attachments.Add(attachment);
        RaiseDomainEvent(new MessageAttachmentAddedEvent(
            Id,
            attachment.FileId,
            FamilyId,
            SenderId,
            attachment.AttachedAt
        ));
    }
}
