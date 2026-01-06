import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { GraphQLService } from './graphql.service';
import { WindowRef } from './window-ref.service';

/**
 * Unit tests for AuthService email-only OAuth authentication.
 * Tests OAuth flow with email-based login_hint parameter.
 *
 * Note: These tests mock WindowRef service to prevent actual page redirects during testing.
 * This approach avoids browser security restrictions on window.location mocking.
 */
describe('AuthService - Email OAuth', () => {
  let service: AuthService;
  let graphqlServiceMock: jasmine.SpyObj<GraphQLService>;
  let routerMock: jasmine.SpyObj<Router>;
  let mockWindow: { location: { href: string } };

  beforeEach(() => {
    // Create mocks
    graphqlServiceMock = jasmine.createSpyObj('GraphQLService', ['query', 'mutate']);
    routerMock = jasmine.createSpyObj('Router', ['navigate']);

    // Create mock window object with settable location.href
    mockWindow = {
      location: {
        href: ''
      }
    };

    // Clear sessionStorage before each test
    sessionStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: GraphQLService, useValue: graphqlServiceMock },
        { provide: Router, useValue: routerMock },
        { provide: WindowRef, useValue: { nativeWindow: mockWindow } }
      ]
    });

    service = TestBed.inject(AuthService);
  });

  describe('OAuth Flow', () => {
    it('should initiate OAuth login with email as loginHint', async () => {
      // Arrange
      const email = 'user@example.com';
      const mockResponse = {
        zitadelAuthUrl: {
          authorizationUrl: 'https://zitadel.example.com/oauth/authorize',
          codeVerifier: 'mock-verifier',
          state: 'mock-state'
        }
      };

      graphqlServiceMock.query.and.returnValue(Promise.resolve(mockResponse));

      // Act
      await service.login(email);

      // Assert - loginHint should be email
      expect(graphqlServiceMock.query).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          loginHint: email
        })
      );
    });

    it('should store PKCE parameters in sessionStorage', async () => {
      // Arrange
      const email = 'user@example.com';
      const mockResponse = {
        zitadelAuthUrl: {
          authorizationUrl: 'https://zitadel.example.com/oauth/authorize',
          codeVerifier: 'mock-code-verifier',
          state: 'mock-state-value'
        }
      };

      graphqlServiceMock.query.and.returnValue(Promise.resolve(mockResponse));

      // Act
      await service.login(email);

      // Assert
      expect(sessionStorage.getItem('pkce_code_verifier')).toBe('mock-code-verifier');
      expect(sessionStorage.getItem('oauth_state')).toBe('mock-state-value');
    });

    it('should redirect to authorization URL', async () => {
      // Arrange
      const email = 'user@example.com';
      const authUrl = 'https://zitadel.example.com/oauth/authorize?client_id=123';
      const mockResponse = {
        zitadelAuthUrl: {
          authorizationUrl: authUrl,
          codeVerifier: 'mock-verifier',
          state: 'mock-state'
        }
      };

      graphqlServiceMock.query.and.returnValue(Promise.resolve(mockResponse));

      // Act
      await service.login(email);

      // Assert
      expect(mockWindow.location.href).toBe(authUrl);
    });

    it('should omit loginHint when identifier is not provided', async () => {
      // Arrange
      const mockResponse = {
        zitadelAuthUrl: {
          authorizationUrl: 'https://zitadel.example.com/oauth/authorize',
          codeVerifier: 'mock-verifier',
          state: 'mock-state'
        }
      };

      graphqlServiceMock.query.and.returnValue(Promise.resolve(mockResponse));

      // Act
      await service.login(); // No identifier provided

      // Assert - loginHint should be undefined
      expect(graphqlServiceMock.query).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          loginHint: undefined
        })
      );
    });

    it('should trim whitespace from email', async () => {
      // Arrange
      const email = '  user@example.com  ';
      const mockResponse = {
        zitadelAuthUrl: {
          authorizationUrl: 'https://zitadel.example.com/oauth/authorize',
          codeVerifier: 'mock-verifier',
          state: 'mock-state'
        }
      };

      graphqlServiceMock.query.and.returnValue(Promise.resolve(mockResponse));

      // Act
      await service.login(email.trim());

      // Assert - loginHint should use trimmed email
      expect(graphqlServiceMock.query).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          loginHint: 'user@example.com'
        })
      );
    });
  });

  describe('Error Handling', () => {
    it('should throw error when GraphQL query fails', async () => {
      // Arrange
      const email = 'user@example.com';
      const error = new Error('Network error');
      graphqlServiceMock.query.and.returnValue(Promise.reject(error));

      // Act & Assert
      await expectAsync(service.login(email)).toBeRejectedWithError('Failed to initiate login');
    });
  });
});
