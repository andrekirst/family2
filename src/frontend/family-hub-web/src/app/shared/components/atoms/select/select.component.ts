import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CommonModule } from '@angular/common';

/**
 * Select (dropdown) component implementing ControlValueAccessor for Angular Reactive Forms.
 * Supports validation error display and accessibility features.
 *
 * @example
 * ```html
 * <app-select
 *   [options]="['ADMIN', 'MEMBER']"
 *   placeholder="Select role"
 *   [error]="getRoleError()"
 *   ariaLabel="User role"
 *   [ariaRequired]="true"
 * ></app-select>
 * ```
 */
@Component({
  selector: 'app-select',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectComponent),
      multi: true
    }
  ],
  template: `
    <div class="relative">
      <select
        [value]="value"
        [disabled]="disabled"
        [attr.aria-label]="ariaLabel"
        [attr.aria-required]="ariaRequired ? 'true' : null"
        [attr.aria-invalid]="error ? 'true' : 'false'"
        [attr.aria-describedby]="error ? errorId : null"
        (change)="onSelectChange($event)"
        (blur)="onBlur()"
        [class]="selectClasses"
      >
        @if (placeholder) {
          <option value="" disabled [selected]="!value">{{ placeholder }}</option>
        }
        @for (option of options; track option) {
          <option [value]="option">{{ option }}</option>
        }
      </select>

      <!-- Error Message -->
      @if (error) {
        <p
          [id]="errorId"
          class="error-message text-sm text-red-600 mt-1"
          role="alert"
          aria-live="polite"
        >
          {{ error }}
        </p>
      }
    </div>
  `,
  styles: []
})
export class SelectComponent implements ControlValueAccessor {
  /**
   * Array of options to display in the dropdown.
   */
  @Input() options: string[] = [];

  /**
   * Placeholder text displayed when no value is selected.
   */
  @Input() placeholder = '';

  /**
   * Error message to display below select.
   * When set, applies error styling to select.
   */
  @Input() error?: string;

  /**
   * ARIA label for screen readers.
   * Important for accessibility when no visible label exists.
   */
  @Input() ariaLabel?: string;

  /**
   * ARIA required attribute.
   * Set to true for required fields to inform screen readers.
   */
  @Input() ariaRequired = false;

  /**
   * Internal value of the select.
   */
  value = '';

  /**
   * Disabled state of the select.
   */
  disabled = false;

  /**
   * Unique ID for error message (for aria-describedby).
   */
  errorId = `select-error-${Math.random().toString(36).substr(2, 9)}`;

  /**
   * Callback function registered by Angular Forms.
   */
  // eslint-disable-next-line @typescript-eslint/no-empty-function
  private onChange: (value: string) => void = () => {};

  /**
   * Callback function registered by Angular Forms for touched state.
   */
  // eslint-disable-next-line @typescript-eslint/no-empty-function
  private onTouched: () => void = () => {};

  /**
   * Computed CSS classes for the select element.
   * Applies different styles based on error and disabled states.
   */
  get selectClasses(): string {
    const baseClasses = 'w-full px-4 py-2 rounded-md transition-colors outline-none appearance-none bg-no-repeat bg-right';
    const focusClasses = 'focus:border-blue-500 focus:ring-2 focus:ring-blue-200';
    const bgImage = 'bg-[url(\'data:image/svg+xml;charset=UTF-8,%3csvg xmlns=%22http://www.w3.org/2000/svg%22 viewBox=%220 0 20 20%22 fill=%22none%22%3e%3cpath d=%22M7 7l3-3 3 3m0 6l-3 3-3-3%22 stroke=%22%239ca3af%22 stroke-width=%221.5%22 stroke-linecap=%22round%22 stroke-linejoin=%22round%22/%3e%3c/svg%3e\')]';
    const bgSize = 'pr-10';

    let stateClasses: string;
    if (this.error) {
      stateClasses = 'border-2 border-red-500 bg-red-50';
    } else if (this.disabled) {
      stateClasses = 'border border-gray-300 bg-gray-100 opacity-60 cursor-not-allowed';
    } else {
      stateClasses = 'border border-gray-300 bg-white';
    }

    return `${baseClasses} ${focusClasses} ${stateClasses} ${bgImage} ${bgSize}`.trim();
  }

  // ControlValueAccessor Implementation

  /**
   * Writes a new value to the element (called by Angular Forms).
   */
  writeValue(value: string | null): void {
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
   * Handles select change events.
   * Updates internal value and notifies Angular Forms.
   */
  onSelectChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.value = select.value;
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
