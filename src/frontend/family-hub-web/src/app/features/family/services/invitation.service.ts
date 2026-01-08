import { Injectable, inject, signal } from '@angular/core';
import { GraphQLService, GraphQLError } from '../../../core/services/graphql.service';
import { PendingInvitation, UserRole } from '../models/family.models';

/**
 * GraphQL response for GetPendingInvitations query.
 */
interface GetPendingInvitationsResponse {
  invitations: {
    pending: PendingInvitation[];
  };
}

/**
 * Error types from Hot Chocolate Mutation Conventions (discriminated union)
 */
type InvitationMutationError =
  | { __typename: 'ValidationError'; message: string; field: string }
  | { __typename: 'BusinessError'; message: string; code: string }
  | { __typename: 'ValueObjectError'; message: string };

/**
 * GraphQL response for InviteFamilyMemberByEmail mutation.
 * Updated for Hot Chocolate v14 Mutation Conventions.
 */
interface InviteFamilyMemberByEmailResponse {
  inviteFamilyMemberByEmail: {
    pendingInvitationType: PendingInvitation | null;
    errors: InvitationMutationError[];
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
 * Updated for Hot Chocolate v14 Mutation Conventions.
 */
interface CancelInvitationResponse {
  cancelInvitation: {
    // CancelInvitation returns bool directly (not in DTO wrapper)
    // Hot Chocolate wraps it in errors array
    errors: InvitationMutationError[];
  };
}

/**
 * GraphQL response for UpdateInvitationRole mutation.
 * Updated for Hot Chocolate v14 Mutation Conventions.
 * Note: Field is 'updatedInvitation' (not 'updatedInvitationDto') because Hot Chocolate
 * removes 'Dto' suffix and converts to camelCase.
 */
interface UpdateInvitationRoleResponse {
  updateInvitationRole: {
    updatedInvitation: { invitationId: string; role: UserRole } | null;
    errors: InvitationMutationError[];
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
  providedIn: 'root',
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
  async loadPendingInvitations(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);

    try {
      const query = `
        query GetPendingInvitations {
          invitations {
            pending {
              id
              email
              role
              status
              invitedAt
              expiresAt
              displayCode
            }
          }
        }
      `;

      const response = await this.graphqlService.query<GetPendingInvitationsResponse>(query);

      this.pendingInvitations.set(response.invitations.pending);
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
      errors: [],
    };

    try {
      // Send each invitation individually
      for (const { email, role } of invitations) {
        try {
          const mutation = `
            mutation InviteFamilyMemberByEmail($input: InviteFamilyMemberByEmailInput!) {
              inviteFamilyMemberByEmail(input: $input) {
                pendingInvitationType {
                  id
                  email
                  role
                  status
                  invitedAt
                  expiresAt
                  displayCode
                }
                errors {
                  __typename
                  ... on ValidationError {
                    message
                    field
                  }
                  ... on BusinessError {
                    message
                    code
                  }
                  ... on ValueObjectError {
                    message
                  }
                }
              }
            }
          `;

          const response = await this.graphqlService.mutate<InviteFamilyMemberByEmailResponse>(
            mutation,
            { input: { familyId, email, role } }
          );

          // Check for business logic errors
          if (
            response.inviteFamilyMemberByEmail.errors &&
            response.inviteFamilyMemberByEmail.errors.length > 0
          ) {
            const error = response.inviteFamilyMemberByEmail.errors[0];
            result.errors.push({
              message: error.message,
              code:
                '__typename' in error && error.__typename === 'BusinessError'
                  ? error.code
                  : undefined,
              field: email,
            });
          } else if (response.inviteFamilyMemberByEmail.pendingInvitationType) {
            result.successCount++;
          }
        } catch (err) {
          result.errors.push({
            message: err instanceof Error ? err.message : 'Failed to send invitation',
            field: email,
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
            errors {
              __typename
              ... on ValidationError {
                message
                field
              }
              ... on BusinessError {
                message
                code
              }
              ... on ValueObjectError {
                message
              }
            }
          }
        }
      `;

      const response = await this.graphqlService.mutate<CancelInvitationResponse>(mutation, {
        input: { invitationId },
      });

      // Check for business logic errors
      if (response.cancelInvitation.errors && response.cancelInvitation.errors.length > 0) {
        throw new Error(response.cancelInvitation.errors[0].message);
      }

      // Remove from local state (success if no errors)
      this.pendingInvitations.update((invitations) =>
        invitations.filter((inv) => inv.id !== invitationId)
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
            updatedInvitation {
              invitationId
              role
            }
            errors {
              __typename
              ... on ValidationError {
                message
                field
              }
              ... on BusinessError {
                message
                code
              }
              ... on ValueObjectError {
                message
              }
            }
          }
        }
      `;

      const response = await this.graphqlService.mutate<UpdateInvitationRoleResponse>(mutation, {
        input: { invitationId, newRole },
      });

      // Check for business logic errors
      if (response.updateInvitationRole.errors && response.updateInvitationRole.errors.length > 0) {
        throw new Error(response.updateInvitationRole.errors[0].message);
      }

      // Update local state
      if (response.updateInvitationRole.updatedInvitation) {
        this.pendingInvitations.update((invitations) =>
          invitations.map((inv) =>
            inv.id === invitationId
              ? { ...inv, role: response.updateInvitationRole.updatedInvitation!.role }
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
