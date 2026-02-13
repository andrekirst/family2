import type { DateFormatPreference, TimeFormatPreference } from './format-preferences.service';

const DATE_FORMAT_KEY = 'familyhub-date-format';
const TIME_FORMAT_KEY = 'familyhub-time-format';
const LOCALE_STORAGE_KEY = 'familyhub-locale';

function localeDefault(): { date: DateFormatPreference; time: TimeFormatPreference } {
  const locale = localStorage.getItem(LOCALE_STORAGE_KEY) ?? 'en';
  return locale === 'de'
    ? { date: 'DD.MM.YYYY', time: '24h' }
    : { date: 'MM/DD/YYYY', time: '12h' };
}

/** Reads stored date format from localStorage (for non-DI contexts). */
export function getStoredDateFormat(): DateFormatPreference {
  const stored = localStorage.getItem(DATE_FORMAT_KEY);
  if (stored === 'DD.MM.YYYY' || stored === 'MM/DD/YYYY') return stored;
  return localeDefault().date;
}

/** Reads stored time format from localStorage (for non-DI contexts). */
export function getStoredTimeFormat(): TimeFormatPreference {
  const stored = localStorage.getItem(TIME_FORMAT_KEY);
  if (stored === '24h' || stored === '12h') return stored;
  return localeDefault().time;
}
