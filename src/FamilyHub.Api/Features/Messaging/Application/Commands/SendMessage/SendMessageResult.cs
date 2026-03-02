using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.SendMessage;

/// <summary>
/// Result of sending a message.
/// </summary>
public sealed record SendMessageResult(
    MessageId MessageId
);
