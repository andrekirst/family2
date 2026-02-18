namespace FamilyHub.Common.Domain;

/// <summary>
/// Stable error code constants used as localization keys in DomainException.
/// These codes are mapped to localized messages at the GraphQL error filter boundary.
/// Convention: SCREAMING_SNAKE_CASE, grouped by module.
/// </summary>
public static class DomainErrorCodes
{
    // Auth
    public const string UserAlreadyAssignedToFamily = "USER_ALREADY_ASSIGNED_TO_FAMILY";
    public const string UserNotAssignedToFamily = "USER_NOT_ASSIGNED_TO_FAMILY";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string UserAlreadyExists = "USER_ALREADY_EXISTS";

    // Family
    public const string UserAlreadyOwnsFamily = "USER_ALREADY_OWNS_FAMILY";
    public const string FamilyCreationFailed = "FAMILY_CREATION_FAILED";

    // Family — Invitations
    public const string InvitationNotFound = "INVITATION_NOT_FOUND";
    public const string InvitationExpired = "INVITATION_EXPIRED";
    public const string InvitationInvalidStatusForAccept = "INVITATION_INVALID_STATUS_FOR_ACCEPT";
    public const string InvitationInvalidStatusForDecline = "INVITATION_INVALID_STATUS_FOR_DECLINE";
    public const string InvitationInvalidStatusForRevoke = "INVITATION_INVALID_STATUS_FOR_REVOKE";
    public const string InvalidInvitationToken = "INVALID_INVITATION_TOKEN";
    public const string InvitationEmailMismatch = "INVITATION_EMAIL_MISMATCH";
    public const string AlreadyFamilyMember = "ALREADY_FAMILY_MEMBER";
    public const string InsufficientPermissionToSendInvitation = "INSUFFICIENT_PERMISSION_TO_SEND_INVITATION";
    public const string InsufficientPermissionToRevokeInvitation = "INSUFFICIENT_PERMISSION_TO_REVOKE_INVITATION";
    public const string DuplicateInvitation = "DUPLICATE_INVITATION";
    public const string MustBeFamilyMemberToSendInvitation = "MUST_BE_FAMILY_MEMBER_TO_SEND_INVITATION";

    // Calendar
    public const string CalendarEventNotFound = "CALENDAR_EVENT_NOT_FOUND";
    public const string EventAlreadyCancelled = "EVENT_ALREADY_CANCELLED";
    public const string CannotUpdateCancelledEvent = "CANNOT_UPDATE_CANCELLED_EVENT";
    public const string MustBeFamilyMemberToCreateEvent = "MUST_BE_FAMILY_MEMBER_TO_CREATE_EVENT";

    // File Management
    public const string NotFound = "NOT_FOUND";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";
    public const string FileNotFound = "FILE_NOT_FOUND";
    public const string FolderNotFound = "FOLDER_NOT_FOUND";
    public const string TagNotFound = "TAG_NOT_FOUND";
    public const string AlbumNotFound = "ALBUM_NOT_FOUND";
    public const string OrganizationRuleNotFound = "ORGANIZATION_RULE_NOT_FOUND";
    public const string InboxFolderNotFound = "INBOX_FOLDER_NOT_FOUND";
    public const string FileVersionNotFound = "FILE_VERSION_NOT_FOUND";

    // EventChain
    public const string ChainDefinitionNotFound = "CHAIN_DEFINITION_NOT_FOUND";
    public const string ChainExecutionNotFound = "CHAIN_EXECUTION_NOT_FOUND";
    public const string DuplicateStepAlias = "DUPLICATE_STEP_ALIAS";
    public const string UnknownTriggerEventType = "UNKNOWN_TRIGGER_EVENT_TYPE";
    public const string UnknownActionType = "UNKNOWN_ACTION_TYPE";
    public const string UserNotInFamily = "USER_NOT_IN_FAMILY";
}
