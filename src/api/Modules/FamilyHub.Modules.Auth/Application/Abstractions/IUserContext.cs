using FamilyHub.Modules.Auth.Domain;

namespace FamilyHub.Modules.Auth.Application.Abstractions;

/// <summary>
/// Provides access to the current authenticated user's context within a MediatR request.
/// This interface extends the SharedKernel IUserContext and adds Auth-specific User aggregate access.
/// Automatically populated by UserContextEnrichmentBehavior for requests marked with IRequireAuthentication.
/// Contains the full User aggregate to avoid multiple database fetches.
/// </summary>
public interface IUserContext : FamilyHub.SharedKernel.Application.Abstractions.IUserContext
{
    /// <summary>
    /// Gets the full User aggregate for the current authenticated user.
    /// Only available after UserContextEnrichmentBehavior has executed.
    /// Throws InvalidOperationException if accessed before enrichment.
    /// </summary>
    User User { get; }
}
