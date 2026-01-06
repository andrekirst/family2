/**
 * Family domain models matching backend GraphQL schema.
 * These interfaces provide type safety for family-related operations.
 */

/**
 * User role within a family.
 * Maps to backend UserRoleType enum.
 */
export type UserRole = 'OWNER' | 'ADMIN' | 'MEMBER';

/**
 * Invitation status.
 * Maps to backend InvitationStatusType enum.
 */
export type InvitationStatus = 'PENDING' | 'ACCEPTED' | 'REJECTED' | 'CANCELLED' | 'EXPIRED';

/**
 * Rich metadata for a user role.
 * Fetched from backend ReferenceDataQueries.
 * Provides value, label, description, and styling for UI display.
 */
export interface RoleMetadata {
  /** GraphQL enum value (OWNER, ADMIN, MEMBER, MANAGED_ACCOUNT) */
  value: UserRole;
  /** Human-readable label for UI display (e.g., "Admin", "Member") */
  label: string;
  /** Description of role permissions and responsibilities */
  description: string;
  /** Tailwind CSS class for badge styling (e.g., "bg-purple-100 text-purple-800") */
  badgeColorClass?: string;
}

/**
 * Audit information for tracking entity creation and updates.
 */
export interface AuditInfo {
  createdAt: string;
  updatedAt: string;
}

/**
 * Family member with full user details.
 * Returned from GetFamilyMembers query.
 */
export interface FamilyMember {
  id: string;
  email: string;
  emailVerified: boolean;
  role: UserRole;
  auditInfo: AuditInfo;
}

/**
 * Pending invitation (not yet accepted).
 * Returned from GetPendingInvitations query.
 * Email-based invitations only.
 */
export interface PendingInvitation {
  id: string;
  email: string;          // Email address of invitee (required)
  role: UserRole;
  status: InvitationStatus;
  invitedAt: string;
  expiresAt: string;
  displayCode?: string;   // User-friendly code for debugging/support
}

/**
 * Created user (returned from mutations).
 * Maps to backend UserType (different from FamilyMemberType used in queries).
 * NOTE: UserType does NOT include role, username, or name.
 */
export interface CreatedUser {
  id: string;
  email: string;
  emailVerified: boolean;
  familyId: string;
  auditInfo: AuditInfo;
}

/**
 * Input for inviting a member via email.
 */
export interface EmailInvitationInput {
  email: string;
  role: 'ADMIN' | 'MEMBER';  // Cannot invite as OWNER
  message?: string;
}

/**
 * Role display names for UI.
 * @deprecated Use RoleService.getRoleMetadata() instead for rich metadata from API.
 */
export const ROLE_LABELS: Record<UserRole, string> = {
  OWNER: 'Owner',
  ADMIN: 'Admin',
  MEMBER: 'Member'
};

/**
 * Role colors for badges (Tailwind classes).
 * @deprecated Use RoleService.getRoleMetadata() instead for rich metadata from API.
 */
export const ROLE_COLORS: Record<UserRole, string> = {
  OWNER: 'bg-purple-100 text-purple-800',
  ADMIN: 'bg-blue-100 text-blue-800',
  MEMBER: 'bg-green-100 text-green-800'
};

/**
 * Status colors for invitation badges (Tailwind classes).
 */
export const STATUS_COLORS: Record<InvitationStatus, string> = {
  PENDING: 'bg-yellow-100 text-yellow-800',
  ACCEPTED: 'bg-green-100 text-green-800',
  REJECTED: 'bg-red-100 text-red-800',
  CANCELLED: 'bg-gray-100 text-gray-800',
  EXPIRED: 'bg-orange-100 text-orange-800'
};
