namespace FamilyHub.Api.Features.FileManagement.Application.Queries.DownloadFile;

public sealed record DownloadFileResult(
    Stream Data,
    string MimeType,
    long Size);
