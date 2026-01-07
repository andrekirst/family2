import { Injectable, inject, signal, computed } from '@angular/core';
import { GraphQLService, GraphQLError } from '../../../core/services/graphql.service';
import { FamilyMember } from '../models/family.models';

/**
 * Family domain model matching backend GraphQL schema.
 */
export interface Family {
  id: string;
  name: string;
  auditInfo: {
    createdAt: string;
    updatedAt: string;
  };
}

/**
 * GraphQL response type for family query (root level).
 */
interface GetCurrentFamilyResponse {
  family: Family | null;
}

/**
 * GraphQL response type for GetFamilyMembers query.
 */
interface GetFamilyMembersResponse {
  familyMembers: FamilyMember[];
}

/**
 * GraphQL response type for createFamily mutation.
 * Updated for Hot Chocolate v14 Mutation Conventions.
 */
interface CreateFamilyResponse {
  createFamily: {
    createdFamily: Family | null;
    errors: (
      | { __typename: 'ValidationError'; message: string; field: string }
      | { __typename: 'BusinessError'; message: string; code: string }
      | { __typename: 'ValueObjectError'; message: string }
    )[];
  };
}

/**
 * Service for managing family data with Angular Signals.
 * Provides reactive state management for family operations.
 *
 * @example
 * ```typescript
 * export class DashboardComponent {
 *   familyService = inject(FamilyService);
 *
 *   ngOnInit() {
 *     this.familyService.loadUserFamilies();
 *   }
 * }
 * ```
 */
@Injectable({
  providedIn: 'root',
})
export class FamilyService {
  private graphqlService = inject(GraphQLService);

  /**
   * Signal holding the current active family.
   * Null when user has no family.
   */
  currentFamily = signal<Family | null>(null);

  /**
   * Signal holding all members of the current family.
   * Empty array when family has no members or not loaded yet.
   */
  familyMembers = signal<FamilyMember[]>([]);

  /**
   * Signal indicating whether an async operation is in progress.
   * Used to show loading spinners.
   */
  isLoading = signal<boolean>(false);

  /**
   * Signal holding error message from last operation.
   * Null when no error.
   */
  error = signal<string | null>(null);

  /**
   * Computed signal indicating whether user has a family.
   * Reactively updates when currentFamily changes.
   */
  hasFamily = computed(() => this.currentFamily() !== null);

  /**
   * Loads the current user's active family from backend.
   * If user has no families, currentFamily remains null.
   *
   * @returns Promise that resolves when load completes
   */
  async loadCurrentFamily(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);

    try {
      const query = `
        query GetCurrentFamily {
          family {
            id
            name
            auditInfo {
              createdAt
              updatedAt
            }
          }
        }
      `;

      const response = await this.graphqlService.query<GetCurrentFamilyResponse>(query);

      // Set current family (or null if user has no families)
      this.currentFamily.set(response.family);
    } catch (err) {
      this.handleError(err, 'Failed to load family');
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Creates a new family with the given name.
   * On success, sets currentFamily to the newly created family.
   *
   * @param name - Family name (1-50 characters)
   * @returns Promise that resolves when creation completes
   */
  async createFamily(name: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);

    try {
      const mutation = `
        mutation CreateFamily($input: CreateFamilyInput!) {
          createFamily(input: $input) {
            createdFamily {
              id
              name
              createdAt
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

      const response = await this.graphqlService.mutate<CreateFamilyResponse>(mutation, {
        input: { name },
      });

      // Check for business logic errors (e.g., user already has family)
      if (response.createFamily.errors && response.createFamily.errors.length > 0) {
        this.error.set(response.createFamily.errors[0].message);
        return;
      }

      // Set currentFamily to newly created family
      if (response.createFamily.createdFamily) {
        this.currentFamily.set(response.createFamily.createdFamily);
      }
    } catch (err) {
      this.handleError(err, 'Failed to create family');
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Loads all members of the specified family.
   *
   * @param familyId - UUID of the family
   * @returns Promise that resolves when load completes
   */
  async loadFamilyMembers(familyId: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);

    try {
      const query = `
        query GetFamilyMembers($familyId: UUID!) {
          familyMembers(familyId: $familyId) {
            id
            email
            emailVerified
            role
            auditInfo {
              createdAt
              updatedAt
            }
          }
        }
      `;

      const response = await this.graphqlService.query<GetFamilyMembersResponse>(query, {
        familyId,
      });

      this.familyMembers.set(response.familyMembers);
    } catch (err) {
      this.handleError(err, 'Failed to load family members');
    } finally {
      this.isLoading.set(false);
    }
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
