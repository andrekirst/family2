using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.SharedKernel.Application.Abstractions;

/// <summary>
/// Provides access to the current authenticated user's context within a MediatR request.
/// This is a shared abstraction that can be used across modules.
/// Automatically populated by UserContextEnrichmentBehavior for requests marked with IRequireAuthentication.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the user ID of the current authenticated user.
    /// </summary>
    UserId UserId { get; }

    /// <summary>
    /// Gets the family ID of the current authenticated user.
    /// </summary>
    FamilyId FamilyId { get; }

    /// <summary>
    /// Gets the role of the current authenticated user in their family.
    /// </summary>
    FamilyRole Role { get; }

    /// <summary>
    /// Gets the email of the current authenticated user.
    /// </summary>
    Email Email { get; }

    // Role helper methods
    /// <summary>
    /// Returns true if the current user has the Owner role.
    /// </summary>
    bool IsOwner => Role == FamilyRole.Owner;

    /// <summary>
    /// Returns true if the current user has the Admin role.
    /// </summary>
    bool IsAdmin => Role == FamilyRole.Admin;

    /// <summary>
    /// Returns true if the current user has the Owner or Admin role.
    /// </summary>
    bool IsOwnerOrAdmin => IsOwner || IsAdmin;
}
