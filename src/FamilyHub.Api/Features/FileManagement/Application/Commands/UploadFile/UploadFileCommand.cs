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
    FolderId FolderId
) : ICommand<Result<UploadFileResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
