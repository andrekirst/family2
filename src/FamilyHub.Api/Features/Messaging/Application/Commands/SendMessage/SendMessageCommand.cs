using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.SendMessage;

/// <summary>
/// Attachment data passed through the command pipeline.
/// </summary>
public sealed record AttachmentData(FileId FileId, string FileName, string MimeType, long FileSize);

/// <summary>
/// Command to send a message in a family channel.
/// </summary>
public sealed record SendMessageCommand(
    FamilyId FamilyId,
    UserId SenderId,
    MessageContent Content,
    IReadOnlyList<AttachmentData>? Attachments = null
) : ICommand<SendMessageResult>;
