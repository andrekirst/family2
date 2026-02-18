import {
  getDayStart,
  getDayEnd,
  formatDayLabel,
  isToday,
} from './day.utils';

const LOCALE_STORAGE_KEY = 'familyhub-locale';

// ── getDayStart ──────────────────────────────────────────────────────

describe('getDayStart', () => {
  it('returns 00:00:00.000 for a date with time', () => {
    const date = new Date(2026, 1, 11, 14, 30, 45, 123);
    const result = getDayStart(date);
    expect(result.getHours()).toBe(0);
    expect(result.getMinutes()).toBe(0);
    expect(result.getSeconds()).toBe(0);
    expect(result.getMilliseconds()).toBe(0);
  });

  it('preserves the date portion', () => {
    const date = new Date(2026, 1, 11, 9, 0);
    const result = getDayStart(date);
    expect(result.getFullYear()).toBe(2026);
    expect(result.getMonth()).toBe(1);
    expect(result.getDate()).toBe(11);
  });

  it('returns midnight when input is already midnight', () => {
    const date = new Date(2026, 1, 11, 0, 0, 0, 0);
    const result = getDayStart(date);
    expect(result.getHours()).toBe(0);
    expect(result.getMinutes()).toBe(0);
    expect(result.getSeconds()).toBe(0);
    expect(result.getMilliseconds()).toBe(0);
  });

  it('does not mutate the original date', () => {
    const date = new Date(2026, 1, 11, 14, 30, 0, 0);
    getDayStart(date);
    expect(date.getHours()).toBe(14);
  });

  it('handles first day of year', () => {
    const date = new Date(2026, 0, 1, 23, 59, 59, 999);
    const result = getDayStart(date);
    expect(result.getFullYear()).toBe(2026);
    expect(result.getMonth()).toBe(0);
    expect(result.getDate()).toBe(1);
    expect(result.getHours()).toBe(0);
  });
});

// ── getDayEnd ────────────────────────────────────────────────────────

describe('getDayEnd', () => {
  it('returns 23:59:59.999 for a date with time', () => {
    const date = new Date(2026, 1, 11, 9, 0, 0, 0);
    const result = getDayEnd(date);
    expect(result.getHours()).toBe(23);
    expect(result.getMinutes()).toBe(59);
    expect(result.getSeconds()).toBe(59);
    expect(result.getMilliseconds()).toBe(999);
  });

  it('preserves the date portion', () => {
    const date = new Date(2026, 1, 11, 9, 0);
    const result = getDayEnd(date);
    expect(result.getFullYear()).toBe(2026);
    expect(result.getMonth()).toBe(1);
    expect(result.getDate()).toBe(11);
  });

  it('returns end-of-day when input is already end-of-day', () => {
    const date = new Date(2026, 1, 11, 23, 59, 59, 999);
    const result = getDayEnd(date);
    expect(result.getHours()).toBe(23);
    expect(result.getMinutes()).toBe(59);
    expect(result.getSeconds()).toBe(59);
    expect(result.getMilliseconds()).toBe(999);
  });

  it('does not mutate the original date', () => {
    const date = new Date(2026, 1, 11, 9, 0, 0, 0);
    getDayEnd(date);
    expect(date.getHours()).toBe(9);
  });

  it('handles last day of year', () => {
    const date = new Date(2026, 11, 31, 0, 0, 0, 0);
    const result = getDayEnd(date);
    expect(result.getFullYear()).toBe(2026);
    expect(result.getMonth()).toBe(11);
    expect(result.getDate()).toBe(31);
    expect(result.getHours()).toBe(23);
    expect(result.getMilliseconds()).toBe(999);
  });
});

// ── formatDayLabel ───────────────────────────────────────────────────

describe('formatDayLabel', () => {
  beforeEach(() => localStorage.setItem(LOCALE_STORAGE_KEY, 'en'));
  afterEach(() => localStorage.removeItem(LOCALE_STORAGE_KEY));

  it('includes weekday, month name, day number and year for English', () => {
    const date = new Date(2026, 1, 11); // Wed Feb 11 2026
    const result = formatDayLabel(date);
    expect(result).toContain('Wednesday');
    expect(result).toContain('February');
    expect(result).toContain('11');
    expect(result).toContain('2026');
  });

  it('formats Monday correctly', () => {
    const date = new Date(2026, 1, 9); // Mon Feb 9 2026
    const result = formatDayLabel(date);
    expect(result).toContain('Monday');
    expect(result).toContain('February');
    expect(result).toContain('9');
    expect(result).toContain('2026');
  });

  it('formats first of January correctly', () => {
    const date = new Date(2026, 0, 1); // Thu Jan 1 2026
    const result = formatDayLabel(date);
    expect(result).toContain('Thursday');
    expect(result).toContain('January');
    expect(result).toContain('1');
    expect(result).toContain('2026');
  });

  it('uses German locale when stored locale is de', () => {
    localStorage.setItem(LOCALE_STORAGE_KEY, 'de');
    const date = new Date(2026, 1, 11); // Wed Feb 11 2026
    const result = formatDayLabel(date);
    expect(result).toContain('2026');
    expect(result).toContain('11');
    // German weekday name for Wednesday
    expect(result.toLowerCase()).toContain('mittwoch');
  });
});

// ── isToday ──────────────────────────────────────────────────────────

describe('isToday', () => {
  it('returns true for today', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 11, 14, 0));
    expect(isToday(new Date(2026, 1, 11))).toBe(true);
    vi.useRealTimers();
  });

  it('returns false for yesterday', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 11, 14, 0));
    expect(isToday(new Date(2026, 1, 10))).toBe(false);
    vi.useRealTimers();
  });

  it('returns false for tomorrow', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 11, 14, 0));
    expect(isToday(new Date(2026, 1, 12))).toBe(false);
    vi.useRealTimers();
  });

  it('returns false for same day in a different month', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 11, 14, 0));
    expect(isToday(new Date(2026, 2, 11))).toBe(false);
    vi.useRealTimers();
  });

  it('returns false for same day and month in a different year', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 11, 14, 0));
    expect(isToday(new Date(2025, 1, 11))).toBe(false);
    vi.useRealTimers();
  });

  it('returns true regardless of time of day', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 1, 11, 23, 59, 59, 999));
    expect(isToday(new Date(2026, 1, 11, 0, 0, 0, 0))).toBe(true);
    vi.useRealTimers();
  });
});
