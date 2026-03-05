import { TestBed } from '@angular/core/testing';
import { HttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { EnvironmentConfigService } from '../config/environment-config.service';

/**
 * Build a minimal 3-part JWT with the given payload.
 * AuthService only decodes the payload (middle part) — it doesn't verify signatures.
 */
function buildMockIdToken(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'RS256', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload));
  const signature = 'mock-signature';
  return `${header}.${body}.${signature}`;
}

describe('AuthService', () => {
  let service: AuthService;
  let mockHttp: { post: ReturnType<typeof vi.fn> };
  let mockEnvConfig: { keycloak: Record<string, string> };

  const mockTokenPayload = {
    sub: 'user-123',
    email: 'test@example.com',
    name: 'Test User',
    email_verified: true,
    exp: Math.floor(Date.now() / 1000) + 3600,
    iat: Math.floor(Date.now() / 1000),
    iss: 'https://auth.example.com/realms/test',
    aud: 'familyhub-web',
  };

  const mockIdToken = buildMockIdToken(mockTokenPayload);

  const mockTokenResponse = {
    access_token: 'mock-access-token',
    id_token: mockIdToken,
    refresh_token: 'mock-refresh-token',
    expires_in: 3600,
    token_type: 'Bearer',
  };

  beforeEach(() => {
    // Clear storage BEFORE creating the service (constructor calls checkAuthStatus)
    localStorage.clear();
    sessionStorage.clear();

    mockHttp = {
      post: vi.fn(),
    };

    mockEnvConfig = {
      keycloak: {
        issuer: 'https://auth.example.com/realms/test',
        clientId: 'familyhub-web',
        redirectUri: 'http://localhost:4200/callback',
        postLogoutRedirectUri: 'http://localhost:4200',
        scope: 'openid profile email',
      },
    };

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: HttpClient, useValue: mockHttp },
        { provide: EnvironmentConfigService, useValue: mockEnvConfig },
      ],
    });

    service = TestBed.inject(AuthService);
  });

  afterEach(() => {
    localStorage.clear();
    sessionStorage.clear();
  });

  describe('handleCallback', () => {
    it('should exchange code, store tokens, and set profile', async () => {
      // Set up PKCE session state (simulating what login() stores)
      sessionStorage.setItem('state', 'test-state');
      sessionStorage.setItem('code_verifier', 'test-verifier');

      mockHttp.post.mockReturnValue(of(mockTokenResponse));

      await service.handleCallback('auth-code', 'test-state');

      // Tokens stored in localStorage
      expect(localStorage.getItem('access_token')).toBe('mock-access-token');
      expect(localStorage.getItem('id_token')).toBe(mockIdToken);
      expect(localStorage.getItem('refresh_token')).toBe('mock-refresh-token');
      expect(localStorage.getItem('expires_at')).toBeTruthy();

      // Authenticated state set
      expect(service.isAuthenticated()).toBe(true);

      // Profile decoded from ID token
      expect(service.userProfile()).toEqual(
        expect.objectContaining({
          userId: 'user-123',
          email: 'test@example.com',
          name: 'Test User',
          emailVerified: true,
        }),
      );

      // PKCE params cleaned up
      expect(sessionStorage.getItem('code_verifier')).toBeNull();
      expect(sessionStorage.getItem('state')).toBeNull();
    });

    it('should throw on state mismatch', async () => {
      sessionStorage.setItem('state', 'expected-state');
      sessionStorage.setItem('code_verifier', 'test-verifier');

      await expect(service.handleCallback('code', 'wrong-state')).rejects.toThrow(
        'Invalid state parameter',
      );

      expect(mockHttp.post).not.toHaveBeenCalled();
    });

    it('should throw when code verifier missing', async () => {
      sessionStorage.setItem('state', 'test-state');
      // No code_verifier stored

      await expect(service.handleCallback('code', 'test-state')).rejects.toThrow(
        'Code verifier not found',
      );
    });

    it('should clear tokens on exchange failure', async () => {
      sessionStorage.setItem('state', 'test-state');
      sessionStorage.setItem('code_verifier', 'test-verifier');

      // Pre-set a token to verify it gets cleared
      localStorage.setItem('access_token', 'old-token');

      mockHttp.post.mockReturnValue(throwError(() => new Error('Network error')));

      await expect(service.handleCallback('code', 'test-state')).rejects.toThrow('Network error');

      expect(localStorage.getItem('access_token')).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('refreshAccessToken', () => {
    it('should store new tokens on success', async () => {
      localStorage.setItem('refresh_token', 'existing-refresh-token');

      const newTokenResponse = {
        ...mockTokenResponse,
        access_token: 'new-access-token',
        refresh_token: 'new-refresh-token',
      };
      mockHttp.post.mockReturnValue(of(newTokenResponse));

      const success = await service.refreshAccessToken();

      expect(success).toBe(true);
      expect(localStorage.getItem('access_token')).toBe('new-access-token');
      expect(localStorage.getItem('refresh_token')).toBe('new-refresh-token');
    });

    it('should return false when no refresh token', async () => {
      // No refresh_token in localStorage

      const success = await service.refreshAccessToken();

      expect(success).toBe(false);
      expect(mockHttp.post).not.toHaveBeenCalled();
    });

    it('should clear tokens and return false on failure', async () => {
      localStorage.setItem('refresh_token', 'expired-refresh-token');
      localStorage.setItem('access_token', 'old-access-token');

      mockHttp.post.mockReturnValue(throwError(() => new Error('Refresh failed')));

      const success = await service.refreshAccessToken();

      expect(success).toBe(false);
      expect(localStorage.getItem('access_token')).toBeNull();
      expect(localStorage.getItem('refresh_token')).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
    });
  });
});
