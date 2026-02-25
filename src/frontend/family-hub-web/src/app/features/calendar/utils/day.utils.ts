import { getStoredLocale } from './week.utils';

// ── Date Calculation ────────────────────────────────────────────────

/** Returns the given `date` with time set to 00:00:00.000 (start of day). */
export function getDayStart(date: Date): Date {
  const d = new Date(date);
  d.setHours(0, 0, 0, 0);
  return d;
}

/** Returns the given `date` with time set to 23:59:59.999 (end of day). */
export function getDayEnd(date: Date): Date {
  const d = new Date(date);
  d.setHours(23, 59, 59, 999);
  return d;
}

// ── Formatting ──────────────────────────────────────────────────────

/**
 * Formats a day navigation label, e.g.:
 * - "Monday, January 27, 2026"
 */
export function formatDayLabel(date: Date): string {
  const locale = getStoredLocale();
  return date.toLocaleDateString(locale, {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  });
}

// ── Date Predicates ─────────────────────────────────────────────────

/** Returns true if `date` falls on today's calendar date. */
export function isToday(date: Date): boolean {
  const today = new Date();
  return (
    date.getFullYear() === today.getFullYear() &&
    date.getMonth() === today.getMonth() &&
    date.getDate() === today.getDate()
  );
}
