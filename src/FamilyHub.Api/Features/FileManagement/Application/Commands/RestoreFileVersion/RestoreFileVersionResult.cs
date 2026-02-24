namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RestoreFileVersion;

public sealed record RestoreFileVersionResult(bool Success, Guid NewVersionId, int NewVersionNumber);
