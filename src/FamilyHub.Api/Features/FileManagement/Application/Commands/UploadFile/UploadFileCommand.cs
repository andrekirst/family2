using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UploadFile;

public sealed record UploadFileCommand(
    FileName Name,
    MimeType MimeType,
    FileSize Size,
    StorageKey StorageKey,
    Checksum Checksum,
    FolderId FolderId,
    FamilyId FamilyId,
    UserId UploadedBy
) : ICommand<UploadFileResult>;
