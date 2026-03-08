using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ConnectExternalStorage;

public sealed record ConnectExternalStorageCommand(
    ExternalProviderType ProviderType,
    string DisplayName,
    string EncryptedAccessToken,
    string? EncryptedRefreshToken,
    DateTime? TokenExpiresAt
) : ICommand<ConnectExternalStorageResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
