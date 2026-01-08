using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.InviteFamilyMemberByEmail;

/// <summary>
/// Result of inviting a family member by email.
/// Contains invitation information returned from the command handler.
/// </summary>
public record InviteFamilyMemberByEmailResult
{
    public required InvitationId InvitationId { get; init; }
    public required Email Email { get; init; }
    public required FamilyRole Role { get; init; }
    public required InvitationToken Token { get; init; }
    public required InvitationDisplayCode DisplayCode { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required InvitationStatus Status { get; init; }
}
