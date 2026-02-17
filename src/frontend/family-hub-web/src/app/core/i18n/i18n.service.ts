import { Injectable, Injector, inject, signal } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { UPDATE_MY_LOCALE_MUTATION } from '../../features/auth/graphql/auth.operations';

export type SupportedLocale = 'en' | 'de';

const LOCALE_STORAGE_KEY = 'familyhub-locale';
const DEFAULT_LOCALE: SupportedLocale = 'en';
const SUPPORTED_LOCALES: SupportedLocale[] = ['en', 'de'];

/**
 * Manages the user's locale preference.
 * Reads from localStorage for instant access at bootstrap (before auth).
 * Provides signal-based reactive access to the current locale.
 *
 * Language switching triggers a page reload because `loadTranslations()`
 * is called at bootstrap time and cannot be hot-swapped.
 *
 * NOTE: Apollo is resolved lazily via Injector to avoid a circular dependency:
 * Apollo config injects I18nService for Accept-Language headers,
 * so I18nService cannot eagerly inject Apollo.
 */
@Injectable({ providedIn: 'root' })
export class I18nService {
  private readonly injector = inject(Injector);

  /** Current locale as a reactive signal */
  readonly currentLocale = signal<SupportedLocale>(this.getStoredLocale());

  /** All supported locales */
  readonly supportedLocales = SUPPORTED_LOCALES;

  /**
   * Get the stored locale from localStorage, falling back to browser language or default.
   */
  private getStoredLocale(): SupportedLocale {
    const stored = localStorage.getItem(LOCALE_STORAGE_KEY);
    if (stored && SUPPORTED_LOCALES.includes(stored as SupportedLocale)) {
      return stored as SupportedLocale;
    }

    // Fall back to browser language
    const browserLang = navigator.language.split('-')[0];
    if (SUPPORTED_LOCALES.includes(browserLang as SupportedLocale)) {
      return browserLang as SupportedLocale;
    }

    return DEFAULT_LOCALE;
  }

  /**
   * Switch the UI language. Saves to localStorage, syncs to backend (fire-and-forget),
   * and reloads the page so `loadTranslations()` picks up the new locale at bootstrap.
   */
  switchLocale(locale: SupportedLocale): void {
    if (!SUPPORTED_LOCALES.includes(locale)) return;

    localStorage.setItem(LOCALE_STORAGE_KEY, locale);
    this.currentLocale.set(locale);

    // Sync to backend (fire-and-forget, don't wait before reload)
    this.syncLocaleToBackend(locale);

    // Reload to re-run loadTranslations() at bootstrap
    window.location.reload();
  }

  /**
   * Apply a locale from the backend (e.g. after login) without reloading
   * if it already matches the current locale.
   */
  applyBackendLocale(locale: string): void {
    if (!SUPPORTED_LOCALES.includes(locale as SupportedLocale)) return;

    const typedLocale = locale as SupportedLocale;
    localStorage.setItem(LOCALE_STORAGE_KEY, typedLocale);

    // Only reload if the locale actually changed
    if (this.currentLocale() !== typedLocale) {
      this.currentLocale.set(typedLocale);
      window.location.reload();
    }
  }

  /**
   * Get the current locale string for use in Accept-Language headers.
   */
  getLocaleForHeader(): string {
    return this.currentLocale();
  }

  /**
   * Persist the locale preference to the backend database.
   * Fire-and-forget — errors are logged but don't block the UI.
   */
  private syncLocaleToBackend(locale: SupportedLocale): void {
    const apollo = this.injector.get(Apollo);
    apollo
      .mutate({
        mutation: UPDATE_MY_LOCALE_MUTATION,
        variables: { input: { locale } },
      })
      .subscribe({
        error: (err) => console.warn('Failed to sync locale to backend:', err),
      });
  }
}
