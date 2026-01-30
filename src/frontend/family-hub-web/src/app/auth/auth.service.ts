import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';

const ACCESS_TOKEN_KEY = 'family_hub_access_token';
const REFRESH_TOKEN_KEY = 'family_hub_refresh_token';
const TOKEN_EXPIRES_KEY = 'family_hub_token_expires';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  // Reactive signal for auth state
  private _isAuthenticated = signal(false);
  isAuthenticated = computed(() => this._isAuthenticated());

  constructor(private router: Router) {
    // Check if user is authenticated on init
    this.checkAuth();
  }

  private checkAuth(): void {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    const expires = localStorage.getItem(TOKEN_EXPIRES_KEY);

    if (token && expires) {
      const expiresAt = new Date(expires);
      const now = new Date();

      // Check if token is still valid (with 1 minute buffer)
      if (expiresAt > new Date(now.getTime() + 60 * 1000)) {
        this._isAuthenticated.set(true);
      } else {
        this.clearTokens();
      }
    }
  }

  saveTokens(accessToken: string, refreshToken: string, expiresAt: string): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(TOKEN_EXPIRES_KEY, expiresAt);
    this._isAuthenticated.set(true);
  }

  clearTokens(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(TOKEN_EXPIRES_KEY);
    this._isAuthenticated.set(false);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  getCurrentUser(): any {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      // Decode JWT (simplified - no validation, just parsing)
      const payload = JSON.parse(atob(token.split('.')[1]));
      return {
        id: payload.sub,
        email: payload.email,
      };
    } catch {
      return null;
    }
  }

  logout(): void {
    this.clearTokens();
    this.router.navigate(['/login']);
  }
}
