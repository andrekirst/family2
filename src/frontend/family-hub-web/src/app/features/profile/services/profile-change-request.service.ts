import { Injectable, inject, signal, computed } from '@angular/core';
import { GraphQLService, GraphQLError } from '../../../core/services/graphql.service';
import {
  ProfileChangeRequest,
  MyPendingChange,
  MyRejectedChange,
  ApproveProfileChangePayload,
  RejectProfileChangePayload,
} from '../models/profile-change-request.models';

/**
 * State for managing profile change requests (approval queue for Owner/Admin).
 */
interface ApprovalQueueState {
  pendingRequests: ProfileChangeRequest[];
  isLoading: boolean;
  error: string | null;
  lastUpdated: string | null;
}

/**
 * State for managing child's pending changes.
 */
interface MyChangesState {
  pendingChanges: MyPendingChange[];
  rejectedChanges: MyRejectedChange[];
  isLoading: boolean;
  error: string | null;
  lastUpdated: string | null;
}

/**
 * GraphQL response type for pending profile changes query.
 */
interface PendingProfileChangesResponse {
  pendingProfileChanges: {
    pendingRequests: ProfileChangeRequest[];
    totalCount: number;
  };
}

/**
 * GraphQL response type for my pending changes query.
 */
interface MyPendingChangesResponse {
  myPendingChanges: {
    pendingChanges: MyPendingChange[];
    rejectedChanges: MyRejectedChange[];
    hasPendingChanges: boolean;
  };
}

/**
 * GraphQL response type for approve mutation.
 */
interface ApproveProfileChangeMutationResponse {
  approveProfileChange: ApproveProfileChangePayload;
}

/**
 * GraphQL response type for reject mutation.
 */
interface RejectProfileChangeMutationResponse {
  rejectProfileChange: RejectProfileChangePayload;
}

/**
 * Service for managing profile change requests in the child approval workflow.
 *
 * Provides two state sets:
 * 1. Approval queue for Owner/Admin users (pending requests from children)
 * 2. My pending changes for children (their own submitted changes)
 *
 * Pattern: Follows ProfileService signal-based state pattern.
 *
 * @example
 * ```typescript
 * export class ApprovalQueueComponent {
 *   service = inject(ProfileChangeRequestService);
 *
 *   ngOnInit() {
 *     this.service.loadPendingForApproval();
 *   }
 *
 *   async approve(requestId: string) {
 *     await this.service.approve(requestId);
 *   }
 * }
 * ```
 */
@Injectable({
  providedIn: 'root',
})
export class ProfileChangeRequestService {
  private graphqlService = inject(GraphQLService);

  // ===== Approval Queue State (Owner/Admin) =====

  private approvalQueueState = signal<ApprovalQueueState>({
    pendingRequests: [],
    isLoading: false,
    error: null,
    lastUpdated: null,
  });

  /**
   * Pending profile change requests for approval (Owner/Admin only).
   */
  readonly pendingRequests = computed(() => this.approvalQueueState().pendingRequests);

  /**
   * Number of pending requests awaiting approval.
   */
  readonly pendingCount = computed(() => this.approvalQueueState().pendingRequests.length);

  /**
   * Whether pending requests are being loaded.
   */
  readonly isLoadingPending = computed(() => this.approvalQueueState().isLoading);

  /**
   * Error from approval queue operations.
   */
  readonly approvalQueueError = computed(() => this.approvalQueueState().error);

  // ===== My Changes State (Children) =====

  private myChangesState = signal<MyChangesState>({
    pendingChanges: [],
    rejectedChanges: [],
    isLoading: false,
    error: null,
    lastUpdated: null,
  });

  /**
   * Child user's pending changes awaiting approval.
   */
  readonly myPendingChanges = computed(() => this.myChangesState().pendingChanges);

  /**
   * Child user's rejected changes (with reasons).
   */
  readonly myRejectedChanges = computed(() => this.myChangesState().rejectedChanges);

  /**
   * Whether child has any pending changes awaiting approval.
   */
  readonly hasPendingChanges = computed(() => this.myChangesState().pendingChanges.length > 0);

  /**
   * Whether child has any rejected changes to review.
   */
  readonly hasRejectedChanges = computed(() => this.myChangesState().rejectedChanges.length > 0);

  /**
   * Whether my changes are being loaded.
   */
  readonly isLoadingMyChanges = computed(() => this.myChangesState().isLoading);

  /**
   * Error from my changes operations.
   */
  readonly myChangesError = computed(() => this.myChangesState().error);

  // ===== Approval Queue Methods (Owner/Admin) =====

  /**
   * Loads pending profile change requests for approval.
   * Only accessible by Owner/Admin users.
   */
  async loadPendingForApproval(): Promise<void> {
    this.approvalQueueState.update((s) => ({ ...s, isLoading: true, error: null }));

    try {
      const query = `
        query GetPendingProfileChanges {
          pendingProfileChanges {
            pendingRequests {
              id
              profileId
              requestedBy
              requestedByDisplayName
              fieldName
              oldValue
              newValue
              status
              createdAt
            }
            totalCount
          }
        }
      `;

      const response = await this.graphqlService.query<PendingProfileChangesResponse>(query);

      this.approvalQueueState.update((s) => ({
        ...s,
        pendingRequests: response.pendingProfileChanges.pendingRequests,
        lastUpdated: new Date().toISOString(),
      }));
    } catch (err) {
      this.handleError(err, 'Failed to load pending profile changes', 'approvalQueue');
    } finally {
      this.approvalQueueState.update((s) => ({ ...s, isLoading: false }));
    }
  }

