using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Queries.GetPendingInvitations;

/// <summary>
/// Result containing pending invitations for a family.
/// </summary>
public sealed record GetPendingInvitationsResult
{
    public required IReadOnlyList<PendingInvitationDto> Invitations { get; init; }
}

/// <summary>
/// DTO representing a pending invitation (domain → application layer mapping).
/// </summary>
public sealed record PendingInvitationDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required UserRole Role { get; init; }
    public required InvitationStatus Status { get; init; }
    public required Guid InvitedByUserId { get; init; }
    public required DateTime InvitedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public string? Message { get; init; }
    public required string DisplayCode { get; init; }

    /// <summary>
    /// Maps a domain FamilyMemberInvitation to a DTO.
    /// </summary>
    public static PendingInvitationDto FromDomain(FamilyMemberInvitation invitation)
    {
        return new PendingInvitationDto
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
