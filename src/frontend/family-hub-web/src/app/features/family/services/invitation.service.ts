import { Injectable, inject, signal } from '@angular/core';
import { GraphQLService, GraphQLError } from '../../../core/services/graphql.service';
import { PendingInvitation } from '../models/family.models';

/**
 * GraphQL response for GetPendingInvitations query.
 */
interface GetPendingInvitationsResponse {
  pendingInvitations: PendingInvitation[];
}

/**
 * GraphQL response for InviteFamilyMemberByEmail mutation.
 */
interface InviteFamilyMemberByEmailResponse {
  inviteFamilyMemberByEmail: {
    invitation: PendingInvitation | null;
    errors: { message: string; code?: string }[] | null;
  };
}

/**
 * Result of inviting family members by email.
 */
interface InviteEmailResult {
  successCount: number;
  errors: { message: string; code?: string; field?: string }[];
}

/**
 * GraphQL response for CancelInvitation mutation.
 */
interface CancelInvitationResponse {
  cancelInvitation: {
    success: boolean;
    errors: { message: string; code?: string }[] | null;
  };
}

/**
 * GraphQL response for UpdateInvitationRole mutation.
 */
interface UpdateInvitationRoleResponse {
  updateInvitationRole: {
    invitation: PendingInvitation | null;
    errors: { message: string; code?: string }[] | null;
  };
}

/**
 * Service for managing family member invitations.
 * Handles email-based invitations only.
 *
 * @example
 * ```typescript
 * export class FamilyManagementPage {
 *   invitationService = inject(InvitationService);
 *
 *   ngOnInit() {
 *     this.invitationService.loadPendingInvitations(this.familyId);
 *   }
 * }
 * ```
 */
@Injectable({
  providedIn: 'root'
})
export class InvitationService {
  private graphqlService = inject(GraphQLService);

  /**
   * Signal holding pending invitations for current family.
   * Empty array when no pending invitations.
   */
  pendingInvitations = signal<PendingInvitation[]>([]);

  /**
   * Signal indicating whether an async operation is in progress.
   */
  isLoading = signal<boolean>(false);

  /**
   * Signal holding error message from last operation.
   * Null when no error.
   */
  error = signal<string | null>(null);

  /**
   * Loads pending invitations for the specified family.
   *
   * @param familyId - UUID of the family
   * @returns Promise that resolves when load completes
   */
  async loadPendingInvitations(familyId: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);

    try {
      const query = `
        query GetPendingInvitations($familyId: UUID!) {
          pendingInvitations(familyId: $familyId) {
            id
            email
            role
            status
            invitedAt
            expiresAt
            displayCode
          }
        }
      `;

      const response = await this.graphqlService.query<GetPendingInvitationsResponse>(
        query,
        { familyId }
      );

      this.pendingInvitations.set(response.pendingInvitations);
    } catch (err) {
      this.handleError(err, 'Failed to load pending invitations');
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Invites family members via email.
   *
   * @param familyId - UUID of the family
   * @param invitations - Array of email-role pairs
   * @returns Promise<InviteEmailResult> containing success count and errors
   */
  async inviteFamilyMembersByEmail(
    familyId: string,
    invitations: { email: string; role: 'ADMIN' | 'MEMBER' }[]
  ): Promise<InviteEmailResult> {
    this.isLoading.set(true);
    this.error.set(null);

    const result: InviteEmailResult = {
      successCount: 0,
      errors: []
    };

    try {
      // Send each invitation individually
      for (const { email, role } of invitations) {
        try {
          const mutation = `
            mutation InviteFamilyMemberByEmail($input: InviteFamilyMemberByEmailInput!) {
              inviteFamilyMemberByEmail(input: $input) {
                invitation {
                  id
                  email
                  role
                  status
                  invitedAt
                  expiresAt
                  displayCode
                }
                errors {
                  message
                  code
                }
              }
            }
          `;

          const response = await this.graphqlService.mutate<InviteFamilyMemberByEmailResponse>(
            mutation,
            { input: { familyId, email, role } }
          );

          // Check for business logic errors
          if (response.inviteFamilyMemberByEmail.errors &&
              response.inviteFamilyMemberByEmail.errors.length > 0) {
            result.errors.push({
              message: response.inviteFamilyMemberByEmail.errors[0].message,
              code: response.inviteFamilyMemberByEmail.errors[0].code,
              field: email
            });
          } else if (response.inviteFamilyMemberByEmail.invitation) {
            result.successCount++;
          }
        } catch (err) {
          result.errors.push({
            message: err instanceof Error ? err.message : 'Failed to send invitation',
            field: email
          });
        }
      }

      return result;
    } catch (err) {
      this.handleError(err, 'Failed to invite members');
      throw err;
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Cancels a pending invitation.
   *
   * @param invitationId - UUID of the invitation to cancel
   * @returns Promise that resolves when cancellation completes
   */
  async cancelInvitation(invitationId: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);

    try {
      const mutation = `
        mutation CancelInvitation($input: CancelInvitationInput!) {
          cancelInvitation(input: $input) {
            success
            errors {
              message
              code
            }
          }
        }
      `;

      const response = await this.graphqlService.mutate<CancelInvitationResponse>(
        mutation,
        { input: { invitationId } }
      );

      // Check for business logic errors
      if (response.cancelInvitation.errors &&
          response.cancelInvitation.errors.length > 0) {
        throw new Error(response.cancelInvitation.errors[0].message);
      }

      // Remove from local state
      this.pendingInvitations.update(invitations =>
        invitations.filter(inv => inv.id !== invitationId)
      );
    } catch (err) {
      this.handleError(err, 'Failed to cancel invitation');
      throw err;
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Updates the role of a pending invitation.
   *
   * @param invitationId - UUID of the invitation
   * @param newRole - New role to assign
   * @returns Promise that resolves when update completes
   */
  async updateInvitationRole(invitationId: string, newRole: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);

    try {
      const mutation = `
        mutation UpdateInvitationRole($input: UpdateInvitationRoleInput!) {
          updateInvitationRole(input: $input) {
            invitation {
              id
              role
            }
            errors {
              message
              code
            }
          }
        }
      `;

      const response = await this.graphqlService.mutate<UpdateInvitationRoleResponse>(
        mutation,
        { input: { invitationId, newRole } }
      );

      // Check for business logic errors
      if (response.updateInvitationRole.errors &&
          response.updateInvitationRole.errors.length > 0) {
        throw new Error(response.updateInvitationRole.errors[0].message);
      }

      // Update local state
      if (response.updateInvitationRole.invitation) {
        this.pendingInvitations.update(invitations =>
          invitations.map(inv =>
            inv.id === invitationId
              ? { ...inv, role: response.updateInvitationRole.invitation!.role }
              : inv
          )
        );
      }
    } catch (err) {
      this.handleError(err, 'Failed to update invitation role');
      throw err;
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Clears error state.
   */
  clearError(): void {
    this.error.set(null);
  }

  /**
   * Handles errors from GraphQL operations.
   * Sets error signal with appropriate message.
   *
   * @param err - Error object from GraphQL operation
   * @param fallbackMessage - Message to use if error is unknown type
   */
  private handleError(err: unknown, fallbackMessage: string): void {
    if (err instanceof Error) {
      this.error.set(err.message);
    } else if (err instanceof GraphQLError) {
      this.error.set(err.message);
    } else {
      this.error.set(fallbackMessage);
    }
  }
}
