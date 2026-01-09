namespace FamilyHub.Modules.Auth.Application.Services;

/// <summary>
/// Centralized cache key builder for validation cache.
/// Ensures consistent key format across validators and handlers.
/// </summary>
/// <remarks>
/// Cache keys follow the pattern: "{EntityType}:{EntityId}"
/// as documented in <see cref="Abstractions.IValidationCache"/>.
///
/// Using this builder prevents cache key mismatches between validators (Set)
/// and handlers (Get) by centralizing key construction.
/// </remarks>
public static class CacheKeyBuilder
{
    /// <summary>
    /// Builds cache key for FamilyMemberInvitation entity.
    /// </summary>
    /// <param name="token">Invitation token value.</param>
    /// <returns>Cache key in format "FamilyMemberInvitation:{token}"</returns>
    public static string FamilyMemberInvitation(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));
        return $"FamilyMemberInvitation:{token}";
    }

    /// <summary>
    /// Builds cache key for Family entity.
    /// </summary>
    /// <param name="familyId">Family ID.</param>
    /// <returns>Cache key in format "Family:{familyId}"</returns>
    public static string Family(Guid familyId)
    {
        if (familyId == Guid.Empty)
        {
            throw new ArgumentException("Family ID cannot be empty.", nameof(familyId));
        }
        return $"Family:{familyId}";
    }

    // Future extension points (commented out until needed):
    // public static string User(Guid userId) => $"User:{userId}";
    // public static string Invitation(Guid invitationId) => $"Invitation:{invitationId}";
}
