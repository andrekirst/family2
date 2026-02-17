import { TestBed } from '@angular/core/testing';
import { Apollo } from 'apollo-angular';
import { of, throwError } from 'rxjs';
import { GoogleIntegrationService } from './google-integration.service';
import { LinkedAccount } from '../models/google-integration.models';

describe('GoogleIntegrationService', () => {
  let service: GoogleIntegrationService;
  let mockApollo: { query: ReturnType<typeof vi.fn>; mutate: ReturnType<typeof vi.fn> };

  const mockAccount: LinkedAccount = {
    googleAccountId: 'google-123',
    googleEmail: 'test@gmail.com',
    status: 'Active',
    grantedScopes: 'openid email',
    lastSyncAt: null,
    createdAt: '2026-02-01T00:00:00Z',
  };

  beforeEach(() => {
    mockApollo = {
      query: vi.fn(),
      mutate: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [GoogleIntegrationService, { provide: Apollo, useValue: mockApollo }],
    });

    service = TestBed.inject(GoogleIntegrationService);
  });

  it('should create', () => {
    expect(service).toBeTruthy();
  });

  describe('loadLinkedAccounts', () => {
    it('should set linkedAccounts signal from query result', () => {
      mockApollo.query.mockReturnValue(
        of({ data: { googleIntegration: { linkedAccounts: [mockAccount] } } }),
      );

      service.loadLinkedAccounts();

      expect(service.linkedAccounts()).toEqual([mockAccount]);
      expect(service.loading()).toBe(false);
    });

    it('should set error signal on failure', () => {
      mockApollo.query.mockReturnValue(throwError(() => new Error('Network error')));

      service.loadLinkedAccounts();

      expect(service.error()).toBe('Network error');
      expect(service.linkedAccounts()).toEqual([]);
      expect(service.loading()).toBe(false);
    });
  });

  describe('linkGoogle', () => {
    it('should redirect to auth URL', () => {
      const authUrl = 'https://accounts.google.com/o/oauth2/v2/auth?test=true';
      mockApollo.query.mockReturnValue(of({ data: { googleIntegration: { authUrl } } }));

      // Mock window.location.href
      const originalLocation = window.location;
      Object.defineProperty(window, 'location', {
        writable: true,
        value: { ...originalLocation, href: '' },
      });

      service.linkGoogle();

      expect(window.location.href).toBe(authUrl);

      // Restore
      Object.defineProperty(window, 'location', {
        writable: true,
        value: originalLocation,
      });
    });
  });

  describe('unlinkGoogle', () => {
    it('should clear linkedAccounts on success', () => {
      // Pre-populate accounts
      mockApollo.query.mockReturnValue(
        of({ data: { googleIntegration: { linkedAccounts: [mockAccount] } } }),
      );
      service.loadLinkedAccounts();
      expect(service.linkedAccounts().length).toBe(1);

      // Unlink
      mockApollo.mutate.mockReturnValue(of({ data: { googleIntegration: { unlink: true } } }));
      service.unlinkGoogle();

      expect(service.linkedAccounts()).toEqual([]);
      expect(service.loading()).toBe(false);
    });
  });

  describe('refreshToken', () => {
    it('should reload accounts on success', () => {
      mockApollo.mutate.mockReturnValue(
        of({
          data: {
            googleIntegration: {
              refreshToken: { success: true, newExpiresAt: '2026-03-01T00:00:00Z' },
            },
          },
        }),
      );

      // Mock loadLinkedAccounts to be called by refreshToken
      mockApollo.query.mockReturnValue(
        of({ data: { googleIntegration: { linkedAccounts: [mockAccount] } } }),
      );

      service.refreshToken();

      // loadLinkedAccounts is called after success, which triggers a query
      expect(mockApollo.query).toHaveBeenCalled();
      expect(service.loading()).toBe(false);
    });
  });

  describe('computed signals', () => {
    it('isLinked should return true when accounts exist', () => {
      mockApollo.query.mockReturnValue(
        of({ data: { googleIntegration: { linkedAccounts: [mockAccount] } } }),
      );

      service.loadLinkedAccounts();

      expect(service.isLinked()).toBe(true);
    });

    it('primaryAccount should return first account', () => {
      mockApollo.query.mockReturnValue(
        of({ data: { googleIntegration: { linkedAccounts: [mockAccount] } } }),
      );

      service.loadLinkedAccounts();

      expect(service.primaryAccount()).toEqual(mockAccount);
    });
  });
});
