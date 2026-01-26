import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { ProfileService } from '../../services/profile.service';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';

/**
 * Profile setup wizard for first-login users.
 * Collects display name before allowing access to dashboard.
 *
 * Minimal flow: Just display name (required for profile completion).
 *
 * @example
 * Route: /profile/setup
 * Guard: authGuard, noProfileSetupGuard
 */
@Component({
  selector: 'app-profile-setup-wizard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, InputComponent, ButtonComponent, IconComponent],
  template: `
    <div class="min-h-screen bg-gray-50 flex flex-col items-center justify-center p-4">
      <div class="max-w-md w-full bg-white rounded-lg shadow-lg p-8">
        <!-- Header -->
        <div class="text-center mb-8">
          <div
            class="mx-auto w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mb-4"
          >
            <app-icon name="user" size="lg" customClass="text-blue-600"></app-icon>
          </div>
          <h1 class="text-2xl font-bold text-gray-900">Welcome to Family Hub!</h1>
          <p class="mt-2 text-gray-600">Let's set up your profile to get started.</p>
        </div>

        <!-- Error Message -->
        @if (profileService.error()) {
          <div
            class="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-800"
            role="alert"
            aria-live="polite"
          >
            <div class="flex items-center">
              <app-icon name="alert-circle" size="sm" customClass="text-red-600 mr-2"></app-icon>
              <span>{{ profileService.error() }}</span>
            </div>
          </div>
        }

        <!-- Form -->
        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="space-y-6">
          <div>
            <label for="displayName" class="block text-sm font-medium text-gray-700 mb-2">
              What should we call you? <span class="text-red-600" aria-label="required">*</span>
            </label>
            <app-input
              id="displayName"
              type="text"
              formControlName="displayName"
              placeholder="Enter your name or nickname"
              [maxLength]="100"
              [error]="getDisplayNameError()"
              ariaLabel="Display name"
              [ariaRequired]="true"
            ></app-input>
            <p class="mt-2 text-sm text-gray-500">
              This is how you'll appear to family members. You can change it later.
            </p>
          </div>

          <app-button
            type="submit"
            variant="primary"
            size="lg"
            [disabled]="!form.valid"
            [loading]="profileService.isLoading()"
            fullWidth
          >
            Continue to Family Hub
          </app-button>
        </form>

        <!-- Privacy Note -->
        <p class="mt-6 text-center text-xs text-gray-500">
          Your information is kept private and secure. See our privacy policy for details.
        </p>
      </div>
    </div>
  `,
})
export class ProfileSetupWizardComponent {
  readonly profileService = inject(ProfileService);
  private router = inject(Router);

  form = new FormGroup({
    displayName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
  });

  /**
   * Gets validation error message for display name field.
   * Only shows error after field is touched (blur).
   */
  getDisplayNameError(): string | undefined {
    const control = this.form.controls.displayName;
    if (!control.touched) return undefined;

    if (control.hasError('required')) {
      return 'Please enter a name';
    }
    if (control.hasError('maxlength')) {
      return 'Name must be 100 characters or less';
    }
    return undefined;
  }

  /**
   * Handles form submission.
   * Creates profile with display name and redirects to dashboard.
   */
  async onSubmit(): Promise<void> {
    if (!this.form.valid) return;

    // Mark as touched to show any validation errors
    this.form.controls.displayName.markAsTouched();

    const displayName = this.form.controls.displayName.value.trim();
    if (!displayName) {
      return;
    }

    const success = await this.profileService.completeSetup(displayName);

    if (success) {
      // Navigate to dashboard (or family creation if no family)
      // The familyGuard will redirect to /family/create if needed
      this.router.navigate(['/dashboard']);
    }
  }
}
