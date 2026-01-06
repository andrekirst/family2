namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type for reference data (roles, enums, constants).
/// Organizes reference queries under a nested structure.
/// </summary>
public sealed record ReferenceDataType
{
    /// <summary>
    /// Role-related reference data (all roles, invitable roles).
    /// </summary>
    public required RolesType Roles { get; init; }

    // Future: Add other reference data categories
    // public InvitationStatusesType? Statuses { get; init; }
}
