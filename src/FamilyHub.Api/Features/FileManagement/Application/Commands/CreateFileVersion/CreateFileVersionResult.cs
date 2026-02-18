namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFileVersion;

public sealed record CreateFileVersionResult(bool Success, Guid VersionId, int VersionNumber);
