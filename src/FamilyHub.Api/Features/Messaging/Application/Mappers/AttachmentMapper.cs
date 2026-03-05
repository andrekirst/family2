using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Models;

namespace FamilyHub.Api.Features.Messaging.Application.Mappers;

public static class AttachmentMapper
{
    public static AttachmentDto ToDto(MessageAttachment attachment)
    {
        return new AttachmentDto
        {
            FileId = attachment.FileId.Value,
            FileName = attachment.FileName,
            MimeType = attachment.MimeType,
            FileSize = attachment.FileSize,
            StorageKey = attachment.StorageKey,
            AttachedAt = attachment.AttachedAt
        };
    }

    public static List<AttachmentDto> ToDtoList(IReadOnlyList<MessageAttachment> attachments)
    {
        return attachments.Select(ToDto).ToList();
    }
}
