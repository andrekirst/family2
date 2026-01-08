using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Modules.Family.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken;

/// <summary>
/// Result containing invitation details (for public viewing by token).
/// </summary>
public sealed record GetInvitationByTokenResult
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required FamilyRole Role { get; init; }
    public required InvitationStatus Status { get; init; }
    public required Guid InvitedByUserId { get; init; }
    public required DateTime InvitedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public string? Message { get; init; }
    public required string DisplayCode { get; init; }

    /// <summary>
    /// Maps a domain FamilyMemberInvitation to a result DTO.
    /// </summary>
    public static GetInvitationByTokenResult FromDomain(FamilyHub.Modules.Family.Domain.FamilyMemberInvitation invitation)
    {
        return new GetInvitationByTokenResult
        {
            Id = invitation.Id.Value,
            Email = invitation.Email.Value,
            Role = invitation.Role,
            Status = invitation.Status,
            InvitedByUserId = invitation.InvitedByUserId.Value,
            InvitedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            Message = invitation.Message,
            DisplayCode = invitation.DisplayCode.Value
        };
    }
}
