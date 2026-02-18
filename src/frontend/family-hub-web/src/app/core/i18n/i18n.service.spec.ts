import { TestBed } from '@angular/core/testing';
import { I18nService, SupportedLocale } from './i18n.service';
import { Apollo } from 'apollo-angular';

const LOCALE_STORAGE_KEY = 'familyhub-locale';

describe('I18nService', () => {
  let service: I18nService;
  let apolloSpy: { mutate: ReturnType<typeof vi.fn> };
  let reloadMock: ReturnType<typeof vi.fn>;
  const originalLocation = window.location;

  beforeEach(() => {
    localStorage.removeItem(LOCALE_STORAGE_KEY);

    // Replace window.location with a mock that allows spying on reload
    reloadMock = vi.fn();
    Object.defineProperty(window, 'location', {
      value: { ...originalLocation, reload: reloadMock },
      writable: true,
      configurable: true,
    });

    apolloSpy = {
      mutate: vi.fn().mockReturnValue({ subscribe: vi.fn() }),
    };

    TestBed.configureTestingModule({
      providers: [I18nService, { provide: Apollo, useValue: apolloSpy }],
    });

    service = TestBed.inject(I18nService);
  });

  afterEach(() => {
    localStorage.removeItem(LOCALE_STORAGE_KEY);
    // Restore original window.location
    Object.defineProperty(window, 'location', {
      value: originalLocation,
      writable: true,
      configurable: true,
    });
    vi.restoreAllMocks();
  });

  describe('currentLocale', () => {
    it('defaults to browser language if no stored locale', () => {
      const locale = service.currentLocale();
      expect(['en', 'de']).toContain(locale);
    });

    it('reads stored locale from localStorage', () => {
      localStorage.setItem(LOCALE_STORAGE_KEY, 'de');
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({
        providers: [I18nService, { provide: Apollo, useValue: apolloSpy }],
      });
      const svc = TestBed.inject(I18nService);
      expect(svc.currentLocale()).toBe('de');
    });

    it('ignores invalid stored locale and falls back', () => {
      localStorage.setItem(LOCALE_STORAGE_KEY, 'fr');
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({
        providers: [I18nService, { provide: Apollo, useValue: apolloSpy }],
      });
      const svc = TestBed.inject(I18nService);
      expect(svc.currentLocale()).not.toBe('fr');
      expect(['en', 'de']).toContain(svc.currentLocale());
    });
  });

  describe('supportedLocales', () => {
    it('contains en and de', () => {
      expect(service.supportedLocales).toContain('en');
      expect(service.supportedLocales).toContain('de');
    });

    it('has exactly 2 supported locales', () => {
      expect(service.supportedLocales).toHaveLength(2);
    });
  });

  describe('switchLocale', () => {
    it('saves locale to localStorage', () => {
      service.switchLocale('de');
      expect(localStorage.getItem(LOCALE_STORAGE_KEY)).toBe('de');
    });

    it('updates the signal', () => {
      service.switchLocale('de');
      expect(service.currentLocale()).toBe('de');
    });

    it('syncs locale to backend via Apollo mutation', () => {
      service.switchLocale('de');
      expect(apolloSpy.mutate).toHaveBeenCalledWith(
        expect.objectContaining({
          variables: { input: { locale: 'de' } },
        }),
      );
    });

    it('triggers page reload', () => {
      service.switchLocale('de');
      expect(reloadMock).toHaveBeenCalled();
    });

    it('ignores unsupported locale', () => {
      service.switchLocale('fr' as SupportedLocale);
      expect(localStorage.getItem(LOCALE_STORAGE_KEY)).toBeNull();
      expect(reloadMock).not.toHaveBeenCalled();
    });
  });

  describe('applyBackendLocale', () => {
    it('saves locale to localStorage', () => {
      service.applyBackendLocale('de');
      expect(localStorage.getItem(LOCALE_STORAGE_KEY)).toBe('de');
    });

    it('reloads when locale differs from current', () => {
      localStorage.setItem(LOCALE_STORAGE_KEY, 'en');
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({
        providers: [I18nService, { provide: Apollo, useValue: apolloSpy }],
      });
      const svc = TestBed.inject(I18nService);

      svc.applyBackendLocale('de');

      expect(reloadMock).toHaveBeenCalled();
    });

    it('does not reload when locale matches current', () => {
      localStorage.setItem(LOCALE_STORAGE_KEY, 'en');
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({
        providers: [I18nService, { provide: Apollo, useValue: apolloSpy }],
      });
      const svc = TestBed.inject(I18nService);

      svc.applyBackendLocale('en');

      expect(reloadMock).not.toHaveBeenCalled();
    });

    it('ignores unsupported locale', () => {
      service.applyBackendLocale('fr');
      expect(reloadMock).not.toHaveBeenCalled();
    });
  });

  describe('getLocaleForHeader', () => {
    it('returns current locale string', () => {
      const locale = service.getLocaleForHeader();
      expect(typeof locale).toBe('string');
      expect(['en', 'de']).toContain(locale);
    });
  });
});
