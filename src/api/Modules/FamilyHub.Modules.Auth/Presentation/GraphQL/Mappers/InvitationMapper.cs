using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Mappers;

/// <summary>
/// Centralized mapper for invitation-related domain types to GraphQL types.
/// Provides consistent mapping logic for roles, statuses, and invitation data.
/// </summary>
public static class InvitationMapper
{
    /// <summary>
    /// Maps FamilyRole value object to UserRoleType GraphQL enum.
    /// Throws InvalidOperationException for unknown roles (fail-fast approach).
    /// </summary>
    public static UserRoleType AsRoleType(this FamilyRole role) =>
        role.Value.ToLowerInvariant() switch
        {
            "owner" => UserRoleType.OWNER,
            "admin" => UserRoleType.ADMIN,
            "member" => UserRoleType.MEMBER,
            "child" => UserRoleType.CHILD,
            _ => throw new InvalidOperationException($"Unknown role: {role.Value}")
        };

    /// <summary>
    /// Maps InvitationStatus value object to InvitationStatusType GraphQL enum.
    /// Throws InvalidOperationException for unknown statuses (fail-fast approach).
    /// </summary>
    public static InvitationStatusType AsStatusType(this InvitationStatus status) =>
        status.Value.ToLowerInvariant() switch
        {
            InvitationStatusConstants.PendingValue => InvitationStatusType.PENDING,
            InvitationStatusConstants.AcceptedValue => InvitationStatusType.ACCEPTED,
            // TODO WHere is the constant or why is rejected here? Explain it to me
            "rejected" => InvitationStatusType.REJECTED,
            InvitationStatusConstants.CanceledValue => InvitationStatusType.CANCELLED,
            InvitationStatusConstants.ExpiredValue => InvitationStatusType.EXPIRED,
            _ => throw new InvalidOperationException($"Unknown invitation status: {status.Value}")
        };
}
