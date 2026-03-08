using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Common.Infrastructure.Auth;

/// <summary>
/// Thrown when a request requires authentication but the user identity
/// could not be determined (missing or invalid JWT "sub" claim).
/// </summary>
public sealed class UserNotAuthenticatedException()
    : DomainException(
        "Authentication is required to perform this action.",
        DomainErrorCodes.UserNotAuthenticated);
