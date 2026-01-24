/**
 * Data interface for invite members wizard step.
 * Represents the data collected in the invite members step of family creation wizard.
 */
export interface InviteMembersStepData {
  /**
   * List of email invitations to send.
   * Empty array if no invitations.
   */
  invitations: EmailInvitation[];

  /**
   * Optional personal message to include in all invitation emails.
   * Max 500 characters.
   */
  message: string;
}

/**
 * Single email invitation with role assignment.
 */
export interface EmailInvitation {
  /**
   * Email address of the person to invite.
   * Must be valid email format.
   */
  email: string;

  /**
   * Role to assign to the invited member.
   */
  role: UserRole;
}

/**
 * Available roles for family members.
 * - ADMIN: Full access to family management
 * - MEMBER: Standard access to family features
 * - CHILD: Limited access (currently mapped to MEMBER in backend)
 */
export type UserRole = 'ADMIN' | 'MEMBER' | 'CHILD';

/**
 * Result of batch invitation operation.
 * Provides success count and error details for partial success scenarios.
 */
export interface InvitationResult {
  /**
   * Number of successfully sent invitations.
   */
  successCount: number;

  /**
   * Number of failed invitations.
   */
  failedCount: number;

  /**
   * List of errors for failed invitations.
   */
  errors: InvitationError[];
}

/**
 * Error details for a failed invitation.
 */
export interface InvitationError {
  /**
   * Email address that failed.
   */
  email: string;

  /**
   * Human-readable error message.
   */
  message: string;

  /**
   * Optional error code for programmatic handling.
   */
  code?: string;
}
