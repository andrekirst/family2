namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL enum for invitation-specific error codes.
/// Provides machine-readable error codes for invitation mutations.
/// </summary>
public enum InvitationErrorCode
{
    /// <summary>
    /// Input validation failed (e.g., invalid email format, username too short).
    /// </summary>
    VALIDATION_FAILED,

    /// <summary>
    /// Email address is already a member of this family or has pending invitation.
    /// </summary>
    DUPLICATE_EMAIL,

    /// <summary>
    /// Username is already taken within this family.
    /// </summary>
    DUPLICATE_USERNAME,

    /// <summary>
    /// Email format is invalid (does not match email pattern).
    /// </summary>
    INVALID_EMAIL_FORMAT,

    /// <summary>
    /// Username format is invalid (must be 3-20 alphanumeric characters or underscores).
    /// </summary>
    INVALID_USERNAME_FORMAT,

    /// <summary>
    /// Zitadel API returned an error during user creation.
    /// </summary>
    ZITADEL_API_ERROR,

    /// <summary>
    /// The specified family was not found.
    /// </summary>
    FAMILY_NOT_FOUND,

    /// <summary>
    /// User is not authorized to perform this action (e.g., not OWNER or ADMIN).
    /// </summary>
    UNAUTHORIZED,

    /// <summary>
    /// Rate limit exceeded for invitation attempts (10 per hour per IP).
    /// </summary>
    RATE_LIMIT_EXCEEDED,

    /// <summary>
    /// Batch size exceeds configured maximum (default: 20 invitations per batch).
    /// </summary>
    BATCH_SIZE_EXCEEDED,

    /// <summary>
    /// Invitation not found or does not belong to this family.
    /// </summary>
    INVITATION_NOT_FOUND,

    /// <summary>
    /// Invitation has already expired and cannot be modified.
    /// </summary>
    INVITATION_EXPIRED,

    /// <summary>
    /// Invitation has already been accepted and cannot be modified.
    /// </summary>
    INVITATION_ALREADY_ACCEPTED,

    /// <summary>
    /// Password strength configuration is invalid (e.g., length out of range 12-32).
    /// </summary>
    INVALID_PASSWORD_CONFIG,

    /// <summary>
    /// Invitation token is invalid or malformed.
    /// </summary>
    INVALID_TOKEN,

    /// <summary>
    /// Person name is required for managed account creation.
    /// </summary>
    PERSON_NAME_REQUIRED,

    /// <summary>
    /// Role is invalid or not allowed (e.g., cannot invite as OWNER).
    /// </summary>
    INVALID_ROLE
}
