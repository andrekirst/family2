using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Common.Application;

/// <summary>
/// Lightweight record containing raw JWT claim values extracted from the token.
/// Unlike <see cref="CurrentUserInfo"/>, this does NOT require a database lookup
/// and can be used for operations where the user may not yet exist (e.g., registration).
/// </summary>
public sealed record RawClaimsInfo(
    ExternalUserId ExternalUserId,
    string Email,
    bool EmailVerified,
    string? UserName);
