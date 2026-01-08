import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';

/**
 * Data interface for family name step.
 * Represents the data collected and validated in this step.
 */
export interface FamilyNameStepData {
  name: string;
}

/**
 * Wizard step component for collecting family name.
 * Implements the WizardStepComponent contract for integration with WizardService.
 *
 * **Purpose:** First step of family creation wizard where users enter a unique family name.
 *
 * **Features:**
 * - Reactive form validation (required, max length 50)
 * - Real-time character counter with color-coding
 * - Accessibility-compliant error messages
 * - Data persistence across wizard navigation
 * - Integration with WizardService via data input/output
 *
 * **Validation Rules:**
 * - Required: Family name cannot be empty
 * - Max Length: 50 characters
 * - Shows errors only after field is touched
 *
 * **Character Counter Color Coding:**
 * - Gray: 0-40 characters (normal)
 * - Amber: 41-50 characters (warning - approaching limit)
 * - Red: 51+ characters (error - exceeds limit, blocked by maxlength)
 *
 * @example
 * ```typescript
 * // Used within WizardComponent:
 * {
 *   id: 'family-name',
 *   componentType: FamilyNameStepComponent,
 *   title: 'Family Name',
 *   validateOnNext: (data) => {
 *     const stepData = data.get('family-name') as FamilyNameStepData;
 *     if (!stepData?.name) return ['Family name is required'];
 *     if (stepData.name.length > 50) return ['Family name too long'];
 *     return null;
 *   }
 * }
 * ```
 */
@Component({
  selector: 'app-family-name-step',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, InputComponent, IconComponent],
  template: `
    <div class="space-y-6">
      <!-- Header Section -->
      <div class="flex flex-col items-center text-center space-y-4">
        <div class="p-4 bg-blue-50 rounded-full">
          <app-icon name="users" size="lg" customClass="text-blue-600"></app-icon>
        </div>
        <div>
          <h2 class="text-xl font-semibold text-gray-900 mb-2">Name Your Family</h2>
          <p class="text-gray-600">
            Give your family a name to get started. You can invite members later.
          </p>
        </div>
      </div>

      <!-- Form Section -->
      <form [formGroup]="familyForm" (ngSubmit)="onSubmit()" class="space-y-4">
        <div>
          <label for="family-name-input" class="block text-sm font-medium text-gray-700 mb-2">
            Family Name <span class="text-red-600" aria-label="required">*</span>
          </label>

          <app-input
            id="family-name-input"
            type="text"
            formControlName="name"
            placeholder="e.g., Smith Family"
            [maxLength]="50"
            [error]="getNameError()"
            ariaLabel="Family name"
            [ariaRequired]="true"
          ></app-input>

          <!-- Helper Text -->
          @if (!getNameError()) {
            <p class="text-sm text-gray-500 mt-2">
              Choose a name that represents your family. This will be visible to all family members.
            </p>
          }
        </div>
        <!-- Hidden submit button for Enter key handling -->
        <button type="submit" class="sr-only" aria-hidden="true" tabindex="-1"></button>
      </form>
    </div>
  `,
  styles: [],
})
export class FamilyNameStepComponent implements OnInit {
  /**
   * Input data from WizardService.
   * Used to restore state when navigating back to this step.
   */
  @Input() data?: FamilyNameStepData;

  /**
   * Output emitter for data changes.
   * WizardService subscribes to this to persist step data.
   */
  @Output() dataChange = new EventEmitter<FamilyNameStepData>();

  /**
   * Reactive form for family name input.
   * Validators:
   * - required: Family name must not be empty
   * - maxLength(50): Family name cannot exceed 50 characters
   *
   * Using nonNullable option ensures type-safe string value (never null/undefined)
   */
  familyForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(50)],
    }),
  });

  /**
   * Initializes form with data from WizardService if available.
   * Sets up reactive effect to emit data changes.
   */
  ngOnInit(): void {
    // Initialize form with existing data if available (restore state)
    if (this.data?.name) {
      this.familyForm.patchValue({ name: this.data.name });
    }

    // Emit initial value immediately
    const initialValue = this.familyForm.getRawValue();
    this.dataChange.emit({ name: initialValue.name });

    // Emit on value changes (reactive form updates)
    this.familyForm.valueChanges.subscribe(() => {
      const formValue = this.familyForm.getRawValue();
      this.dataChange.emit({ name: formValue.name });
    });
  }

  /**
   * Gets validation error message for name field.
   * Returns undefined if field is untouched or valid.
   *
   * Error messages:
   * - "Family name is required" - When field is empty
   * - "Family name must be 50 characters or less" - When exceeds max length
   *
   * @returns Error message string or undefined
   */
  getNameError(): string | undefined {
    const control = this.familyForm.controls.name;

    // Don't show errors until field is touched
    if (!control.touched) {
      return undefined;
    }

    if (control.hasError('required')) {
      return 'Family name is required';
    }

    if (control.hasError('maxlength')) {
      return 'Family name must be 50 characters or less';
    }

    return undefined;
  }

  /**
   * Handles form submission (Enter key press).
   * Triggers the wizard's Next/Submit button programmatically.
   */
  onSubmit(): void {
    // Mark field as touched to trigger validation
    this.familyForm.controls.name.markAsTouched();

    // Find and click the wizard's Next/Submit button in the footer
    // The button is the second button in the footer (first is Back, second is Next/Submit)
    const buttons = document.querySelectorAll<HTMLButtonElement>(
      'app-wizard footer app-button button'
    );

    if (buttons.length >= 2) {
      // Click the second button (Next/Submit)
      buttons[1].click();
    }
  }
}
