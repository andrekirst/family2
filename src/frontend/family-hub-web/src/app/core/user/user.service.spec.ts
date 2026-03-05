import { TestBed } from '@angular/core/testing';
import { Apollo } from 'apollo-angular';
import { of, throwError } from 'rxjs';
import { UserService, CurrentUser } from './user.service';
import { I18nService } from '../i18n/i18n.service';

describe('UserService', () => {
  let service: UserService;
  let mockApollo: { mutate: ReturnType<typeof vi.fn>; query: ReturnType<typeof vi.fn> };
  let mockI18n: { applyBackendLocale: ReturnType<typeof vi.fn> };

  const mockUser: CurrentUser = {
    id: 'user-1',
    email: 'test@example.com',
    name: 'Test User',
    emailVerified: true,
    isActive: true,
    familyId: 'family-1',
    avatarId: null,
    permissions: ['read', 'write'],
    preferredLocale: 'de',
  };

  function createService(): void {
    TestBed.configureTestingModule({
      providers: [
        UserService,
        { provide: Apollo, useValue: mockApollo },
        { provide: I18nService, useValue: mockI18n },
      ],
    });

    service = TestBed.inject(UserService);
  }

  beforeEach(() => {
    mockApollo = {
      mutate: vi.fn(),
      query: vi.fn(),
    };

    mockI18n = {
      applyBackendLocale: vi.fn(),
    };
  });

  describe('registerUser', () => {
    beforeEach(() => createService());

    it('should set currentUser signal on success', async () => {
      mockApollo.mutate.mockReturnValue(of({ data: { registerUser: mockUser } }));

      await service.registerUser();

      expect(service.currentUser()).toEqual(mockUser);
    });

    it('should send mutation with errorPolicy none', async () => {
      mockApollo.mutate.mockReturnValue(of({ data: { registerUser: mockUser } }));

      await service.registerUser();

      expect(mockApollo.mutate).toHaveBeenCalledWith(
        expect.objectContaining({ errorPolicy: 'none' }),
      );
    });

    it('should apply preferredLocale via i18nService', async () => {
      mockApollo.mutate.mockReturnValue(of({ data: { registerUser: mockUser } }));

      await service.registerUser();

      expect(mockI18n.applyBackendLocale).toHaveBeenCalledWith('de');
    });

    it('should skip locale when preferredLocale absent', async () => {
      const userWithoutLocale = { ...mockUser, preferredLocale: undefined };
      mockApollo.mutate.mockReturnValue(of({ data: { registerUser: userWithoutLocale } }));

      await service.registerUser();

      expect(mockI18n.applyBackendLocale).not.toHaveBeenCalled();
    });

    it('should retry after 1s when first attempt fails', async () => {
      vi.useFakeTimers();

      mockApollo.mutate
        .mockReturnValueOnce(throwError(() => new Error('Network error')))
        .mockReturnValueOnce(of({ data: { registerUser: mockUser } }));

      const promise = service.registerUser();

      // Advance past the 1-second retry delay
      await vi.advanceTimersByTimeAsync(1000);

      const result = await promise;

      expect(mockApollo.mutate).toHaveBeenCalledTimes(2);
      expect(result).toEqual(mockUser);

      vi.useRealTimers();
    });

    // These tests use real timers (wait actual 1s for retry) to avoid
    // unhandled rejection warnings from the fake-timer + async promise interaction.
    it('should throw when both attempts fail', async () => {
      mockApollo.mutate.mockReturnValue(throwError(() => new Error('Persistent error')));

      await expect(service.registerUser()).rejects.toThrow('Persistent error');
      expect(mockApollo.mutate).toHaveBeenCalledTimes(2);
    }, 3000);

    it('should throw when mutation returns null data', async () => {
      mockApollo.mutate.mockReturnValue(of({ data: null }));

      await expect(service.registerUser()).rejects.toThrow('Backend registration returned null');
    }, 3000);
  });

  describe('fetchCurrentUser', () => {
    beforeEach(() => createService());

    it('should set currentUser signal on success', async () => {
      mockApollo.query.mockReturnValue(of({ data: { me: { profile: mockUser } } }));

      const result = await service.fetchCurrentUser();

      expect(result).toEqual(mockUser);
      expect(service.currentUser()).toEqual(mockUser);
    });

    it('should return null when profile missing', async () => {
      mockApollo.query.mockReturnValue(of({ data: { me: { profile: null } } }));

      const result = await service.fetchCurrentUser();

      expect(result).toBeNull();
    });
  });

  describe('whenReady', () => {
    beforeEach(() => createService());

    it('should return immediately when user already set', async () => {
      // Pre-populate the user signal
      mockApollo.mutate.mockReturnValue(of({ data: { registerUser: mockUser } }));
      await service.registerUser();

      const result = await service.whenReady();

      // query should NOT be called — user is already available
      expect(mockApollo.query).not.toHaveBeenCalled();
      expect(result).toEqual(mockUser);
    });

    it('should return in-flight promise when registerUser running', async () => {
      mockApollo.mutate.mockReturnValue(of({ data: { registerUser: mockUser } }));

      // Start registerUser (sets _readyPromise)
      const registerPromise = service.registerUser();

      // Call whenReady while registerUser is in-flight
      const readyPromise = service.whenReady();

      const [registerResult, readyResult] = await Promise.all([registerPromise, readyPromise]);

      expect(registerResult).toEqual(mockUser);
      expect(readyResult).toEqual(mockUser);
      // Should share the same promise, not trigger a new query
      expect(mockApollo.query).not.toHaveBeenCalled();
    });

    it('should trigger fetchCurrentUser on cold start', async () => {
      mockApollo.query.mockReturnValue(of({ data: { me: { profile: mockUser } } }));

      const result = await service.whenReady();

      expect(mockApollo.query).toHaveBeenCalledTimes(1);
      expect(result).toEqual(mockUser);
    });
  });

  describe('clearUser', () => {
    beforeEach(() => createService());

    it('should reset currentUser and ready promise', async () => {
      // Pre-populate
      mockApollo.mutate.mockReturnValue(of({ data: { registerUser: mockUser } }));
      await service.registerUser();
      expect(service.currentUser()).toEqual(mockUser);

      service.clearUser();

      expect(service.currentUser()).toBeNull();

      // Next whenReady should trigger a fresh query
      mockApollo.query.mockReturnValue(of({ data: { me: { profile: mockUser } } }));
      await service.whenReady();
      expect(mockApollo.query).toHaveBeenCalledTimes(1);
    });
  });

  describe('isLoading', () => {
    beforeEach(() => createService());

    it('should be true during registerUser and false after', async () => {
      // Track isLoading values during execution
      const loadingStates: boolean[] = [];
      mockApollo.mutate.mockImplementation(() => {
        loadingStates.push(service.isLoading());
        return of({ data: { registerUser: mockUser } });
      });

      await service.registerUser();

      expect(loadingStates).toContain(true);
      expect(service.isLoading()).toBe(false);
    });

    it('should be false after registerUser failure', async () => {
      mockApollo.mutate.mockReturnValue(throwError(() => new Error('fail')));

      try {
        await service.registerUser();
      } catch {
        // Expected
      }

      expect(service.isLoading()).toBe(false);
    }, 3000);
  });
});
