using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.SendMessage;

/// <summary>
/// Attachment data passed through the command pipeline.
/// StorageKey references the binary in the storage provider (MinIO/Postgres).
/// Checksum is the SHA-256 hash from the upload response.
/// </summary>
public sealed record AttachmentData(
    string StorageKey, string FileName, string MimeType, long FileSize, string Checksum);

/// <summary>
/// Command to send a message in a family channel.
/// </summary>
public sealed record SendMessageCommand(
    MessageContent Content,
    IReadOnlyList<AttachmentData>? Attachments = null,
    ConversationId? ConversationId = null
) : ICommand<SendMessageResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
