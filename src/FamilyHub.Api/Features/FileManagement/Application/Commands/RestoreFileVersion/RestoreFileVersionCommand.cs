using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RestoreFileVersion;

public sealed record RestoreFileVersionCommand(
    FileVersionId VersionId,
    FileId FileId
) : ICommand<RestoreFileVersionResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
