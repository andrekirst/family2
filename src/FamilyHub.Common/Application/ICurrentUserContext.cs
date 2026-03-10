namespace FamilyHub.Common.Application;

/// <summary>
/// Scoped service providing lazy access to the current authenticated user's context.
/// The DB call only happens when <see cref="GetCurrentUserAsync"/> is first accessed,
/// avoiding unnecessary lookups for unauthenticated or introspection requests.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    /// Gets the current authenticated user's info. Lazily resolved from the database
    /// on first access. Throws <see cref="UnauthorizedAccessException"/> if the user
    /// is not authenticated or not found in the database.
    /// </summary>
    Task<CurrentUserInfo> GetCurrentUserAsync();

    /// <summary>
    /// Gets raw JWT claim values without any database lookup.
    /// Use this for operations where the user may not exist in the database yet
    /// (e.g., user registration). Throws if the request is not authenticated.
    /// </summary>
    RawClaimsInfo GetRawClaims();

    /// <summary>
    /// Whether the current HTTP request has an authenticated identity.
    /// Does NOT trigger a database lookup.
    /// </summary>
    bool IsAuthenticated { get; }
}
