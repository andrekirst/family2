import '@angular/localize/init';
import { loadTranslations } from '@angular/localize';
import { registerLocaleData } from '@angular/common';
import localeDe from '@angular/common/locales/de';
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

import en from './app/core/i18n/translations/en.json';
import de from './app/core/i18n/translations/de.json';

// Register German locale data for DatePipe, DecimalPipe, etc.
registerLocaleData(localeDe);

// Determine locale from localStorage (instant, pre-auth access)
const LOCALE_STORAGE_KEY = 'familyhub-locale';
const stored = localStorage.getItem(LOCALE_STORAGE_KEY);
const browserLang = navigator.language.split('-')[0];
export const locale: string =
  stored && ['en', 'de'].includes(stored)
    ? stored
    : ['en', 'de'].includes(browserLang)
      ? browserLang
      : 'en';

// Load translations for the resolved locale (en translations are identity-mapped)
const translations: Record<string, Record<string, string>> = { en, de };
loadTranslations(translations[locale] ?? en);

bootstrapApplication(App, appConfig).catch((err) => console.error(err));
