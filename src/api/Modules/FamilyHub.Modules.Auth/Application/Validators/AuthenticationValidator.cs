using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Validators;

/// <summary>
/// Centralized validator for authentication requirements.
/// Validates that a user is authenticated before performing operations.
/// </summary>
public static class AuthenticationValidator
{
    /// <summary>
    /// Validates that a userId is present (user is authenticated).
    /// </summary>
    /// <param name="userId">The nullable UserId from ICurrentUserService</param>
    /// <param name="operationName">Name of the operation being performed (for error messages)</param>
    /// <returns>The validated UserId</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when userId is null</exception>
    public static UserId RequireAuthentication(UserId? userId, string operationName)
    {
        if (userId == null)
        {
            throw new UnauthorizedAccessException(
                $"You must be authenticated to {operationName}.");
        }

        return userId.Value;
    }

    /// <summary>
    /// Validates that an email is present (user has verified email).
    /// </summary>
    /// <param name="email">The nullable Email from user context</param>
    /// <param name="operationName">Name of the operation being performed (for error messages)</param>
    /// <returns>The validated Email</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when email is null</exception>
    public static Email RequireEmail(Email? email, string operationName)
    {
        if (email == null)
        {
            throw new UnauthorizedAccessException(
                $"Email required. You must have a verified email to {operationName}.");
        }

        return email.Value;
    }
}
