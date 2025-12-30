import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CommonModule } from '@angular/common';

/**
 * Input component implementing ControlValueAccessor for Angular Reactive Forms.
 * Supports validation error display, character counting, and accessibility features.
 *
 * @example
 * ```html
 * <app-input
 *   type="text"
 *   placeholder="Enter family name"
 *   [maxLength]="50"
 *   [error]="getNameError()"
 *   ariaLabel="Family name"
 *   [ariaRequired]="true"
 * ></app-input>
 * ```
 */
@Component({
  selector: 'app-input',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true
    }
  ],
  template: `
    <div class="relative">
      <input
        [type]="type"
        [value]="value"
        [placeholder]="placeholder"
        [disabled]="disabled"
        [attr.maxlength]="maxLength"
        [attr.aria-label]="ariaLabel"
        [attr.aria-required]="ariaRequired ? 'true' : null"
        [attr.aria-invalid]="error ? 'true' : 'false'"
        [attr.aria-describedby]="error ? errorId : null"
        (input)="onInputChange($event)"
        (blur)="onBlur()"
        [class]="inputClasses"
      />

      <!-- Character Counter -->
      <div *ngIf="maxLength" [class]="counterClasses">
        {{ value?.length || 0 }}/{{ maxLength }}
      </div>

      <!-- Error Message -->
      <p
        *ngIf="error"
        [id]="errorId"
        class="error-message text-sm text-red-600 mt-1"
        role="alert"
        aria-live="polite"
      >
        {{ error }}
      </p>
    </div>
  `,
  styles: [`
    .character-counter {
      position: absolute;
      right: 12px;
      top: 50%;
      transform: translateY(-50%);
      font-size: 0.875rem;
      pointer-events: none;
    }
  `]
})
export class InputComponent implements ControlValueAccessor {
  /**
   * Input type variant (text, email, password).
   * Defaults to 'text'.
   */
  @Input() type: 'text' | 'email' | 'password' = 'text';

  /**
   * Placeholder text displayed when input is empty.
   */
  @Input() placeholder: string = '';

  /**
   * Maximum character length. When set, displays character counter.
   */
  @Input() maxLength?: number;

  /**
   * Error message to display below input.
   * When set, applies error styling to input.
   */
  @Input() error: string = '';

  /**
   * ARIA label for screen readers.
   * Important for accessibility when no visible label exists.
   */
  @Input() ariaLabel?: string;

  /**
   * ARIA required attribute.
   * Set to true for required fields to inform screen readers.
   */
  @Input() ariaRequired: boolean = false;

  /**
   * Internal value of the input.
   */
  value: string = '';

  /**
   * Disabled state of the input.
   */
  disabled: boolean = false;

  /**
   * Unique ID for error message (for aria-describedby).
   */
  errorId = `input-error-${Math.random().toString(36).substr(2, 9)}`;

  /**
   * Callback function registered by Angular Forms.
   */
  private onChange: (value: string) => void = () => {};

  /**
   * Callback function registered by Angular Forms for touched state.
   */
  private onTouched: () => void = () => {};

  /**
   * Computed CSS classes for the input element.
   * Applies different styles based on error and disabled states.
   */
  get inputClasses(): string {
    const baseClasses = 'w-full px-4 py-2 rounded-md transition-colors outline-none';
    const focusClasses = 'focus:border-blue-500 focus:ring-2 focus:ring-blue-200';

    let stateClasses: string;
    if (this.error) {
      stateClasses = 'border-2 border-red-500 bg-red-50';
    } else if (this.disabled) {
      stateClasses = 'border border-gray-300 bg-gray-100 opacity-60 cursor-not-allowed';
    } else {
      stateClasses = 'border border-gray-300 bg-white';
    }

    // Add padding-right if character counter is shown
    const paddingClass = this.maxLength ? 'pr-20' : '';

    return `${baseClasses} ${focusClasses} ${stateClasses} ${paddingClass}`.trim();
  }

  /**
   * Computed CSS classes for the character counter.
   * Changes color when approaching limit (>90%).
   */
  get counterClasses(): string {
    const baseClasses = 'character-counter';
    const length = this.value?.length || 0;
    const percentage = this.maxLength ? (length / this.maxLength) * 100 : 0;

    const colorClass = percentage > 90 ? 'text-amber-600' : 'text-gray-500';

    return `${baseClasses} ${colorClass}`;
  }

  // ControlValueAccessor Implementation

  /**
   * Writes a new value to the element (called by Angular Forms).
   */
  writeValue(value: any): void {
    this.value = value || '';
  }

  /**
   * Registers a callback function that should be called when the value changes.
   */
  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  /**
   * Registers a callback function that should be called when the control is touched.
   */
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  /**
   * Sets the disabled state of the control.
   */
  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  // Event Handlers

  /**
   * Handles input change events.
   * Updates internal value and notifies Angular Forms.
   */
  onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.value = input.value;
    this.onChange(this.value);
  }

  /**
   * Handles blur events.
   * Notifies Angular Forms that the control has been touched.
   */
  onBlur(): void {
    this.onTouched();
  }
}
