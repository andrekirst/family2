using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.StoreUploadedFile;

public sealed record StoreUploadedFileResult(
    StorageKey StorageKey,
    MimeType MimeType,
    FileSize FileSize,
    Checksum Checksum);
