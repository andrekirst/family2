import { Injectable, signal, computed, inject, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { GraphQLService } from './graphql.service';
import {
  AuthState,
  LoginMutationResponse,
  RegisterMutationResponse,
  LogoutMutationResponse,
  RefreshTokenMutationResponse,
  RequestPasswordResetMutationResponse,
  ResetPasswordMutationResponse,
  ResetPasswordWithCodeMutationResponse,
  VerifyEmailMutationResponse,
  ResendVerificationEmailMutationResponse,
  ChangePasswordMutationResponse,
  ValidatePasswordQueryResponse,
  PasswordValidationResult,
} from '../models/auth.models';

/**
 * Authentication service for local email/password authentication.
 * Handles registration, login, logout, password reset, and token management.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthService implements OnDestroy {
  private readonly ACCESS_TOKEN_KEY = 'family_hub_access_token';
  private readonly REFRESH_TOKEN_KEY = 'family_hub_refresh_token';
  private readonly TOKEN_EXPIRES_KEY = 'family_hub_token_expires';

  // Token refresh margin (refresh 1 minute before expiration)
  private readonly REFRESH_MARGIN_MS = 60 * 1000;

  private refreshTimer: ReturnType<typeof setTimeout> | null = null;

  // Signals for reactive auth state
  private authState = signal<AuthState>({
    isAuthenticated: false,
    user: null,
    accessToken: null,
    refreshToken: null,
    expiresAt: null,
  });

  // Computed signals for derived state
  readonly isAuthenticated = computed(() => this.authState().isAuthenticated);
  readonly currentUser = computed(() => this.authState().user);
  readonly accessToken = computed(() => this.authState().accessToken);
  readonly emailVerified = computed(() => this.authState().user?.emailVerified ?? false);

  private readonly graphql = inject(GraphQLService);
  private readonly router = inject(Router);

  constructor() {
    this.initializeAuthState();
  }

  ngOnDestroy(): void {
    this.clearRefreshTimer();
  }

  /**
   * Initialize auth state from localStorage on app startup.
   */
  private initializeAuthState(): void {
    const accessToken = localStorage.getItem(this.ACCESS_TOKEN_KEY);
    const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);
    const expiresAt = localStorage.getItem(this.TOKEN_EXPIRES_KEY);

    if (accessToken && refreshToken && expiresAt) {
      const expiry = new Date(expiresAt);

      if (expiry > new Date()) {
        // Token is valid, restore auth state
        this.authState.set({
          isAuthenticated: true,
          user: null, // Will be loaded from API on first request
          accessToken,
          refreshToken,
          expiresAt: expiry,
        });

        // Schedule token refresh
        this.scheduleTokenRefresh(expiry);
      } else {
        // Token expired, try to refresh
        this.refreshTokens().catch(() => {
          this.clearAuthState();
        });
      }
    }
  }

  /**
   * Register a new user with email and password.
   * Creates user account and sends verification email.
   */
  async register(email: string, password: string, confirmPassword: string): Promise<void> {
    const mutation = `
      mutation Register($input: RegisterInput!) {
        register(input: $input) {
          userId
          email
          emailVerificationRequired
          accessToken
          refreshToken
          errors {
            code
            message
          }
        }
      }
    `;

    const response = await this.graphql.mutate<RegisterMutationResponse>(mutation, {
      input: { email, password, confirmPassword },
    });

    const result = response.register;

    if (result.errors && result.errors.length > 0) {
      throw new Error(result.errors[0].message);
    }

    if (!result.userId) {
      throw new Error('Registration failed - no user ID returned');
    }

    // If tokens were returned (email verification not required), set auth state
    if (result.accessToken && result.refreshToken) {
      const expiresAt = new Date(Date.now() + 15 * 60 * 1000); // 15 minutes

      this.storeTokens(result.accessToken, result.refreshToken, expiresAt);
      this.authState.set({
        isAuthenticated: true,
        user: {
          id: result.userId,
          email: result.email!,
          emailVerified: !result.emailVerificationRequired,
          createdAt: new Date(),
        },
        accessToken: result.accessToken,
        refreshToken: result.refreshToken,
        expiresAt,
      });

      this.scheduleTokenRefresh(expiresAt);
    }
  }

  /**
   * Authenticate user with email and password.
   * Returns JWT tokens on success.
   */
  async login(email: string, password: string): Promise<void> {
    const mutation = `
      mutation Login($input: LoginInput!) {
        login(input: $input) {
          userId
          email
          accessToken
          refreshToken
          expiresIn
          familyId
          emailVerified
          errors {
            code
            message
          }
        }
      }
    `;

    const response = await this.graphql.mutate<LoginMutationResponse>(mutation, {
      input: { email, password },
    });

    const result = response.login;

    if (result.errors && result.errors.length > 0) {
      throw new Error(result.errors[0].message);
    }

    if (!result.accessToken || !result.refreshToken) {
      throw new Error('Login failed - no tokens returned');
    }

    const expiresAt = new Date(Date.now() + (result.expiresIn ?? 900) * 1000);

    this.storeTokens(result.accessToken, result.refreshToken, expiresAt);
    this.authState.set({
      isAuthenticated: true,
      user: {
        id: result.userId!,
        email: result.email!,
        emailVerified: result.emailVerified ?? false,
        familyId: result.familyId ?? undefined,
        createdAt: new Date(),
      },
      accessToken: result.accessToken,
      refreshToken: result.refreshToken,
      expiresAt,
    });

    this.scheduleTokenRefresh(expiresAt);
  }

  /**
   * Log out the current user.
   * Revokes refresh token and clears local state.
   */
  async logout(): Promise<void> {
    const refreshToken = this.authState().refreshToken;

    try {
      if (refreshToken) {
        const mutation = `
          mutation Logout($refreshToken: String) {
            logout(refreshToken: $refreshToken) {
              success
              revokedSessionCount
              errors {
                code
                message
              }
            }
          }
        `;

        await this.graphql.mutate<LogoutMutationResponse>(mutation, {
          refreshToken,
        });
      }
    } catch {
      // Ignore errors during logout - we want to clear local state regardless
    } finally {
      this.clearAuthState();
      await this.router.navigate(['/login']);
    }
  }

  /**
   * Refresh access and refresh tokens.
   * Called automatically before token expiration.
   */
  async refreshTokens(): Promise<void> {
    const currentRefreshToken =
      this.authState().refreshToken ?? localStorage.getItem(this.REFRESH_TOKEN_KEY);

    if (!currentRefreshToken) {
      this.clearAuthState();
      throw new Error('No refresh token available');
    }

    const mutation = `
      mutation RefreshToken($refreshToken: String!) {
        refreshToken(refreshToken: $refreshToken) {
          accessToken
          refreshToken
          expiresIn
          errors {
            code
            message
          }
        }
      }
    `;

    try {
      const response = await this.graphql.mutate<RefreshTokenMutationResponse>(mutation, {
        refreshToken: currentRefreshToken,
      });

      const result = response.refreshToken;

      if (result.errors && result.errors.length > 0) {
        this.clearAuthState();
        throw new Error(result.errors[0].message);
      }

      if (!result.accessToken || !result.refreshToken) {
        this.clearAuthState();
        throw new Error('Token refresh failed - no tokens returned');
      }

      const expiresAt = new Date(Date.now() + (result.expiresIn ?? 900) * 1000);

      this.storeTokens(result.accessToken, result.refreshToken, expiresAt);

      // Update auth state, preserving user data
      this.authState.update((state) => ({
        ...state,
        accessToken: result.accessToken!,
        refreshToken: result.refreshToken!,
        expiresAt,
      }));

      this.scheduleTokenRefresh(expiresAt);
    } catch (error) {
      this.clearAuthState();
      throw error;
    }
  }

  /**
   * Request a password reset email.
   * @param email User's email address
   * @param useMobileCode If true, sends 6-digit code instead of link
   */
  async requestPasswordReset(email: string, useMobileCode = false): Promise<void> {
    const mutation = `
      mutation RequestPasswordReset($input: RequestPasswordResetInput!) {
        requestPasswordReset(input: $input) {
          success
          message
          errors {
            code
            message
          }
        }
      }
    `;

    const response = await this.graphql.mutate<RequestPasswordResetMutationResponse>(mutation, {
      input: { email, useMobileCode },
    });

    const result = response.requestPasswordReset;

    if (result.errors && result.errors.length > 0) {
      throw new Error(result.errors[0].message);
    }
  }

  /**
   * Reset password using token from email link.
   */
  async resetPassword(
    token: string,
    newPassword: string,
    confirmNewPassword: string
  ): Promise<void> {
    const mutation = `
      mutation ResetPassword($input: ResetPasswordInput!) {
        resetPassword(input: $input) {
          success
          errors {
            code
            message
          }
        }
      }
    `;

    const response = await this.graphql.mutate<ResetPasswordMutationResponse>(mutation, {
      input: { token, newPassword, confirmNewPassword },
    });

    const result = response.resetPassword;

    if (result.errors && result.errors.length > 0) {
      throw new Error(result.errors[0].message);
    }
  }

  /**
   * Reset password using 6-digit code (mobile flow).
   */
  async resetPasswordWithCode(
    email: string,
    code: string,
    newPassword: string,
    confirmNewPassword: string
  ): Promise<void> {
    const mutation = `
      mutation ResetPasswordWithCode($input: ResetPasswordWithCodeInput!) {
        resetPasswordWithCode(input: $input) {
          success
          errors {
            code
            message
          }
        }
      }
    `;

    const response = await this.graphql.mutate<ResetPasswordWithCodeMutationResponse>(mutation, {
      input: { email, code, newPassword, confirmNewPassword },
    });

    const result = response.resetPasswordWithCode;

    if (result.errors && result.errors.length > 0) {
      throw new Error(result.errors[0].message);
    }
  }

  /**
   * Verify user's email address using token from email link.
   */
  async verifyEmail(token: string): Promise<void> {
    const mutation = `
      mutation VerifyEmail($token: String!) {
        verifyEmail(token: $token) {
          success
          message
          errors {
            code
            message
          }
        }
      }
    `;

    const response = await this.graphql.mutate<VerifyEmailMutationResponse>(mutation, {
      token,
    });

    const result = response.verifyEmail;

    if (result.errors && result.errors.length > 0) {
      throw new Error(result.errors[0].message);
    }

    // Update user's email verified status
    if (result.success) {
      this.authState.update((state) => ({
        ...state,
        user: state.user ? { ...state.user, emailVerified: true } : null,
      }));
    }
  }

  /**
   * Resend email verification link.
   * Requires authentication.
   */
  async resendVerificationEmail(): Promise<void> {
    const mutation = `
      mutation ResendVerificationEmail {
        resendVerificationEmail {
          success
          message
          errors {
            code
            message
          }
        }
      }
    `;

    const response = await this.graphql.mutate<ResendVerificationEmailMutationResponse>(
      mutation,
      {}
    );

    const result = response.resendVerificationEmail;

    if (result.errors && result.errors.length > 0) {
      throw new Error(result.errors[0].message);
    }
  }

  /**
   * Change the authenticated user's password.
   */
  async changePassword(
    currentPassword: string,
    newPassword: string,
    confirmNewPassword: string
  ): Promise<void> {
    const mutation = `
      mutation ChangePassword($input: ChangePasswordInput!) {
        changePassword(input: $input) {
          success
          errors {
            code
            message
          }
        }
      }
    `;

    const response = await this.graphql.mutate<ChangePasswordMutationResponse>(mutation, {
      input: { currentPassword, newPassword, confirmNewPassword },
    });

    const result = response.changePassword;

    if (result.errors && result.errors.length > 0) {
      throw new Error(result.errors[0].message);
    }
  }

  /**
   * Validate password strength.
   * Returns real-time feedback for password input.
   */
  async validatePassword(password: string): Promise<PasswordValidationResult> {
    const query = `
      query ValidatePassword($password: String!) {
        auth {
          validatePassword(password: $password) {
            isValid
            score
            strength
            errors
            suggestions
          }
        }
      }
    `;

    const response = await this.graphql.query<ValidatePasswordQueryResponse>(query, {
      password,
    });

    return response.auth.validatePassword;
  }

  /**
   * Get the current access token.
   */
  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  // ============================================
  // Private Helper Methods
  // ============================================

  private storeTokens(accessToken: string, refreshToken: string, expiresAt: Date): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(this.TOKEN_EXPIRES_KEY, expiresAt.toISOString());
  }

  private clearAuthState(): void {
    this.clearRefreshTimer();

    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.TOKEN_EXPIRES_KEY);

    this.authState.set({
      isAuthenticated: false,
      user: null,
      accessToken: null,
      refreshToken: null,
      expiresAt: null,
    });
  }

  private scheduleTokenRefresh(expiresAt: Date): void {
    this.clearRefreshTimer();

    const refreshTime = expiresAt.getTime() - this.REFRESH_MARGIN_MS - Date.now();

    if (refreshTime > 0) {
      this.refreshTimer = setTimeout(() => {
        this.refreshTokens().catch((error) => {
          console.error('Auto-refresh failed:', error);
          this.clearAuthState();
          this.router.navigate(['/login']);
        });
      }, refreshTime);
    }
  }

  private clearRefreshTimer(): void {
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
      this.refreshTimer = null;
    }
  }
}
