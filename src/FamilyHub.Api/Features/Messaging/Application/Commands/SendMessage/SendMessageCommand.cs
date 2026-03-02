using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.SendMessage;

/// <summary>
/// Command to send a message in a family channel.
/// </summary>
public sealed record SendMessageCommand(
    FamilyId FamilyId,
    UserId SenderId,
    MessageContent Content
) : ICommand<SendMessageResult>;
