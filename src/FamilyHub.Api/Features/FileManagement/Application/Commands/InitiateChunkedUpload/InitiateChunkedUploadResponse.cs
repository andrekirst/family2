namespace FamilyHub.Api.Features.FileManagement.Application.Commands.InitiateChunkedUpload;

/// <summary>
/// REST response DTO for the initiate chunked upload endpoint.
/// </summary>
public sealed record InitiateChunkedUploadResponse(string UploadId);
