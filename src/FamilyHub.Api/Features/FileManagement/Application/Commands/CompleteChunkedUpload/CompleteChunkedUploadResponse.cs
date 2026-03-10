namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CompleteChunkedUpload;

/// <summary>
/// REST response DTO for the complete chunked upload endpoint.
/// </summary>
public sealed record CompleteChunkedUploadResponse(
    string StorageKey,
    string MimeType,
    long Size,
    string Checksum);
