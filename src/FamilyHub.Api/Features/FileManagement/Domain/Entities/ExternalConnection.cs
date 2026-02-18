using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Represents a connection to an external storage provider (OneDrive, Google Drive, Dropbox, Paperless-ngx).
/// Tokens are stored encrypted (AES-256-GCM) — the entity stores only ciphertext.
/// </summary>
public sealed class ExternalConnection : AggregateRoot<ExternalConnectionId>
{
#pragma warning disable CS8618
    private ExternalConnection() { }
#pragma warning restore CS8618

    public static ExternalConnection Create(
        FamilyId familyId,
        ExternalProviderType providerType,
        string displayName,
        string encryptedAccessToken,
        string? encryptedRefreshToken,
        DateTime? tokenExpiresAt,
        UserId connectedBy)
    {
        var connection = new ExternalConnection
        {
            Id = ExternalConnectionId.New(),
            FamilyId = familyId,
            ProviderType = providerType,
            DisplayName = displayName,
            EncryptedAccessToken = encryptedAccessToken,
            EncryptedRefreshToken = encryptedRefreshToken,
            TokenExpiresAt = tokenExpiresAt,
            ConnectedBy = connectedBy,
            Status = ConnectionStatus.Connected,
            ConnectedAt = DateTime.UtcNow
        };

        connection.RaiseDomainEvent(new ExternalStorageConnectedEvent(
            connection.Id, providerType, familyId, connectedBy));

        return connection;
    }

    public void UpdateTokens(
        string encryptedAccessToken,
        string? encryptedRefreshToken,
        DateTime? tokenExpiresAt)
    {
        EncryptedAccessToken = encryptedAccessToken;
        EncryptedRefreshToken = encryptedRefreshToken;
        TokenExpiresAt = tokenExpiresAt;
        Status = ConnectionStatus.Connected;
    }

    public void MarkError()
    {
        Status = ConnectionStatus.Error;
    }

    public void MarkExpired()
    {
        Status = ConnectionStatus.Expired;
    }

    public void Disconnect()
    {
        Status = ConnectionStatus.Disconnected;
        RaiseDomainEvent(new ExternalStorageDisconnectedEvent(
            Id, ProviderType, FamilyId));
    }

    public bool IsTokenExpired => TokenExpiresAt.HasValue && TokenExpiresAt.Value <= DateTime.UtcNow;

    public FamilyId FamilyId { get; private set; }
    public ExternalProviderType ProviderType { get; private set; }
    public string DisplayName { get; private set; }
    public string EncryptedAccessToken { get; private set; }
    public string? EncryptedRefreshToken { get; private set; }
    public DateTime? TokenExpiresAt { get; private set; }
    public UserId ConnectedBy { get; private set; }
    public ConnectionStatus Status { get; private set; }
    public DateTime ConnectedAt { get; private set; }
}
