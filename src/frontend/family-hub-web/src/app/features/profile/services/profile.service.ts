import { Injectable, inject, signal, computed } from '@angular/core';
import { GraphQLService, GraphQLError } from '../../../core/services/graphql.service';
import {
  UserProfile,
  ProfileState,
  UpdateUserProfileInput,
  GetMyProfileResponse,
} from '../models/profile.models';

/**
 * Error types for updateUserProfile mutation.
 * Backend uses Hot Chocolate's [UseMutationConvention] with [DefaultMutationErrors]
 * which generates an errors array containing these union types.
 */
type UpdateUserProfileError =
  | { __typename: 'BusinessError'; message: string; code: string }
  | { __typename: 'ValidationError'; message: string; field: string }
  | { __typename: 'UnauthorizedError'; message: string }
  | { __typename: 'InternalServerError'; message: string }
  | { __typename: 'ValueObjectError'; message: string };

/**
 * GraphQL response type for updateUserProfile mutation.
 * Mutation convention generates an OBJECT payload with DTO field and errors array.
 * Note: DTO field name keeps 'Dto' suffix (UpdateUserProfileDto → updateUserProfileDto).
 */
interface UpdateUserProfileMutationResponse {
  updateUserProfile: {
    updateUserProfileDto: {
      profileId: string;
      displayName: string;
      updatedAt: string;
      isNewProfile: boolean;
    } | null;
    errors: UpdateUserProfileError[];
  };
}

/**
 * Service for managing user profile data with Angular Signals.
 * Provides reactive state management for profile operations.
 *
 * Pattern: Follows FamilyService signal-based state pattern.
 *
 * @example
 * ```typescript
 * export class ProfilePageComponent {
 *   profileService = inject(ProfileService);
 *
 *   ngOnInit() {
 *     this.profileService.loadProfile();
 *   }
 * }
 * ```
 */
@Injectable({
  providedIn: 'root',
})
export class ProfileService {
  private graphqlService = inject(GraphQLService);

  /**
   * Internal state signal for profile data.
   */
  private profileState = signal<ProfileState>({
    profile: null,
    isLoading: false,
    isSetupComplete: false,
    error: null,
    lastUpdated: null,
  });

  // ===== Public Computed Signals =====

  /**
   * Current user profile (null if not loaded or doesn't exist).
   */
  readonly profile = computed(() => this.profileState().profile);

  /**
   * Loading state indicator.
   */
  readonly isLoading = computed(() => this.profileState().isLoading);

  /**
   * Whether profile setup is complete (displayName is set).
   * Used by profileSetupGuard to block dashboard access.
   */
  readonly isSetupComplete = computed(() => this.profileState().isSetupComplete);

  /**
   * Current error message (null if no error).
   */
  readonly error = computed(() => this.profileState().error);

  /**
   * Whether user has a profile.
   */
  readonly hasProfile = computed(() => this.profileState().profile !== null);

  /**
   * Display name for UI (falls back to 'User').
   */
  readonly displayName = computed(() => this.profileState().profile?.displayName ?? 'User');

  // ===== Public Methods =====

  /**
   * Loads the current user's profile from backend.
   * Called by APP_INITIALIZER on app startup and by components on demand.
   *
   * @returns Promise that resolves when load completes
   */
  async loadProfile(): Promise<void> {
    this.profileState.update((s) => ({ ...s, isLoading: true, error: null }));

    try {
      // Namespaced query: query { account { myProfile { ... } } }
      // This follows the nested namespace structure for domain organization.
      const query = `
        query GetMyProfile {
          account {
            myProfile {
              id
              userId
              displayName
              birthday
              age
              pronouns
              preferences {
                language
                timezone
                dateFormat
              }
              fieldVisibility {
                birthdayVisibility
                pronounsVisibility
                preferencesVisibility
              }
              createdAt
              updatedAt
            }
          }
        }
      `;

      const response = await this.graphqlService.query<{ account: GetMyProfileResponse }>(query);

      const profile = response.account.myProfile;
      const isSetupComplete =
        profile !== null && profile.displayName !== null && profile.displayName.trim().length > 0;

      this.profileState.update((s) => ({
        ...s,
        profile,
        isSetupComplete,
        lastUpdated: new Date().toISOString(),
      }));
    } catch (err) {
      this.handleError(err, 'Failed to load profile');
    } finally {
      this.profileState.update((s) => ({ ...s, isLoading: false }));
    }
  }

  /**
   * Updates the user profile (creates if doesn't exist).
   * Uses atomic upsert pattern on backend.
   *
   * @param input - Profile fields to update
   * @returns Promise resolving to true on success, false on failure
   */
  async updateProfile(input: UpdateUserProfileInput): Promise<boolean> {
    this.profileState.update((s) => ({ ...s, isLoading: true, error: null }));

    try {
      const mutation = `
        mutation UpdateUserProfile($input: UpdateUserProfileInput!) {
          updateUserProfile(input: $input) {
            updateUserProfileDto {
              profileId
              displayName
              updatedAt
              isNewProfile
            }
            errors {
              __typename
              ... on BusinessError {
                message
                code
              }
              ... on ValidationError {
                message
                field
              }
              ... on UnauthorizedError {
                message
              }
              ... on InternalServerError {
                message
              }
              ... on ValueObjectError {
                message
              }
            }
          }
        }
      `;

      const response = await this.graphqlService.mutate<UpdateUserProfileMutationResponse>(
        mutation,
        { input }
      );

      // Check for mutation convention errors in payload
      const payload = response.updateUserProfile;
      if (payload.errors && payload.errors.length > 0) {
        const errorMessage = payload.errors.map((e) => e.message).join('; ');
        throw new Error(errorMessage);
      }

      // Success case - DTO data available
      // Reload full profile to get all updated fields including calculated age
      await this.loadProfile();

      return true;
    } catch (err) {
      this.handleError(err, 'Failed to update profile');
      return false;
    } finally {
      this.profileState.update((s) => ({ ...s, isLoading: false }));
    }
  }

  /**
   * Quick setup method for first-login wizard (display name only).
   * Creates a minimal profile with just the display name.
   *
   * @param displayName - User's chosen display name
   * @returns Promise resolving to true on success, false on failure
   */
  async completeSetup(displayName: string): Promise<boolean> {
    return this.updateProfile({ displayName });
  }

  /**
   * Clears any error state.
   * Call this when user dismisses error message or navigates away.
   */
  clearError(): void {
    this.profileState.update((s) => ({ ...s, error: null }));
  }

  // ===== Private Methods =====

  /**
   * Handles errors from GraphQL operations.
   * Sets error signal with appropriate message.
   *
   * @param err - Error object from GraphQL operation
   * @param fallbackMessage - Message to use if error is unknown type
   */
  private handleError(err: unknown, fallbackMessage: string): void {
    let message = fallbackMessage;

    if (err instanceof Error) {
      message = err.message;
    } else if (err instanceof GraphQLError) {
      message = err.message;
    }

    this.profileState.update((s) => ({ ...s, error: message }));
  }
}
