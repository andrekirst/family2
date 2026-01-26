/**
 * Profile Change Request Models
 *
 * TypeScript interfaces for the child approval workflow feature.
 * When a user with "Child" role modifies their profile, changes must be
 * approved by parents (Owner/Admin).
 */

/**
 * Status of a profile change request.
 */
export type ChangeRequestStatus = 'PENDING' | 'APPROVED' | 'REJECTED';

/**
 * Full profile change request details (for approval queue).
 */
export interface ProfileChangeRequest {
  id: string;
  profileId: string;
  requestedBy: string;
  requestedByDisplayName: string;
  fieldName: string;
  oldValue: string | null;
  newValue: string;
  status: ChangeRequestStatus;
  reviewedBy: string | null;
  reviewedAt: string | null;
  rejectionReason: string | null;
  createdAt: string;
}

/**
 * Pending change for a child user to see their submitted changes.
 */
export interface MyPendingChange {
  id: string;
  fieldName: string;
  oldValue: string | null;
  newValue: string;
  createdAt: string;
}

/**
 * Rejected change for a child user to see why their change was rejected.
 */
export interface MyRejectedChange {
  id: string;
  fieldName: string;
  oldValue: string | null;
  newValue: string;
  rejectionReason: string;
  rejectedAt: string;
}

/**
 * Result from pending profile changes query (for Owner/Admin).
 */
export interface PendingProfileChangesResult {
  pendingRequests: ProfileChangeRequest[];
  totalCount: number;
}

/**
 * Result from my pending changes query (for children).
 */
export interface MyPendingChangesResult {
  pendingChanges: MyPendingChange[];
  rejectedChanges: MyRejectedChange[];
  hasPendingChanges: boolean;
}

/**
 * Input for approving a profile change request.
 */
export interface ApproveProfileChangeInput {
  requestId: string;
}

/**
 * Input for rejecting a profile change request.
 */
export interface RejectProfileChangeInput {
  requestId: string;
  reason: string;
}

/**
 * Payload returned after approving a profile change.
 */
export interface ApproveProfileChangePayload {
  success: boolean;
  requestId: string;
  appliedFieldName: string;
  appliedValue: string;
  errors: string[];
}

/**
 * Payload returned after rejecting a profile change.
 */
export interface RejectProfileChangePayload {
  success: boolean;
  requestId: string;
  errors: string[];
}

/**
 * Maps field names to user-friendly display labels.
 */
export const FIELD_NAME_LABELS: Record<string, string> = {
  DisplayName: 'Display Name',
  FirstName: 'First Name',
  LastName: 'Last Name',
  NickName: 'Nickname',
  PreferredLanguage: 'Preferred Language',
  Timezone: 'Timezone',
  Bio: 'Bio',
  AvatarUrl: 'Avatar URL',
};

/**
 * Gets a user-friendly label for a field name.
 */
export function getFieldLabel(fieldName: string): string {
  return FIELD_NAME_LABELS[fieldName] ?? fieldName;
}
