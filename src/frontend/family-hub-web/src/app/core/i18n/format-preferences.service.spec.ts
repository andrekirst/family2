import { TestBed } from '@angular/core/testing';
import { FormatPreferencesService } from './format-preferences.service';

const DATE_FORMAT_KEY = 'familyhub-date-format';
const TIME_FORMAT_KEY = 'familyhub-time-format';
const LOCALE_STORAGE_KEY = 'familyhub-locale';

describe('FormatPreferencesService', () => {
  let service: FormatPreferencesService;

  beforeEach(() => {
    localStorage.removeItem(DATE_FORMAT_KEY);
    localStorage.removeItem(TIME_FORMAT_KEY);
    localStorage.removeItem(LOCALE_STORAGE_KEY);

    TestBed.configureTestingModule({
      providers: [FormatPreferencesService],
    });
    service = TestBed.inject(FormatPreferencesService);
  });

  afterEach(() => {
    localStorage.removeItem(DATE_FORMAT_KEY);
    localStorage.removeItem(TIME_FORMAT_KEY);
    localStorage.removeItem(LOCALE_STORAGE_KEY);
  });

  describe('dateFormat', () => {
    it('defaults to MM/DD/YYYY for en locale', () => {
      localStorage.setItem(LOCALE_STORAGE_KEY, 'en');
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({ providers: [FormatPreferencesService] });
      const svc = TestBed.inject(FormatPreferencesService);
      expect(svc.dateFormat()).toBe('MM/DD/YYYY');
    });

    it('defaults to DD.MM.YYYY for de locale', () => {
      localStorage.setItem(LOCALE_STORAGE_KEY, 'de');
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({ providers: [FormatPreferencesService] });
      const svc = TestBed.inject(FormatPreferencesService);
      expect(svc.dateFormat()).toBe('DD.MM.YYYY');
    });

    it('reads stored value from localStorage', () => {
      localStorage.setItem(DATE_FORMAT_KEY, 'DD.MM.YYYY');
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({ providers: [FormatPreferencesService] });
      const svc = TestBed.inject(FormatPreferencesService);
      expect(svc.dateFormat()).toBe('DD.MM.YYYY');
    });

    it('ignores invalid stored values', () => {
      localStorage.setItem(DATE_FORMAT_KEY, 'YYYY-MM-DD');
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({ providers: [FormatPreferencesService] });
      const svc = TestBed.inject(FormatPreferencesService);
      expect(['DD.MM.YYYY', 'MM/DD/YYYY']).toContain(svc.dateFormat());
    });
  });

  describe('timeFormat', () => {
    it('defaults to 12h for en locale', () => {
      localStorage.setItem(LOCALE_STORAGE_KEY, 'en');
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({ providers: [FormatPreferencesService] });
      const svc = TestBed.inject(FormatPreferencesService);
      expect(svc.timeFormat()).toBe('12h');
    });

    it('defaults to 24h for de locale', () => {
      localStorage.setItem(LOCALE_STORAGE_KEY, 'de');
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({ providers: [FormatPreferencesService] });
      const svc = TestBed.inject(FormatPreferencesService);
      expect(svc.timeFormat()).toBe('24h');
    });

    it('reads stored value from localStorage', () => {
      localStorage.setItem(TIME_FORMAT_KEY, '24h');
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({ providers: [FormatPreferencesService] });
      const svc = TestBed.inject(FormatPreferencesService);
      expect(svc.timeFormat()).toBe('24h');
    });
  });

  describe('setDateFormat', () => {
    it('updates signal and localStorage', () => {
      service.setDateFormat('DD.MM.YYYY');
      expect(service.dateFormat()).toBe('DD.MM.YYYY');
      expect(localStorage.getItem(DATE_FORMAT_KEY)).toBe('DD.MM.YYYY');
    });

    it('can switch between formats', () => {
      service.setDateFormat('DD.MM.YYYY');
      expect(service.dateFormat()).toBe('DD.MM.YYYY');

      service.setDateFormat('MM/DD/YYYY');
      expect(service.dateFormat()).toBe('MM/DD/YYYY');
      expect(localStorage.getItem(DATE_FORMAT_KEY)).toBe('MM/DD/YYYY');
    });
  });

  describe('setTimeFormat', () => {
    it('updates signal and localStorage', () => {
      service.setTimeFormat('24h');
      expect(service.timeFormat()).toBe('24h');
      expect(localStorage.getItem(TIME_FORMAT_KEY)).toBe('24h');
    });

    it('can switch between formats', () => {
      service.setTimeFormat('24h');
      expect(service.timeFormat()).toBe('24h');

      service.setTimeFormat('12h');
      expect(service.timeFormat()).toBe('12h');
      expect(localStorage.getItem(TIME_FORMAT_KEY)).toBe('12h');
    });
  });
});
