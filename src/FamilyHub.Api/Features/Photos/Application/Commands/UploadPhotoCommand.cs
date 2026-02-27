using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Photos.Application.Commands;

public sealed record UploadPhotoCommand(
    FamilyId FamilyId,
    UserId UploadedBy,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StoragePath,
    PhotoCaption? Caption
) : ICommand<UploadPhotoResult>;
