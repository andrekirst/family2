import { Injectable, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { GraphQLService } from './graphql.service';
import {
  AuthState,
  GetZitadelAuthUrlResponse,
  CompleteZitadelLoginResponse
} from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'family_hub_access_token';
  private readonly TOKEN_EXPIRES_KEY = 'family_hub_token_expires';

  // Signals for reactive auth state
  private authState = signal<AuthState>({
    isAuthenticated: false,
    user: null,
    accessToken: null,
    expiresAt: null,
  });

  // Computed signals for derived state
  readonly isAuthenticated = computed(() => this.authState().isAuthenticated);
  readonly currentUser = computed(() => this.authState().user);
  readonly accessToken = computed(() => this.authState().accessToken);

  private readonly graphql = inject(GraphQLService);
  private readonly router = inject(Router);

  constructor() {
    this.initializeAuthState();
  }

  private initializeAuthState(): void {
    const token = localStorage.getItem(this.TOKEN_KEY);
    const expiresAt = localStorage.getItem(this.TOKEN_EXPIRES_KEY);

    if (token && expiresAt && new Date(expiresAt) > new Date()) {
      // Token is valid, restore auth state
      // Note: We don't store full user in localStorage for security
      // Will be loaded on first API call
      this.authState.set({
        isAuthenticated: true,
        user: null, // Will be loaded from API
        accessToken: token,
        expiresAt: new Date(expiresAt),
      });
    }
  }

  /**
   * Initiates OAuth login with Zitadel.
   * @param identifier - Email address entered by user (optional, used as login_hint)
   */
  async login(identifier?: string): Promise<void> {
    try {
      // Use identifier as email for login_hint parameter
      const loginHint = identifier?.trim() || undefined;

      const query = `
        query GetZitadelAuthUrl($loginHint: String) {
          zitadelAuthUrl(loginHint: $loginHint) {
            authorizationUrl
            codeVerifier
            state
          }
        }
      `;

      const response = await this.graphql.query<GetZitadelAuthUrlResponse>(
        query,
        { loginHint }
      );
      const { authorizationUrl, codeVerifier, state } = response.zitadelAuthUrl;

      // Store PKCE verifier and state in sessionStorage (temporary)
      sessionStorage.setItem('pkce_code_verifier', codeVerifier);
      sessionStorage.setItem('oauth_state', state);

      // Redirect to Zitadel OAuth UI
      window.location.href = authorizationUrl;
    } catch (error) {
      console.error('Login failed:', error);
      throw new Error('Failed to initiate login');
    }
  }

  async completeLogin(code: string, state: string): Promise<void> {
    try {
      // Validate state parameter (CSRF protection)
      const storedState = sessionStorage.getItem('oauth_state');
      if (!storedState || storedState !== state) {
        throw new Error('Invalid state parameter - possible CSRF attack');
      }

      // Retrieve PKCE verifier
      const codeVerifier = sessionStorage.getItem('pkce_code_verifier');
      if (!codeVerifier) {
        throw new Error('Missing PKCE verifier');
      }

      // Exchange authorization code for tokens
      const mutation = `
        mutation CompleteZitadelLogin($input: CompleteZitadelLoginInput!) {
          completeZitadelLogin(input: $input) {
            authenticationResult {
              user {
                id
                email
                emailVerified
                auditInfo {
                  createdAt
                }
              }
              accessToken
              expiresAt
            }
            errors {
              message
              code
            }
          }
        }
      `;

      const response = await this.graphql.mutate<CompleteZitadelLoginResponse>(
        mutation,
        {
          input: {
            authorizationCode: code,
            codeVerifier: codeVerifier,
          },
        }
      );

      const result = response.completeZitadelLogin;

      // Check for errors
      if (result.errors && result.errors.length > 0) {
        throw new Error(result.errors[0].message);
      }

      if (!result.authenticationResult) {
        throw new Error('Authentication failed - no result');
      }

      const { user, accessToken, expiresAt } = result.authenticationResult;

      // Store token in localStorage (persistent)
      localStorage.setItem(this.TOKEN_KEY, accessToken);
      localStorage.setItem(this.TOKEN_EXPIRES_KEY, expiresAt);

      // Clear sessionStorage
      sessionStorage.removeItem('pkce_code_verifier');
      sessionStorage.removeItem('oauth_state');

      // Update auth state signal
      this.authState.set({
        isAuthenticated: true,
        user: {
          id: user.id,
          email: user.email,
          emailVerified: user.emailVerified,
          createdAt: new Date(user.auditInfo.createdAt),
        },
        accessToken: accessToken,
        expiresAt: new Date(expiresAt),
      });

    } catch (error) {
      // Clean up on error
      sessionStorage.clear();
      console.error('Complete login failed:', error);
      throw error;
    }
  }

  logout(): void {
    // Clear tokens
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.TOKEN_EXPIRES_KEY);
    sessionStorage.clear();

    // Reset auth state
    this.authState.set({
      isAuthenticated: false,
      user: null,
      accessToken: null,
      expiresAt: null,
    });

    // Redirect to login
    this.router.navigate(['/login']);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }
}