  /**
   * Approves a profile change request.
   * Applies the change to the child's profile.
   *
   * @param requestId - The change request ID to approve
   * @returns Promise resolving to true on success, false on failure
   */
  async approve(requestId: string): Promise<boolean> {
    this.approvalQueueState.update((s) => ({ ...s, isLoading: true, error: null }));

    try {
      const mutation = `
        mutation ApproveProfileChange($input: ApproveProfileChangeInput!) {
          approveProfileChange(input: $input) {
            success
            requestId
            appliedFieldName
            appliedValue
            errors
          }
        }
      `;

      const response = await this.graphqlService.mutate<ApproveProfileChangeMutationResponse>(
        mutation,
        {
          input: { requestId },
        }
      );

      if (!response.approveProfileChange.success) {
        const errorMessage =
          response.approveProfileChange.errors.join(', ') || 'Failed to approve change';
        this.approvalQueueState.update((s) => ({ ...s, error: errorMessage }));
        return false;
      }

      // Remove the approved request from the list
      this.approvalQueueState.update((s) => ({
        ...s,
        pendingRequests: s.pendingRequests.filter((r) => r.id !== requestId),
      }));

      return true;
    } catch (err) {
      this.handleError(err, 'Failed to approve profile change', 'approvalQueue');
      return false;
    } finally {
      this.approvalQueueState.update((s) => ({ ...s, isLoading: false }));
    }
  }

  /**
   * Rejects a profile change request.
   * Requires a reason explaining why the change was rejected.
   *
   * @param requestId - The change request ID to reject
   * @param reason - Explanation for rejection (min 10 characters)
   * @returns Promise resolving to true on success, false on failure
   */
  async reject(requestId: string, reason: string): Promise<boolean> {
    if (reason.trim().length < 10) {
      this.approvalQueueState.update((s) => ({
        ...s,
        error: 'Rejection reason must be at least 10 characters',
      }));
      return false;
    }

    this.approvalQueueState.update((s) => ({ ...s, isLoading: true, error: null }));

    try {
      const mutation = `
        mutation RejectProfileChange($input: RejectProfileChangeInput!) {
          rejectProfileChange(input: $input) {
            success
            requestId
            errors
          }
        }
      `;

      const response = await this.graphqlService.mutate<RejectProfileChangeMutationResponse>(
        mutation,
        {
          input: { requestId, reason },
        }
      );

      if (!response.rejectProfileChange.success) {
        const errorMessage =
          response.rejectProfileChange.errors.join(', ') || 'Failed to reject change';
        this.approvalQueueState.update((s) => ({ ...s, error: errorMessage }));
        return false;
      }

      // Remove the rejected request from the list
      this.approvalQueueState.update((s) => ({
        ...s,
        pendingRequests: s.pendingRequests.filter((r) => r.id !== requestId),
      }));

      return true;
    } catch (err) {
      this.handleError(err, 'Failed to reject profile change', 'approvalQueue');
      return false;
    } finally {
      this.approvalQueueState.update((s) => ({ ...s, isLoading: false }));
    }
  }

  // ===== My Changes Methods (Children) =====

  /**
   * Loads the current user's pending and rejected changes.
   * For children to see the status of their submitted profile changes.
   */
  async loadMyChanges(): Promise<void> {
    this.myChangesState.update((s) => ({ ...s, isLoading: true, error: null }));

    try {
      const query = `
        query GetMyPendingChanges {
          myPendingChanges {
            pendingChanges {
              id
              fieldName
              oldValue
              newValue
              createdAt
            }
            rejectedChanges {
              id
              fieldName
              oldValue
              newValue
              rejectionReason
              rejectedAt
            }
            hasPendingChanges
          }
        }
      `;

      const response = await this.graphqlService.query<MyPendingChangesResponse>(query);

      this.myChangesState.update((s) => ({
        ...s,
        pendingChanges: response.myPendingChanges.pendingChanges,
        rejectedChanges: response.myPendingChanges.rejectedChanges,
        lastUpdated: new Date().toISOString(),
      }));
    } catch (err) {
      this.handleError(err, 'Failed to load your pending changes', 'myChanges');
    } finally {
      this.myChangesState.update((s) => ({ ...s, isLoading: false }));
    }
  }

  /**
   * Dismisses a rejected change notification.
   * Removes it from the rejected changes list locally.
   *
   * @param requestId - The rejected change ID to dismiss
   */
  dismissRejectedChange(requestId: string): void {
    this.myChangesState.update((s) => ({
      ...s,
      rejectedChanges: s.rejectedChanges.filter((r) => r.id !== requestId),
    }));
  }

  // ===== Shared Methods =====

  /**
   * Clears error state for approval queue.
   */
  clearApprovalQueueError(): void {
    this.approvalQueueState.update((s) => ({ ...s, error: null }));
  }

  /**
   * Clears error state for my changes.
   */
  clearMyChangesError(): void {
    this.myChangesState.update((s) => ({ ...s, error: null }));
  }

  // ===== Private Methods =====

  /**
   * Handles errors from GraphQL operations.
   *
   * @param err - Error object from GraphQL operation
   * @param fallbackMessage - Message to use if error is unknown type
   * @param target - Which state to update ('approvalQueue' or 'myChanges')
   */
  private handleError(
    err: unknown,
    fallbackMessage: string,
    target: 'approvalQueue' | 'myChanges'
  ): void {
    let message = fallbackMessage;

    if (err instanceof Error) {
      message = err.message;
    } else if (err instanceof GraphQLError) {
      message = err.message;
    }

    if (target === 'approvalQueue') {
      this.approvalQueueState.update((s) => ({ ...s, error: message }));
    } else {
      this.myChangesState.update((s) => ({ ...s, error: message }));
    }
  }
}
