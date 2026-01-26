import { Component, inject, Output, EventEmitter, OnInit, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { ProfileService } from '../../services/profile.service';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import {
  LANGUAGE_OPTIONS,
  TIMEZONE_OPTIONS,
  DATE_FORMAT_OPTIONS,
} from '../../models/profile.models';

/**
 * Preferences tab for language, timezone, and date format settings.
 *
 * Fields:
 * - Language (dropdown: en, de, es, fr)
 * - Timezone (dropdown: UTC, Europe/Berlin, etc.)
 * - Date format (dropdown: yyyy-MM-dd, etc.)
 */
@Component({
  selector: 'app-preferences-tab',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonComponent],
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()" class="space-y-6">
      <p class="text-gray-600 mb-6">
        Customize how Family Hub displays information for you. These settings affect only your view.
      </p>

      <!-- Language -->
      <div>
        <label for="language" class="block text-sm font-medium text-gray-700 mb-2">Language</label>
        <select
          id="language"
          formControlName="language"
          class="w-full px-4 py-2 border border-gray-300 rounded-md focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-colors bg-white"
          aria-label="Language preference"
        >
          @for (option of languageOptions; track option.value) {
            <option [value]="option.value">{{ option.label }}</option>
          }
        </select>
        <p class="mt-1 text-sm text-gray-500">Choose your preferred language for the interface.</p>
      </div>

      <!-- Timezone -->
      <div>
        <label for="timezone" class="block text-sm font-medium text-gray-700 mb-2">Timezone</label>
        <select
          id="timezone"
          formControlName="timezone"
          class="w-full px-4 py-2 border border-gray-300 rounded-md focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-colors bg-white"
          aria-label="Timezone preference"
        >
          @for (option of timezoneOptions; track option.value) {
            <option [value]="option.value">{{ option.label }}</option>
          }
        </select>
        <p class="mt-1 text-sm text-gray-500">Used for displaying times and scheduling events.</p>
      </div>

      <!-- Date Format -->
      <div>
        <label for="dateFormat" class="block text-sm font-medium text-gray-700 mb-2">
          Date Format
        </label>
        <select
          id="dateFormat"
          formControlName="dateFormat"
          class="w-full px-4 py-2 border border-gray-300 rounded-md focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-colors bg-white"
          aria-label="Date format preference"
        >
          @for (option of dateFormatOptions; track option.value) {
            <option [value]="option.value">{{ option.label }}</option>
          }
        </select>
        <p class="mt-1 text-sm text-gray-500">
          Preview: <span class="font-medium">{{ dateExample() }}</span>
        </p>
      </div>

      <!-- Submit Button -->
      <div class="flex justify-end pt-4 border-t border-gray-200">
        <app-button
          type="submit"
          variant="primary"
          [disabled]="!form.dirty"
          [loading]="profileService.isLoading()"
        >
          Save Preferences
        </app-button>
      </div>
    </form>
  `,
})
export class PreferencesTabComponent implements OnInit {
  readonly profileService = inject(ProfileService);

  @Output() saveSuccess = new EventEmitter<string>();

  languageOptions = LANGUAGE_OPTIONS;
  timezoneOptions = TIMEZONE_OPTIONS;
  dateFormatOptions = DATE_FORMAT_OPTIONS;

  form = new FormGroup({
    language: new FormControl('en', { nonNullable: true }),
    timezone: new FormControl('UTC', { nonNullable: true }),
    dateFormat: new FormControl('yyyy-MM-dd', { nonNullable: true }),
  });

  /**
   * Computed signal that shows a date example based on selected format.
   */
  dateExample = computed(() => {
    const format = this.form.controls.dateFormat.value;
    const date = new Date();
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();

    switch (format) {
      case 'dd/MM/yyyy':
        return `${day}/${month}/${year}`;
      case 'MM/dd/yyyy':
        return `${month}/${day}/${year}`;
      case 'dd.MM.yyyy':
        return `${day}.${month}.${year}`;
      default:
        return `${year}-${month}-${day}`;
    }
  });

  constructor() {
    // React to profile changes and update form
    effect(() => {
      const profile = this.profileService.profile();
      if (profile?.preferences && !this.form.dirty) {
        this.form.patchValue(
          {
            language: profile.preferences.language,
            timezone: profile.preferences.timezone,
            dateFormat: profile.preferences.dateFormat,
          },
          { emitEvent: false }
        );
      }
    });
  }

  ngOnInit(): void {
    const profile = this.profileService.profile();
    if (profile?.preferences) {
      this.form.patchValue({
        language: profile.preferences.language,
        timezone: profile.preferences.timezone,
        dateFormat: profile.preferences.dateFormat,
      });
    }
  }

  /**
   * Handles form submission.
   * Updates preferences and emits success event.
   */
  async onSubmit(): Promise<void> {
    const values = this.form.getRawValue();
    const profile = this.profileService.profile();

    if (!profile) return;

    const success = await this.profileService.updateProfile({
      displayName: profile.displayName,
      preferences: values,
    });

    if (success) {
      this.form.markAsPristine();
      this.saveSuccess.emit('Preferences updated successfully.');
    }
  }
}
