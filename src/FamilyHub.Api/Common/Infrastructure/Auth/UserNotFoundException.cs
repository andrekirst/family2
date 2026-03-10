using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Common.Infrastructure.Auth;

/// <summary>
/// Thrown when the authenticated user (identified by external OAuth ID)
/// does not have a corresponding record in the application database.
/// </summary>
public sealed class UserNotFoundException()
    : DomainException(
        "User not found. Please complete registration first.",
        DomainErrorCodes.UserNotFound);
