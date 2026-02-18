using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFileVersion;

public sealed record CreateFileVersionCommand(
    FileId FileId,
    StorageKey StorageKey,
    FileSize FileSize,
    Checksum Checksum,
    UserId UploadedBy
) : ICommand<CreateFileVersionResult>;
