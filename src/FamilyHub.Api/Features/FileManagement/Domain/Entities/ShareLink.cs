using System.Security.Cryptography;
using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Represents a shareable link for a file or folder.
/// External users can access shared content via a public route without authentication.
/// Token is a URL-safe base64-encoded 32-byte cryptographic random value.
/// </summary>
public sealed class ShareLink : AggregateRoot<ShareLinkId>
{
#pragma warning disable CS8618
    private ShareLink() { }
#pragma warning restore CS8618

    public static ShareLink Create(
        ShareResourceType resourceType,
        Guid resourceId,
        FamilyId familyId,
        UserId createdBy,
        DateTime? expiresAt,
        string? passwordHash,
        int? maxDownloads)
    {
        var link = new ShareLink
        {
            Id = ShareLinkId.New(),
            Token = GenerateToken(),
            ResourceType = resourceType,
            ResourceId = resourceId,
            FamilyId = familyId,
            CreatedBy = createdBy,
            ExpiresAt = expiresAt,
            PasswordHash = passwordHash,
            MaxDownloads = maxDownloads,
            DownloadCount = 0,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        link.RaiseDomainEvent(new ShareLinkCreatedEvent(
            link.Id, resourceId, familyId, createdBy, expiresAt));

        return link;
    }

    public string Token { get; private set; }
    public ShareResourceType ResourceType { get; private set; }
    public Guid ResourceId { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public UserId CreatedBy { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public string? PasswordHash { get; private set; }
    public int? MaxDownloads { get; private set; }
    public int DownloadCount { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    public bool IsDownloadLimitReached => MaxDownloads.HasValue && DownloadCount >= MaxDownloads.Value;

    public bool IsAccessible => !IsRevoked && !IsExpired && !IsDownloadLimitReached;

    public bool HasPassword => !string.IsNullOrEmpty(PasswordHash);

    public void IncrementDownloadCount()
    {
        DownloadCount++;
    }

    public void Revoke(UserId revokedBy)
    {
        IsRevoked = true;
        RaiseDomainEvent(new ShareLinkRevokedEvent(Id, revokedBy));
    }

    private static string GenerateToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
