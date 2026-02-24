using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RestoreFileVersion;

public sealed record RestoreFileVersionCommand(
    FileVersionId VersionId,
    FileId FileId,
    UserId RestoredBy
) : ICommand<RestoreFileVersionResult>;
