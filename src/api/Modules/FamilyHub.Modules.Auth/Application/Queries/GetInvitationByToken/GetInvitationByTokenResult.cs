using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken;

/// <summary>
/// Result containing invitation details (for public viewing by token).
/// </summary>
public sealed record GetInvitationByTokenResult
{
    /// <summary>
    /// Gets the unique identifier of the invitation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the email address of the invited person.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the role assigned to the invited person.
    /// </summary>
    public required FamilyRole Role { get; init; }

    /// <summary>
    /// Gets the current status of the invitation.
    /// </summary>
    public required InvitationStatus Status { get; init; }

    /// <summary>
    /// Gets the user ID of the person who sent the invitation.
    /// </summary>
    public required Guid InvitedByUserId { get; init; }

    /// <summary>
    /// Gets the date and time when the invitation was created.
    /// </summary>
    public required DateTime InvitedAt { get; init; }

    /// <summary>
    /// Gets the date and time when the invitation expires.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Gets the optional personal message included with the invitation.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets the human-readable display code for the invitation.
    /// </summary>
    public required string DisplayCode { get; init; }

    /// <summary>
    /// Maps a domain FamilyMemberInvitation to a result DTO.
    /// </summary>
    public static GetInvitationByTokenResult FromDomain(FamilyMemberInvitationAggregate invitation)
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
