import { Injectable, inject, signal, computed } from '@angular/core';
import { GraphQLService, GraphQLError } from '../../../core/services/graphql.service';

/**
 * Family domain model matching backend GraphQL schema.
 */
export interface Family {
  familyId: { value: string };
  name: string;
  memberCount: number;
  createdAt: string;
}

/**
 * GraphQL response type for getUserFamilies query.
 */
interface GetUserFamiliesResponse {
  getUserFamilies: {
    families: Family[];
  };
}

/**
 * GraphQL response type for createFamily mutation.
 */
interface CreateFamilyResponse {
  createFamily: {
    family: Family | null;
    errors: Array<{ message: string; code?: string }> | null;
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
  providedIn: 'root'
})
export class FamilyService {
  private graphqlService = inject(GraphQLService);

  /**
   * Signal holding the current active family.
   * Null when user has no family.
   */
  currentFamily = signal<Family | null>(null);

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
   * Loads user's families from backend and sets currentFamily to first family.
   * If user has no families, currentFamily remains null.
   *
   * @returns Promise that resolves when load completes
   */
  async loadUserFamilies(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);

    try {
      const query = `
        query GetUserFamilies {
          getUserFamilies {
            families {
              familyId { value }
              name
              memberCount
              createdAt
            }
          }
        }
      `;

      const response = await this.graphqlService.query<GetUserFamiliesResponse>(query);

      if (response.getUserFamilies.families.length > 0) {
        this.currentFamily.set(response.getUserFamilies.families[0]);
      } else {
        this.currentFamily.set(null);
      }
    } catch (err) {
      this.handleError(err, 'Failed to load families');
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
            family {
              familyId { value }
              name
              memberCount
              createdAt
            }
            errors {
              message
              code
            }
          }
        }
      `;

      const response = await this.graphqlService.mutate<CreateFamilyResponse>(
        mutation,
        { input: { name } }
      );

      // Check for business logic errors (e.g., user already has family)
      if (response.createFamily.errors && response.createFamily.errors.length > 0) {
        this.error.set(response.createFamily.errors[0].message);
        return;
      }

      // Set currentFamily to newly created family
      if (response.createFamily.family) {
        this.currentFamily.set(response.createFamily.family);
      }
    } catch (err) {
      this.handleError(err, 'Failed to create family');
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
