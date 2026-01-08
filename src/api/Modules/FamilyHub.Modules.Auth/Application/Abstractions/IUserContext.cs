using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Abstractions;

/// <summary>
/// Provides access to the current authenticated user's context within a MediatR request.
/// Automatically populated by UserContextEnrichmentBehavior for requests marked with IRequireAuthentication.
/// Contains the full User aggregate to avoid multiple database fetches.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the full User aggregate for the current authenticated user.
    /// Only available after UserContextEnrichmentBehavior has executed.
    /// Throws InvalidOperationException if accessed before enrichment.
    /// </summary>
    User User { get; }

    // Convenience properties for common access patterns
    UserId UserId => User.Id;
    FamilyId FamilyId => User.FamilyId;
    FamilyRole Role => User.Role;
    Email Email => User.Email;

    // Role helper methods
    bool IsOwner => User.Role == FamilyRole.Owner;
    bool IsAdmin => User.Role == FamilyRole.Admin;
    bool IsOwnerOrAdmin => IsOwner || IsAdmin;
}
