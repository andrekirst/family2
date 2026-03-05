import { Injectable, inject, signal } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { firstValueFrom } from 'rxjs';
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
  avatarId?: string | null;
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

  // Shared promise so multiple consumers (callback, dashboard, guards) coalesce
  // into a single in-flight request instead of each making their own.
  private _readyPromise: Promise<CurrentUser | null> | null = null;

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
  registerUser(): Promise<CurrentUser> {
    const promise = this._doRegisterUser();
    this._readyPromise = promise;
    return promise;
  }

  private async _doRegisterUser(): Promise<CurrentUser> {
    this.isLoading.set(true);
    try {
      return await this._attemptRegister();
    } catch (firstError) {
      console.warn('RegisterUser first attempt failed, retrying in 1s…', firstError);
      await new Promise((resolve) => setTimeout(resolve, 1000));
      try {
        return await this._attemptRegister();
      } catch (retryError) {
        console.error('RegisterUser retry also failed:', retryError);
        throw retryError;
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  private async _attemptRegister(): Promise<CurrentUser> {
    // Use errorPolicy: 'none' so GraphQL errors throw instead of silently
    // returning { data: null }. The global 'all' policy is useful for queries
    // with partial data, but registerUser is all-or-nothing.
    const result = await firstValueFrom(
      this.apollo.mutate<RegisterUserResponse>({
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
        errorPolicy: 'none',
      }),
    );

    if (!result?.data?.registerUser) {
      // With errorPolicy: 'none', GraphQL errors throw before reaching here.
      // If we still get null data, log the full result for diagnostics.
      console.error('RegisterUser returned null data. Full result:', JSON.stringify(result));
      throw new Error('Backend registration returned null data (no GraphQL errors)');
    }

    const user = result.data.registerUser;
    this.currentUser.set(user);

    // Sync backend locale preference to the frontend
    if (user.preferredLocale) {
      this.i18nService.applyBackendLocale(user.preferredLocale);
    }

    return user;
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
      const result = await firstValueFrom(
        this.apollo.query<GetMyProfileResponse>({
          query: GET_CURRENT_USER_QUERY,
          fetchPolicy: 'network-only', // Always fetch fresh data
        }),
      );

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
   * Wait for the user to be available. Multiple consumers (dashboard, guards)
   * share a single in-flight request via _readyPromise.
   *
   * Fast path: currentUser signal already populated → returns immediately.
   * In-flight: registerUser() running → piggybacks on its promise.
   * Cold start (F5 refresh): triggers fetchCurrentUser().
   */
  whenReady(): Promise<CurrentUser | null> {
    const user = this.currentUser();
    if (user) {
      return Promise.resolve(user);
    }
    if (this._readyPromise) {
      return this._readyPromise;
    }
    this._readyPromise = this.fetchCurrentUser();
    return this._readyPromise;
  }

  /**
   * Clear user state on logout.
   */
  clearUser(): void {
    this.currentUser.set(null);
    this._readyPromise = null;
  }
}
