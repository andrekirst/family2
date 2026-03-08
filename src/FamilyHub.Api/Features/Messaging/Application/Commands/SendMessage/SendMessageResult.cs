using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.SendMessage;

/// <summary>
/// Result of sending a message.
/// </summary>
public sealed record SendMessageResult(
    MessageId MessageId,
    Message SentMessage
);
