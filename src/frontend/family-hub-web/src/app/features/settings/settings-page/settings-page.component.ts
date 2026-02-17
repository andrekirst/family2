import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { I18nService, SupportedLocale } from '../../../core/i18n/i18n.service';
import {
  FormatPreferencesService,
  DateFormatPreference,
  TimeFormatPreference,
} from '../../../core/i18n/format-preferences.service';
import { UserService } from '../../../core/user/user.service';
import { TopBarService } from '../../../shared/services/top-bar.service';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="max-w-2xl mx-auto space-y-6">
      <!-- Profile Card -->
      <div class="bg-white shadow rounded-lg p-6">
        <h2 class="text-lg font-semibold text-gray-900 mb-4" i18n="@@settings.profile">Profile</h2>
        <div class="space-y-3">
          <div>
            <span class="text-sm font-medium text-gray-500" i18n="@@settings.name">Name</span>
            <p class="mt-1 text-gray-900">{{ userService.currentUser()?.name ?? '—' }}</p>
          </div>
          <div>
            <span class="text-sm font-medium text-gray-500" i18n="@@settings.email">Email</span>
            <p class="mt-1 text-gray-900">{{ userService.currentUser()?.email ?? '—' }}</p>
          </div>
        </div>
      </div>

      <!-- Language Card -->
      <div class="bg-white shadow rounded-lg p-6">
        <h2 class="text-lg font-semibold text-gray-900" i18n="@@settings.language">Language</h2>
        <p class="mt-1 text-sm text-gray-500 mb-4" i18n="@@settings.languageDesc">
          Choose your preferred language
        </p>
        <div class="flex gap-3">
          <button
            (click)="switchLanguage('en')"
            class="flex-1 px-4 py-2.5 text-sm font-medium rounded-lg border transition-colors"
            [class.border-blue-500]="i18n.currentLocale() === 'en'"
            [class.bg-blue-50]="i18n.currentLocale() === 'en'"
            [class.text-blue-700]="i18n.currentLocale() === 'en'"
            [class.border-gray-300]="i18n.currentLocale() !== 'en'"
            [class.text-gray-700]="i18n.currentLocale() !== 'en'"
            [class.hover:bg-gray-50]="i18n.currentLocale() !== 'en'"
            data-testid="lang-en"
          >
            English
          </button>
          <button
            (click)="switchLanguage('de')"
            class="flex-1 px-4 py-2.5 text-sm font-medium rounded-lg border transition-colors"
            [class.border-blue-500]="i18n.currentLocale() === 'de'"
            [class.bg-blue-50]="i18n.currentLocale() === 'de'"
            [class.text-blue-700]="i18n.currentLocale() === 'de'"
            [class.border-gray-300]="i18n.currentLocale() !== 'de'"
            [class.text-gray-700]="i18n.currentLocale() !== 'de'"
            [class.hover:bg-gray-50]="i18n.currentLocale() !== 'de'"
            data-testid="lang-de"
          >
            Deutsch
          </button>
        </div>
      </div>

      <!-- Date Format Card -->
      <div class="bg-white shadow rounded-lg p-6">
        <h2 class="text-lg font-semibold text-gray-900" i18n="@@settings.dateFormat">
          Date Format
        </h2>
        <p class="mt-1 text-sm text-gray-500 mb-4" i18n="@@settings.dateFormatDesc">
          Choose how dates are displayed
        </p>
        <div class="flex gap-3">
          <button
            (click)="setDateFormat('DD.MM.YYYY')"
            class="flex-1 px-4 py-2.5 text-sm font-medium rounded-lg border transition-colors"
            [class.border-blue-500]="formatPrefs.dateFormat() === 'DD.MM.YYYY'"
            [class.bg-blue-50]="formatPrefs.dateFormat() === 'DD.MM.YYYY'"
            [class.text-blue-700]="formatPrefs.dateFormat() === 'DD.MM.YYYY'"
            [class.border-gray-300]="formatPrefs.dateFormat() !== 'DD.MM.YYYY'"
            [class.text-gray-700]="formatPrefs.dateFormat() !== 'DD.MM.YYYY'"
            [class.hover:bg-gray-50]="formatPrefs.dateFormat() !== 'DD.MM.YYYY'"
            data-testid="date-format-eu"
          >
            DD.MM.YYYY
          </button>
          <button
            (click)="setDateFormat('MM/DD/YYYY')"
            class="flex-1 px-4 py-2.5 text-sm font-medium rounded-lg border transition-colors"
            [class.border-blue-500]="formatPrefs.dateFormat() === 'MM/DD/YYYY'"
            [class.bg-blue-50]="formatPrefs.dateFormat() === 'MM/DD/YYYY'"
            [class.text-blue-700]="formatPrefs.dateFormat() === 'MM/DD/YYYY'"
            [class.border-gray-300]="formatPrefs.dateFormat() !== 'MM/DD/YYYY'"
            [class.text-gray-700]="formatPrefs.dateFormat() !== 'MM/DD/YYYY'"
            [class.hover:bg-gray-50]="formatPrefs.dateFormat() !== 'MM/DD/YYYY'"
            data-testid="date-format-us"
          >
            MM/DD/YYYY
          </button>
        </div>
      </div>

      <!-- Time Format Card -->
      <div class="bg-white shadow rounded-lg p-6">
        <h2 class="text-lg font-semibold text-gray-900" i18n="@@settings.timeFormat">
          Time Format
        </h2>
        <p class="mt-1 text-sm text-gray-500 mb-4" i18n="@@settings.timeFormatDesc">
          Choose 12-hour or 24-hour time
        </p>
        <div class="flex gap-3">
          <button
            (click)="setTimeFormat('24h')"
            class="flex-1 px-4 py-2.5 text-sm font-medium rounded-lg border transition-colors"
            [class.border-blue-500]="formatPrefs.timeFormat() === '24h'"
            [class.bg-blue-50]="formatPrefs.timeFormat() === '24h'"
            [class.text-blue-700]="formatPrefs.timeFormat() === '24h'"
            [class.border-gray-300]="formatPrefs.timeFormat() !== '24h'"
            [class.text-gray-700]="formatPrefs.timeFormat() !== '24h'"
            [class.hover:bg-gray-50]="formatPrefs.timeFormat() !== '24h'"
            data-testid="time-format-24h"
          >
            {{ time24hLabel }}
          </button>
          <button
            (click)="setTimeFormat('12h')"
            class="flex-1 px-4 py-2.5 text-sm font-medium rounded-lg border transition-colors"
            [class.border-blue-500]="formatPrefs.timeFormat() === '12h'"
            [class.bg-blue-50]="formatPrefs.timeFormat() === '12h'"
            [class.text-blue-700]="formatPrefs.timeFormat() === '12h'"
            [class.border-gray-300]="formatPrefs.timeFormat() !== '12h'"
            [class.text-gray-700]="formatPrefs.timeFormat() !== '12h'"
            [class.hover:bg-gray-50]="formatPrefs.timeFormat() !== '12h'"
            data-testid="time-format-12h"
          >
            {{ time12hLabel }}
          </button>
        </div>
      </div>
    </div>
  `,
})
export class SettingsPageComponent implements OnInit {
  readonly i18n = inject(I18nService);
  readonly formatPrefs = inject(FormatPreferencesService);
  readonly userService = inject(UserService);
  private readonly topBarService = inject(TopBarService);

  readonly time24hLabel = $localize`:@@settings.24h:24-hour (14:00)`;
  readonly time12hLabel = $localize`:@@settings.12h:12-hour (2:00 PM)`;

  ngOnInit(): void {
    this.topBarService.setConfig({ title: $localize`:@@settings.title:Settings` });
  }

  switchLanguage(locale: SupportedLocale): void {
    if (locale !== this.i18n.currentLocale()) {
      this.i18n.switchLocale(locale);
    }
  }

  setDateFormat(format: DateFormatPreference): void {
    this.formatPrefs.setDateFormat(format);
  }

  setTimeFormat(format: TimeFormatPreference): void {
    this.formatPrefs.setTimeFormat(format);
  }
}
