using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Audit log entry for share link access.
/// Records IP, user agent, and action (view/download) for security and analytics.
/// </summary>
public sealed class ShareLinkAccessLog : AggregateRoot<ShareLinkAccessLogId>
{
#pragma warning disable CS8618
    private ShareLinkAccessLog() { }
#pragma warning restore CS8618

    public static ShareLinkAccessLog Create(
        ShareLinkId shareLinkId,
        string ipAddress,
        string? userAgent,
        ShareAccessAction action)
    {
        return new ShareLinkAccessLog
        {
            Id = ShareLinkAccessLogId.New(),
            ShareLinkId = shareLinkId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Action = action,
            AccessedAt = DateTime.UtcNow
        };
    }

    public ShareLinkId ShareLinkId { get; private set; }
    public string IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public ShareAccessAction Action { get; private set; }
    public DateTime AccessedAt { get; private set; }
}
