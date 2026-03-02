using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Domain.Events;

/// <summary>
/// Domain event raised when a message is sent in a family channel.
/// Used for real-time subscription publishing and future event chain triggers.
/// </summary>
public sealed record MessageSentEvent(
    MessageId MessageId,
    FamilyId FamilyId,
    UserId SenderId,
    MessageContent Content,
    DateTime SentAt
) : DomainEvent;
