using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RestoreFileVersion;

public sealed record RestoreFileVersionCommand(
    FileVersionId VersionId,
    FileId FileId,
    UserId RestoredBy,
    FamilyId FamilyId
) : ICommand<RestoreFileVersionResult>, IFamilyScoped;
