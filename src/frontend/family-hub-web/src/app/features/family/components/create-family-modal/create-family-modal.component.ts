import { Component, Input, Output, EventEmitter, inject, computed } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ModalComponent } from '../../../../shared/components/molecules/modal/modal.component';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';
import { FamilyService } from '../../services/family.service';

/**
 * Modal component for creating a new family.
 * Implements Reactive Forms with validation and integrates with FamilyService.
 *
 * @example
 * ```html
 * <app-create-family-modal
 *   [isOpen]="!familyService.hasFamily()"
 *   (onSuccess)="onFamilyCreated()"
 * ></app-create-family-modal>
 * ```
 */
@Component({
  selector: 'app-create-family-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ModalComponent,
    InputComponent,
    IconComponent
  ],
  template: `
    <app-modal
      [isOpen]="isOpen"
      title="Create Your Family"
      [closeable]="false"
    >
      <form [formGroup]="familyForm" (ngSubmit)="onSubmit()">
        <div class="space-y-4">
          <!-- Icon + Description -->
          <div class="flex items-center space-x-3">
            <app-icon name="users" size="lg" customClass="text-blue-600"></app-icon>
            <p class="text-gray-600">Give your family a name to get started</p>
          </div>

          <!-- Family Name Input -->
          <app-input
            type="text"
            formControlName="name"
            placeholder="Enter family name"
            [maxLength]="50"
            [error]="getNameError()"
            ariaLabel="Family name"
            [ariaRequired]="true"
          ></app-input>

          <!-- API Error Display -->
          <div
            *ngIf="familyService.error() as apiError"
            role="alert"
            class="text-sm text-red-600 bg-red-50 p-3 rounded-md"
            aria-live="polite"
          >
            <p>{{ apiError }}</p>
          </div>

          <!-- Actions -->
          <div class="flex justify-end">
            <button
              type="submit"
              [disabled]="familyForm.invalid || isSubmitting()"
              class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
            >
              <span *ngIf="!isSubmitting()">Create Family</span>
              <span *ngIf="isSubmitting()" class="flex items-center">
                <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Creating...
              </span>
            </button>
          </div>
        </div>
      </form>
    </app-modal>
  `,
  styles: []
})
export class CreateFamilyModalComponent {
  /**
   * Controls whether the modal is visible.
   */
  @Input() isOpen = false;

  /**
   * Event emitted when family is successfully created.
   * Parent should handle this by allowing navigation to authenticated homepage.
   */
  @Output() onSuccess = new EventEmitter<void>();

  /**
   * Injected FamilyService for state management and API calls.
   */
  familyService = inject(FamilyService);

  /**
   * Reactive form for family creation.
   */
  familyForm = new FormGroup({
    name: new FormControl('', [
      Validators.required,
      Validators.maxLength(50)
    ])
  });

  /**
   * Computed signal indicating submission in progress.
   * Derived from familyService.isLoading().
   */
  isSubmitting = computed(() => this.familyService.isLoading());

  /**
   * Gets validation error message for name field.
   * Returns undefined if field is untouched or valid.
   *
   * @returns Error message or undefined
   */
  getNameError(): string | undefined {
    const control = this.familyForm.controls.name;
    if (!control.touched) return undefined;

    if (control.hasError('required')) {
      return 'Family name is required';
    }
    if (control.hasError('maxlength')) {
      return 'Family name must be 50 characters or less';
    }
    return undefined;
  }

  /**
   * Handles form submission.
   * Calls FamilyService.createFamily() and emits onSuccess if successful.
   */
  async onSubmit(): Promise<void> {
    if (this.familyForm.invalid) return;

    const name = this.familyForm.value.name!;
    await this.familyService.createFamily(name);

    // Check if creation was successful (no error)
    if (!this.familyService.error()) {
      this.onSuccess.emit();
      this.familyForm.reset();
    }
  }
}
