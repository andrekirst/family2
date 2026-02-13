import { Injectable, inject, signal } from '@angular/core';
import { Apollo } from 'apollo-angular';
import {
  REGISTER_USER_MUTATION,
  GET_CURRENT_USER_QUERY,
} from '../../features/auth/graphql/auth.operations';
import { I18nService } from '../i18n/i18n.service';

export interface CurrentUser {
  id: string;
  email: string;
  name: string;
  emailVerified: boolean;
  isActive: boolean;
  familyId?: string | null;
  permissions: string[];
  preferredLocale?: string;
}

// GraphQL response types
interface RegisterUserResponse {
  registerUser: CurrentUser;
}

interface GetMyProfileResponse {
  me: { profile: CurrentUser };
}

/**
 * Service managing backend user state.
 * Separate from AuthService (which manages OAuth/tokens).
 *
 * Responsibilities:
 * - Sync OAuth login with backend database (RegisterUser mutation)
 * - Fetch user profile and family membership from backend
 * - Maintain reactive user state with Angular Signals
 */
@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly i18nService = inject(I18nService);

  // Reactive state using Angular Signals
  currentUser = signal<CurrentUser | null>(null);
  isLoading = signal(false);

  constructor(private apollo: Apollo) {}

  /**
   * Register/sync user with backend after OAuth login.
   * Called once per login session by CallbackComponent.
   *
   * Backend extracts user data from JWT claims (no input needed).
   *
   * @returns User data from backend
   * @throws Error if registration fails
   */
  async registerUser(): Promise<CurrentUser> {
    this.isLoading.set(true);

    try {
      const result = await this.apollo
        .mutate<RegisterUserResponse>({
          mutation: REGISTER_USER_MUTATION,
          variables: {
            input: {
              // Note: Backend extracts real values from JWT claims
              // These are placeholder values to satisfy GraphQL schema
              email: '',
              name: '',
              externalUserId: '',
              externalProvider: 'KEYCLOAK',
              emailVerified: false,
            },
          },
        })
        .toPromise();

      if (!result?.data?.registerUser) {
        throw new Error('Failed to register user with backend');
      }

      const user = result.data.registerUser;
      this.currentUser.set(user);

      // Sync backend locale preference to the frontend
      if (user.preferredLocale) {
        this.i18nService.applyBackendLocale(user.preferredLocale);
      }

      return user;
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Fetch current user data from backend.
   * Used by dashboard to load user + family data.
   *
   * @returns User data or null if not found
   */
  async fetchCurrentUser(): Promise<CurrentUser | null> {
    this.isLoading.set(true);

    try {
      const result = await this.apollo
        .query<GetMyProfileResponse>({
          query: GET_CURRENT_USER_QUERY,
          fetchPolicy: 'network-only', // Always fetch fresh data
        })
        .toPromise();

      if (!result?.data?.me?.profile) {
        return null;
      }

      const user = result.data.me.profile;
      this.currentUser.set(user);
      return user;
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Clear user state on logout.
   */
  clearUser(): void {
    this.currentUser.set(null);
  }
}
