using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CompleteChunkedUpload;

public sealed record CompleteChunkedUploadResult(
    StorageKey StorageKey,
    MimeType MimeType,
    FileSize Size,
    Checksum Checksum);
