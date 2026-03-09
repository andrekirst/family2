namespace FamilyHub.Api.Features.FileManagement.Application.Commands.StoreUploadedFile;

/// <summary>
/// REST response DTO for the store uploaded file endpoint.
/// </summary>
public sealed record StoreUploadedFileResponse(
    string StorageKey,
    string MimeType,
    long Size,
    string Checksum);
