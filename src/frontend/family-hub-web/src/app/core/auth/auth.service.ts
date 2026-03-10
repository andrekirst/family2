import { Injectable, inject, signal, isDevMode } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { EnvironmentConfigService } from '../config/environment-config.service';
import { UserProfile, AuthTokens, JwtPayload } from './auth.models';
import {
  generateCodeVerifier,
  generateCodeChallenge,
  generateState,
  base64UrlDecode,
} from '../../shared/utils/crypto.utils';

/** Fraction of token lifetime at which to trigger proactive refresh (80% = refresh at 80% of lifetime) */
const REFRESH_AT_LIFETIME_FRACTION = 0.8;

/**
 * Authentication service implementing OAuth 2.0 Authorization Code Flow with PKCE
 * with proactive token refresh, cross-tab sync, and visibility-based refresh.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  // Reactive state using Angular signals
  isAuthenticated = signal(false);
  userProfile = signal<UserProfile | null>(null);

  private readonly STORAGE_ACCESS_TOKEN = 'access_token';
  private readonly STORAGE_ID_TOKEN = 'id_token';
  private readonly STORAGE_REFRESH_TOKEN = 'refresh_token';
  private readonly STORAGE_EXPIRES_AT = 'expires_at';
  private readonly STORAGE_ISSUED_AT = 'issued_at';
  private readonly STORAGE_EXPECTED_ISSUER = 'expected_issuer';

  private readonly envConfig = inject(EnvironmentConfigService);
  private refreshTimer: ReturnType<typeof setTimeout> | null = null;
  private refreshPromise: Promise<boolean> | null = null;

  constructor(private http: HttpClient) {
    this.checkAuthStatus();
    this.setupVisibilityHandler();
    this.setupStorageSyncHandler();
  }

  /**
   * Check if user is currently authenticated on service initialization
   */
  private checkAuthStatus(): void {
    const accessToken = this.getAccessToken();
    if (accessToken && !this.isTokenExpired()) {
      // Verify stored tokens belong to the current environment's issuer
      const storedIssuer = localStorage.getItem(this.STORAGE_EXPECTED_ISSUER);
      if (storedIssuer && storedIssuer !== this.envConfig.keycloak.issuer) {
        console.warn(
          'Stale auth detected: token issuer mismatch (stored:',
          storedIssuer,
          'expected:',
          this.envConfig.keycloak.issuer,
          ')',
        );
        this.clearTokens();
        return;
      }

      const profile = this.getUserProfileFromToken();
      if (profile) {
        this.userProfile.set(profile);
        this.isAuthenticated.set(true);
        this.scheduleTokenRefresh();
      }
    } else {
      this.clearTokens();
    }
  }

  /**
   * Schedule a proactive token refresh at 80% of the token's lifetime.
   * For a 5-minute token, refreshes at 4 minutes. For a 30-minute token, at 24 minutes.
   * Re-schedules itself after each successful refresh.
   */
  private scheduleTokenRefresh(): void {
    this.clearRefreshTimer();

    const expiresAt = localStorage.getItem(this.STORAGE_EXPIRES_AT);
    const issuedAt = localStorage.getItem(this.STORAGE_ISSUED_AT);
    if (!expiresAt) return;

    const expirationTime = parseInt(expiresAt, 10);
    const issuedTime = issuedAt ? parseInt(issuedAt, 10) : undefined;
    const now = Math.floor(Date.now() / 1000);

    // Calculate delay: refresh at 80% of token lifetime, or 60s before expiry as fallback
    const tokenLifetime = issuedTime ? expirationTime - issuedTime : undefined;
    const refreshAt = tokenLifetime
      ? issuedTime! + Math.floor(tokenLifetime * REFRESH_AT_LIFETIME_FRACTION)
      : expirationTime - 60;
    const delaySeconds = refreshAt - now;

    if (delaySeconds > 0) {
      this.refreshTimer = setTimeout(async () => {
        const success = await this.refreshAccessToken();
        if (success) {
          this.scheduleTokenRefresh();
        }
      }, delaySeconds * 1000);
    } else {
      // Already near expiry — refresh immediately
      this.refreshAccessToken().then((success) => {
        if (success) {
          this.scheduleTokenRefresh();
        }
      });
    }
  }

  private clearRefreshTimer(): void {
    if (this.refreshTimer !== null) {
      clearTimeout(this.refreshTimer);
      this.refreshTimer = null;
    }
  }

  /**
   * When the user switches back to this tab, check if the token needs refreshing.
   */
  private setupVisibilityHandler(): void {
    document.addEventListener('visibilitychange', () => {
      if (document.visibilityState === 'visible' && this.isAuthenticated()) {
        if (this.isTokenExpired()) {
          this.refreshAccessToken().then((success) => {
            if (success) {
              this.scheduleTokenRefresh();
            }
          });
        } else {
          // Token still valid but timer may have drifted while tab was hidden
          this.scheduleTokenRefresh();
        }
      }
    });
  }

  /**
   * When another tab refreshes tokens, pick up the new values via the storage event.
   */
  private setupStorageSyncHandler(): void {
    window.addEventListener('storage', (event: StorageEvent) => {
      if (event.key === this.STORAGE_ACCESS_TOKEN || event.key === this.STORAGE_EXPIRES_AT) {
        const accessToken = this.getAccessToken();
        if (accessToken && !this.isTokenExpired()) {
          const profile = this.getUserProfileFromToken();
          if (profile) {
            this.userProfile.set(profile);
            this.isAuthenticated.set(true);
            this.scheduleTokenRefresh();
          }
        } else if (!accessToken) {
          // Another tab logged out
          this.clearRefreshTimer();
          this.isAuthenticated.set(false);
          this.userProfile.set(null);
        }
      }
    });
  }

  /**
   * Initiate OAuth 2.0 Authorization Code Flow with PKCE
   */
  async login(): Promise<void> {
    // Generate PKCE parameters
    const codeVerifier = generateCodeVerifier();
    const codeChallenge = await generateCodeChallenge(codeVerifier);
    const state = generateState();

    // Store for callback validation (session storage - cleared on tab close)
    sessionStorage.setItem('code_verifier', codeVerifier);
    sessionStorage.setItem('state', state);

    // Store intended redirect URL (if navigating from protected route)
    const redirectUrl = sessionStorage.getItem('redirect_url') || '/dashboard';
    sessionStorage.setItem('post_login_redirect', redirectUrl);

    // Build Keycloak authorization URL
    const authUrl = new URL(`${this.envConfig.keycloak.issuer}/protocol/openid-connect/auth`);
    authUrl.searchParams.append('client_id', this.envConfig.keycloak.clientId);
    authUrl.searchParams.append('redirect_uri', this.envConfig.keycloak.redirectUri);
    authUrl.searchParams.append('response_type', 'code');
    authUrl.searchParams.append('scope', this.envConfig.keycloak.scope);
    authUrl.searchParams.append('code_challenge', codeChallenge);
    authUrl.searchParams.append('code_challenge_method', 'S256');
    authUrl.searchParams.append('state', state);
    authUrl.searchParams.append('prompt', 'login');

    // Redirect to Keycloak
    window.location.href = authUrl.toString();
  }

  /**
   * Handle OAuth callback with authorization code
   * This is called by the callback component after Keycloak redirects back
   */
  async handleCallback(code: string, state: string): Promise<void> {
    // Validate state parameter (CSRF protection)
    const storedState = sessionStorage.getItem('state');
    if (state !== storedState) {
      console.error('State parameter mismatch - possible CSRF attack');
      throw new Error('Invalid state parameter');
    }

    // Retrieve code verifier
    const codeVerifier = sessionStorage.getItem('code_verifier');
    if (!codeVerifier) {
      throw new Error('Code verifier not found - PKCE flow interrupted');
    }

    // Exchange authorization code for tokens
    const tokenUrl = `${this.envConfig.keycloak.issuer}/protocol/openid-connect/token`;
    const body = new URLSearchParams({
      grant_type: 'authorization_code',
      code,
      redirect_uri: this.envConfig.keycloak.redirectUri,
      client_id: this.envConfig.keycloak.clientId,
      code_verifier: codeVerifier,
    });

    try {
      const tokens = await firstValueFrom(
        this.http.post<AuthTokens>(tokenUrl, body.toString(), {
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        }),
      );

      // Store tokens
      this.storeTokens(tokens);

      // Decode and set user profile
      const profile = this.getUserProfileFromToken();
      if (profile) {
        this.userProfile.set(profile);
        this.isAuthenticated.set(true);

        if (isDevMode()) {
          console.log('OAuth login successful', { userId: profile.userId });
        }
      }

      // Clean up PKCE session storage
      sessionStorage.removeItem('code_verifier');
      sessionStorage.removeItem('state');
    } catch (error) {
      console.error('Token exchange failed:', error);
      this.clearTokens();
      throw error;
    }
  }

  /**
   * Consume and clear the stored post-login redirect URL.
   * Returns the redirect path (defaults to '/dashboard' if none stored).
   */
  consumePostLoginRedirect(): string {
    const redirectUrl = sessionStorage.getItem('post_login_redirect') || '/dashboard';
    sessionStorage.removeItem('post_login_redirect');
    sessionStorage.removeItem('redirect_url');
    return this.sanitizeRedirectUrl(redirectUrl);
  }

  /**
   * Sanitize redirect URL to prevent open redirect attacks.
   * Only allows relative paths starting with '/'. Blocks protocol-relative URLs,
   * absolute URLs, and javascript: URIs.
   */
  private sanitizeRedirectUrl(url: string): string {
    const fallback = '/dashboard';
    if (!url || !url.startsWith('/') || url.startsWith('//') || url.includes('://')) {
      return fallback;
    }
    return url;
  }

  /**
   * Logout user and redirect to Keycloak logout endpoint
   */
  async logout(): Promise<void> {
    const idToken = this.getIdToken();

    // Clear local tokens
    this.clearTokens();
    this.isAuthenticated.set(false);
    this.userProfile.set(null);

    // Build Keycloak logout URL
    const logoutUrl = new URL(`${this.envConfig.keycloak.issuer}/protocol/openid-connect/logout`);
    logoutUrl.searchParams.append('client_id', this.envConfig.keycloak.clientId);
    logoutUrl.searchParams.append(
      'post_logout_redirect_uri',
      this.envConfig.keycloak.postLogoutRedirectUri,
    );
    if (idToken) {
      logoutUrl.searchParams.append('id_token_hint', idToken);
    }

    // Redirect to Keycloak logout
    window.location.href = logoutUrl.toString();
  }

  /**
   * Refresh access token using refresh token.
   * Deduplicates concurrent calls — multiple callers share a single in-flight request.
   */
  async refreshAccessToken(): Promise<boolean> {
    // Deduplicate: if a refresh is already in progress, return the same promise
    if (this.refreshPromise) {
      return this.refreshPromise;
    }

    this.refreshPromise = this.executeTokenRefresh();

    try {
      return await this.refreshPromise;
    } finally {
      this.refreshPromise = null;
    }
  }

  private async executeTokenRefresh(): Promise<boolean> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return false;
    }

    const tokenUrl = `${this.envConfig.keycloak.issuer}/protocol/openid-connect/token`;
    const body = new URLSearchParams({
      grant_type: 'refresh_token',
      refresh_token: refreshToken,
      client_id: this.envConfig.keycloak.clientId,
    });

    try {
      const tokens = await firstValueFrom(
        this.http.post<AuthTokens>(tokenUrl, body.toString(), {
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        }),
      );

      this.storeTokens(tokens);
      return true;
    } catch (error) {
      console.error('Token refresh failed:', error);
      this.clearTokens();
      this.isAuthenticated.set(false);
      this.userProfile.set(null);
      return false;
    }
  }

  /**
   * Get current access token from storage
   */
  getAccessToken(): string | null {
    return localStorage.getItem(this.STORAGE_ACCESS_TOKEN);
  }

  /**
   * Get ID token from storage
   */
  getIdToken(): string | null {
    return localStorage.getItem(this.STORAGE_ID_TOKEN);
  }

  /**
   * Get refresh token from storage
   */
  getRefreshToken(): string | null {
    return localStorage.getItem(this.STORAGE_REFRESH_TOKEN);
  }

  /**
   * Check if current access token is expired
   */
  isTokenExpired(): boolean {
    const expiresAt = localStorage.getItem(this.STORAGE_EXPIRES_AT);
    if (!expiresAt) {
      return true;
    }

    const expirationTime = parseInt(expiresAt, 10);
    const currentTime = Math.floor(Date.now() / 1000);

    // Add 30-second buffer to refresh before actual expiration
    return currentTime >= expirationTime - 30;
  }

  /**
   * Store OAuth tokens in localStorage
   */
  private storeTokens(tokens: AuthTokens): void {
    localStorage.setItem(this.STORAGE_ACCESS_TOKEN, tokens.access_token);
    localStorage.setItem(this.STORAGE_ID_TOKEN, tokens.id_token);
    localStorage.setItem(this.STORAGE_REFRESH_TOKEN, tokens.refresh_token);

    // Calculate and store issuance and expiration times
    const issuedAt = Math.floor(Date.now() / 1000);
    const expiresAt = issuedAt + tokens.expires_in;
    localStorage.setItem(this.STORAGE_ISSUED_AT, issuedAt.toString());
    localStorage.setItem(this.STORAGE_EXPIRES_AT, expiresAt.toString());

    // Store issuer for cross-environment stale auth detection
    localStorage.setItem(this.STORAGE_EXPECTED_ISSUER, this.envConfig.keycloak.issuer);

    this.scheduleTokenRefresh();
  }

  /**
   * Clear all OAuth tokens from localStorage
   */
  private clearTokens(): void {
    this.clearRefreshTimer();
    localStorage.removeItem(this.STORAGE_ACCESS_TOKEN);
    localStorage.removeItem(this.STORAGE_ID_TOKEN);
    localStorage.removeItem(this.STORAGE_REFRESH_TOKEN);
    localStorage.removeItem(this.STORAGE_EXPIRES_AT);
    localStorage.removeItem(this.STORAGE_ISSUED_AT);
    localStorage.removeItem(this.STORAGE_EXPECTED_ISSUER);
  }

  /**
   * Decode JWT ID token and extract user profile
   */
  private getUserProfileFromToken(): UserProfile | null {
    const idToken = this.getIdToken();
    if (!idToken) {
      return null;
    }

    try {
      // JWT format: header.payload.signature
      const parts = idToken.split('.');
      if (parts.length !== 3) {
        return null;
      }

      // Decode payload (middle part)
      const payload = JSON.parse(base64UrlDecode(parts[1])) as JwtPayload;

      return {
        userId: payload.sub,
        email: payload.email,
        name: payload.name,
        emailVerified: payload.email_verified,
      };
    } catch (error) {
      console.error('Failed to decode ID token:', error);
      return null;
    }
  }
}
