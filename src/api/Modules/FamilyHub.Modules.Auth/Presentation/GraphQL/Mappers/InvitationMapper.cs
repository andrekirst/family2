using FamilyHub.Modules.Auth.Domain.Constants;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Mappers;

/// <summary>
/// Centralized mapper for invitation-related domain types to GraphQL types.
/// Provides consistent mapping logic for roles, statuses, and invitation data.
/// </summary>
public static class InvitationMapper
{
    /// <summary>
    /// Maps UserRole value object to UserRoleType GraphQL enum.
    /// Throws InvalidOperationException for unknown roles (fail-fast approach).
    /// </summary>
    public static UserRoleType AsRoleType(this UserRole role) =>
        role.Value.ToLowerInvariant() switch
        {
            UserRoleConstants.OwnerValue => UserRoleType.OWNER,
            UserRoleConstants.AdminValue => UserRoleType.ADMIN,
            UserRoleConstants.MemberValue => UserRoleType.MEMBER,
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
