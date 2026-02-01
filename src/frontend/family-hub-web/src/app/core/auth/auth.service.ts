import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { UserProfile, AuthTokens, JwtPayload } from './auth.models';
import {
  generateCodeVerifier,
  generateCodeChallenge,
  generateState,
  base64UrlDecode,
} from '../../shared/utils/crypto.utils';

/**
 * Authentication service implementing OAuth 2.0 Authorization Code Flow with PKCE
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

  constructor(
    private http: HttpClient,
    private router: Router,
  ) {
    this.checkAuthStatus();
  }

  /**
   * Check if user is currently authenticated on service initialization
   */
  private checkAuthStatus(): void {
    const accessToken = this.getAccessToken();
    if (accessToken && !this.isTokenExpired()) {
      const profile = this.getUserProfileFromToken();
      if (profile) {
        this.userProfile.set(profile);
        this.isAuthenticated.set(true);
      }
    } else {
      this.clearTokens();
    }
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
    const authUrl = new URL(`${environment.keycloak.issuer}/protocol/openid-connect/auth`);
    authUrl.searchParams.append('client_id', environment.keycloak.clientId);
    authUrl.searchParams.append('redirect_uri', environment.keycloak.redirectUri);
    authUrl.searchParams.append('response_type', 'code');
    authUrl.searchParams.append('scope', environment.keycloak.scope);
    authUrl.searchParams.append('code_challenge', codeChallenge);
    authUrl.searchParams.append('code_challenge_method', 'S256');
    authUrl.searchParams.append('state', state);

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
    const tokenUrl = `${environment.keycloak.issuer}/protocol/openid-connect/token`;
    const body = new URLSearchParams({
      grant_type: 'authorization_code',
      code,
      redirect_uri: environment.keycloak.redirectUri,
      client_id: environment.keycloak.clientId,
      code_verifier: codeVerifier,
    });

    try {
      const tokens = await this.http
        .post<AuthTokens>(tokenUrl, body.toString(), {
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        })
        .toPromise();

      if (!tokens) {
        throw new Error('No tokens received from Keycloak');
      }

      // Store tokens
      this.storeTokens(tokens);

      // Decode and set user profile
      const profile = this.getUserProfileFromToken();
      if (profile) {
        this.userProfile.set(profile);
        this.isAuthenticated.set(true);
      }

      // Clean up session storage
      sessionStorage.removeItem('code_verifier');
      sessionStorage.removeItem('state');

      // Redirect to intended destination
      const redirectUrl = sessionStorage.getItem('post_login_redirect') || '/dashboard';
      sessionStorage.removeItem('post_login_redirect');
      sessionStorage.removeItem('redirect_url');

      this.router.navigate([redirectUrl]);
    } catch (error) {
      console.error('Token exchange failed:', error);
      this.clearTokens();
      throw error;
    }
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
    const logoutUrl = new URL(`${environment.keycloak.issuer}/protocol/openid-connect/logout`);
    logoutUrl.searchParams.append('client_id', environment.keycloak.clientId);
    logoutUrl.searchParams.append(
      'post_logout_redirect_uri',
      environment.keycloak.postLogoutRedirectUri,
    );
    if (idToken) {
      logoutUrl.searchParams.append('id_token_hint', idToken);
    }

    // Redirect to Keycloak logout
    window.location.href = logoutUrl.toString();
  }

  /**
   * Refresh access token using refresh token
   */
  async refreshAccessToken(): Promise<boolean> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return false;
    }

    const tokenUrl = `${environment.keycloak.issuer}/protocol/openid-connect/token`;
    const body = new URLSearchParams({
      grant_type: 'refresh_token',
      refresh_token: refreshToken,
      client_id: environment.keycloak.clientId,
    });

    try {
      const tokens = await this.http
        .post<AuthTokens>(tokenUrl, body.toString(), {
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        })
        .toPromise();

      if (tokens) {
        this.storeTokens(tokens);
        return true;
      }
      return false;
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
    localStorage.setItem(this.STORAGE_ACCESS_TOKEN, tokens.accessToken);
    localStorage.setItem(this.STORAGE_ID_TOKEN, tokens.idToken);
    localStorage.setItem(this.STORAGE_REFRESH_TOKEN, tokens.refreshToken);

    // Calculate expiration time
    const expiresAt = Math.floor(Date.now() / 1000) + tokens.expiresIn;
    localStorage.setItem(this.STORAGE_EXPIRES_AT, expiresAt.toString());
  }

  /**
   * Clear all OAuth tokens from localStorage
   */
  private clearTokens(): void {
    localStorage.removeItem(this.STORAGE_ACCESS_TOKEN);
    localStorage.removeItem(this.STORAGE_ID_TOKEN);
    localStorage.removeItem(this.STORAGE_REFRESH_TOKEN);
    localStorage.removeItem(this.STORAGE_EXPIRES_AT);
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
