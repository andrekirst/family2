import { Injectable, signal } from '@angular/core';

export type DateFormatPreference = 'DD.MM.YYYY' | 'MM/DD/YYYY';
export type TimeFormatPreference = '24h' | '12h';

const DATE_FORMAT_KEY = 'familyhub-date-format';
const TIME_FORMAT_KEY = 'familyhub-time-format';
const LOCALE_STORAGE_KEY = 'familyhub-locale';

function localeDefault(): { date: DateFormatPreference; time: TimeFormatPreference } {
  const locale = localStorage.getItem(LOCALE_STORAGE_KEY) ?? 'en';
  return locale === 'de'
    ? { date: 'DD.MM.YYYY', time: '24h' }
    : { date: 'MM/DD/YYYY', time: '12h' };
}

@Injectable({ providedIn: 'root' })
export class FormatPreferencesService {
  readonly dateFormat = signal<DateFormatPreference>(this.loadDateFormat());
  readonly timeFormat = signal<TimeFormatPreference>(this.loadTimeFormat());

  setDateFormat(format: DateFormatPreference): void {
    localStorage.setItem(DATE_FORMAT_KEY, format);
    this.dateFormat.set(format);
  }

  setTimeFormat(format: TimeFormatPreference): void {
    localStorage.setItem(TIME_FORMAT_KEY, format);
    this.timeFormat.set(format);
  }

  private loadDateFormat(): DateFormatPreference {
    const stored = localStorage.getItem(DATE_FORMAT_KEY);
    if (stored === 'DD.MM.YYYY' || stored === 'MM/DD/YYYY') return stored;
    return localeDefault().date;
  }

  private loadTimeFormat(): TimeFormatPreference {
    const stored = localStorage.getItem(TIME_FORMAT_KEY);
    if (stored === '24h' || stored === '12h') return stored;
    return localeDefault().time;
  }
}
