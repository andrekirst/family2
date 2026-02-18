using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ConnectExternalStorage;

public sealed record ConnectExternalStorageCommand(
    FamilyId FamilyId,
    ExternalProviderType ProviderType,
    string DisplayName,
    string EncryptedAccessToken,
    string? EncryptedRefreshToken,
    DateTime? TokenExpiresAt,
    UserId ConnectedBy
) : ICommand<ConnectExternalStorageResult>;
