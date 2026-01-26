import { Component, inject, Output, EventEmitter, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { ProfileService } from '../../services/profile.service';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';

/**
 * Personal info tab for editing display name, birthday, and pronouns.
 *
 * Fields:
 * - Display name (required, text, max 100 chars)
 * - Birthday (date picker, shows calculated age)
 * - Pronouns (text, max 50 chars)
 */
@Component({
  selector: 'app-personal-info-tab',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, InputComponent, ButtonComponent],
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()" class="space-y-6">
      <!-- Display Name -->
      <div>
        <label for="displayName" class="block text-sm font-medium text-gray-700 mb-2">
          Display Name <span class="text-red-600" aria-label="required">*</span>
        </label>
        <app-input
          id="displayName"
          type="text"
          formControlName="displayName"
          placeholder="How you want to be called"
          [maxLength]="100"
          [error]="getError('displayName')"
          ariaLabel="Display name"
          [ariaRequired]="true"
        ></app-input>
        <p class="mt-1 text-sm text-gray-500">This is how you'll appear to family members.</p>
      </div>

      <!-- Birthday -->
      <div>
        <label for="birthday" class="block text-sm font-medium text-gray-700 mb-2">Birthday</label>
        <input
          id="birthday"
          type="date"
          formControlName="birthday"
          class="w-full px-4 py-2 border border-gray-300 rounded-md focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-colors"
          aria-label="Birthday"
          [max]="maxBirthdayDate"
        />
        @if (
          profileService.profile()?.age !== null && profileService.profile()?.age !== undefined
        ) {
          <p class="mt-1 text-sm text-gray-500">
            Age: <span class="font-medium">{{ profileService.profile()?.age }}</span> years
          </p>
        }
        <p class="mt-1 text-sm text-gray-500">
          Family members can see your birthday based on your privacy settings.
        </p>
      </div>

      <!-- Pronouns -->
      <div>
        <label for="pronouns" class="block text-sm font-medium text-gray-700 mb-2">Pronouns</label>
        <app-input
          id="pronouns"
          type="text"
          formControlName="pronouns"
          placeholder="e.g., she/her, he/him, they/them"
          [maxLength]="50"
          [error]="getError('pronouns')"
          ariaLabel="Pronouns"
        ></app-input>
        <p class="mt-1 text-sm text-gray-500">
          Let others know how you'd like to be addressed. Max 50 characters.
        </p>
      </div>

      <!-- Submit Button -->
      <div class="flex justify-end pt-4 border-t border-gray-200">
        <app-button
          type="submit"
          variant="primary"
          [disabled]="!form.valid || !form.dirty"
          [loading]="profileService.isLoading()"
        >
          Save Changes
        </app-button>
      </div>
    </form>
  `,
})
export class PersonalInfoTabComponent implements OnInit {
  readonly profileService = inject(ProfileService);

  @Output() saveSuccess = new EventEmitter<string>();

  form = new FormGroup({
    displayName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    birthday: new FormControl<string | null>(null),
    pronouns: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(50)],
    }),
  });

  /**
   * Maximum allowed birthday date (today).
   * Prevents selecting future dates.
   */
  maxBirthdayDate = new Date().toISOString().split('T')[0];

  constructor() {
    // React to profile changes and update form
    effect(() => {
      const profile = this.profileService.profile();
      if (profile && !this.form.dirty) {
        this.form.patchValue(
          {
            displayName: profile.displayName,
            birthday: profile.birthday ?? null,
            pronouns: profile.pronouns ?? '',
          },
          { emitEvent: false }
        );
      }
    });
  }

  ngOnInit(): void {
    // Initialize form with current profile data
    const profile = this.profileService.profile();
    if (profile) {
      this.form.patchValue({
        displayName: profile.displayName,
        birthday: profile.birthday ?? null,
        pronouns: profile.pronouns ?? '',
      });
    }
  }

  /**
   * Gets validation error message for a field.
   * Only shows error after field is touched (blur).
   */
  getError(field: 'displayName' | 'pronouns'): string | undefined {
    const control = this.form.controls[field];
    if (!control.touched) return undefined;

    if (control.hasError('required')) {
      return 'Display name is required';
    }
    if (control.hasError('maxlength')) {
      const max = field === 'displayName' ? 100 : 50;
      return `Maximum ${max} characters allowed`;
    }
    return undefined;
  }

  /**
   * Handles form submission.
   * Updates profile and emits success event.
   */
  async onSubmit(): Promise<void> {
    if (!this.form.valid) return;

    const values = this.form.getRawValue();
    const success = await this.profileService.updateProfile({
      displayName: values.displayName.trim(),
      birthday: values.birthday ?? undefined,
      pronouns: values.pronouns?.trim() || undefined,
    });

    if (success) {
      this.form.markAsPristine();
      this.saveSuccess.emit('Personal information updated successfully.');
    }
  }
}
