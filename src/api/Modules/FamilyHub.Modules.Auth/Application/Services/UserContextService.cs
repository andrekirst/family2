using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;

namespace FamilyHub.Modules.Auth.Application.Services;

/// <summary>
/// Scoped service that holds the current authenticated user's context.
/// Populated by UserContextEnrichmentBehavior at the start of each MediatR request.
/// One instance per HTTP request - provides caching to avoid multiple User database queries.
/// </summary>
public sealed class UserContextService : IUserContext
{
    private User? _user;

    /// <summary>
    /// Gets the current authenticated user.
    /// Throws InvalidOperationException if accessed before UserContextEnrichmentBehavior has run.
    /// </summary>
    public User User
    {
        get => _user ?? throw new InvalidOperationException(
            "User context has not been initialized. " +
            "Ensure the request implements IRequireAuthentication and UserContextEnrichmentBehavior has executed.");
    }

    /// <summary>
    /// Sets the current user context. Called internally by UserContextEnrichmentBehavior.
    /// </summary>
    /// <param name="user">The authenticated user to set in context.</param>
    /// <exception cref="ArgumentNullException">Thrown if user is null.</exception>
    internal void SetUser(User user)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
    }
}
