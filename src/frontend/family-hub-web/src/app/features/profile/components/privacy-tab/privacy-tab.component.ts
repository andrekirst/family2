import { Component, inject, Output, EventEmitter, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { ProfileService } from '../../services/profile.service';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';
import { VISIBILITY_OPTIONS, FieldVisibility } from '../../models/profile.models';

/**
 * Privacy tab for controlling field visibility settings.
 *
 * Fields:
 * - Birthday visibility (hidden/family/public)
 * - Pronouns visibility (hidden/family/public)
 * - Preferences visibility (hidden/family/public)
 */
@Component({
  selector: 'app-privacy-tab',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonComponent, IconComponent],
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()" class="space-y-6">
      <p class="text-gray-600 mb-6">
        Control who can see your personal information. These settings apply when other family
        members or users view your profile.
      </p>

      <!-- Visibility Legend -->
      <div class="bg-gray-50 rounded-lg p-4 mb-6">
        <h3 class="text-sm font-medium text-gray-700 mb-3">Visibility Levels</h3>
        <div class="space-y-2">
          @for (option of visibilityOptions; track option.value) {
            <div class="flex items-center text-sm">
              <span [class]="getVisibilityBadgeClass(option.value)" class="w-20 text-center">
                {{ option.label }}
              </span>
              <span class="ml-3 text-gray-600">{{ option.description }}</span>
            </div>
          }
        </div>
      </div>

      <!-- Birthday Visibility -->
      <div class="border-b border-gray-200 pb-4">
        <div class="flex flex-col sm:flex-row sm:justify-between sm:items-start gap-3">
          <div class="flex-1">
            <label for="birthdayVisibility" class="block text-sm font-medium text-gray-900">
              Birthday
            </label>
            <p class="text-sm text-gray-500">Who can see your birthday and calculated age</p>
          </div>
          <select
            id="birthdayVisibility"
            formControlName="birthdayVisibility"
            class="w-full sm:w-40 px-3 py-2 border border-gray-300 rounded-md focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-colors bg-white text-sm"
            aria-label="Birthday visibility"
          >
            @for (option of visibilityOptions; track option.value) {
              <option [value]="option.value">{{ option.label }}</option>
            }
          </select>
        </div>
      </div>

      <!-- Pronouns Visibility -->
      <div class="border-b border-gray-200 pb-4">
        <div class="flex flex-col sm:flex-row sm:justify-between sm:items-start gap-3">
          <div class="flex-1">
            <label for="pronounsVisibility" class="block text-sm font-medium text-gray-900">
              Pronouns
            </label>
            <p class="text-sm text-gray-500">Who can see your preferred pronouns</p>
          </div>
          <select
            id="pronounsVisibility"
            formControlName="pronounsVisibility"
            class="w-full sm:w-40 px-3 py-2 border border-gray-300 rounded-md focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-colors bg-white text-sm"
            aria-label="Pronouns visibility"
          >
            @for (option of visibilityOptions; track option.value) {
              <option [value]="option.value">{{ option.label }}</option>
            }
          </select>
        </div>
      </div>

      <!-- Preferences Visibility -->
      <div class="pb-4">
        <div class="flex flex-col sm:flex-row sm:justify-between sm:items-start gap-3">
          <div class="flex-1">
            <label for="preferencesVisibility" class="block text-sm font-medium text-gray-900">
              Preferences
            </label>
            <p class="text-sm text-gray-500">
              Who can see your language, timezone, and date format settings
            </p>
          </div>
          <select
            id="preferencesVisibility"
            formControlName="preferencesVisibility"
            class="w-full sm:w-40 px-3 py-2 border border-gray-300 rounded-md focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-colors bg-white text-sm"
            aria-label="Preferences visibility"
          >
            @for (option of visibilityOptions; track option.value) {
              <option [value]="option.value">{{ option.label }}</option>
            }
          </select>
        </div>
      </div>

      <!-- Info Box -->
      <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div class="flex items-start">
          <app-icon name="info-circle" size="sm" customClass="text-blue-600 mt-0.5 mr-3"></app-icon>
          <div>
            <h4 class="text-sm font-medium text-blue-900">About Privacy Settings</h4>
            <p class="mt-1 text-sm text-blue-700">
              Your display name is always visible to family members. These settings control the
              visibility of additional profile information.
            </p>
          </div>
        </div>
      </div>

      <!-- Submit Button -->
      <div class="flex justify-end pt-4 border-t border-gray-200">
        <app-button
          type="submit"
          variant="primary"
          [disabled]="!form.dirty"
          [loading]="profileService.isLoading()"
        >
          Save Privacy Settings
        </app-button>
      </div>
    </form>
  `,
})
export class PrivacyTabComponent implements OnInit {
  readonly profileService = inject(ProfileService);

  @Output() saveSuccess = new EventEmitter<string>();

  visibilityOptions = VISIBILITY_OPTIONS;

  form = new FormGroup({
    birthdayVisibility: new FormControl<FieldVisibility>('family', { nonNullable: true }),
    pronounsVisibility: new FormControl<FieldVisibility>('family', { nonNullable: true }),
    preferencesVisibility: new FormControl<FieldVisibility>('hidden', { nonNullable: true }),
  });

  constructor() {
    // React to profile changes and update form
    effect(() => {
      const profile = this.profileService.profile();
      if (profile?.fieldVisibility && !this.form.dirty) {
        this.form.patchValue(
          {
            birthdayVisibility: profile.fieldVisibility.birthdayVisibility,
            pronounsVisibility: profile.fieldVisibility.pronounsVisibility,
            preferencesVisibility: profile.fieldVisibility.preferencesVisibility,
          },
          { emitEvent: false }
        );
      }
    });
  }

  ngOnInit(): void {
    const profile = this.profileService.profile();
    if (profile?.fieldVisibility) {
      this.form.patchValue({
        birthdayVisibility: profile.fieldVisibility.birthdayVisibility,
        pronounsVisibility: profile.fieldVisibility.pronounsVisibility,
        preferencesVisibility: profile.fieldVisibility.preferencesVisibility,
      });
    }
  }

  /**
   * Gets CSS classes for visibility badge display.
   */
  getVisibilityBadgeClass(visibility: FieldVisibility): string {
    const base = 'px-2 py-1 text-xs font-medium rounded';
    switch (visibility) {
      case 'hidden':
        return `${base} bg-gray-100 text-gray-700`;
      case 'family':
        return `${base} bg-blue-100 text-blue-700`;
      case 'public':
        return `${base} bg-green-100 text-green-700`;
      default:
        return base;
    }
  }

  /**
   * Handles form submission.
   * Updates visibility settings and emits success event.
   */
  async onSubmit(): Promise<void> {
    const values = this.form.getRawValue();
    const profile = this.profileService.profile();

    if (!profile) return;

    const success = await this.profileService.updateProfile({
      displayName: profile.displayName,
      fieldVisibility: values,
    });

    if (success) {
      this.form.markAsPristine();
      this.saveSuccess.emit('Privacy settings updated successfully.');
    }
  }
}
