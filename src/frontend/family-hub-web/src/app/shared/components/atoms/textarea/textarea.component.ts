import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CommonModule } from '@angular/common';

/**
 * Textarea component implementing ControlValueAccessor for Angular Reactive Forms.
 * Supports validation error display, character counting, and accessibility features.
 *
 * @example
 * ```html
 * <app-textarea
 *   placeholder="Enter email addresses"
 *   [rows]="3"
 *   [maxLength]="500"
 *   [error]="getEmailError()"
 *   ariaLabel="Email addresses"
 * ></app-textarea>
 * ```
 */
@Component({
  selector: 'app-textarea',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TextareaComponent),
      multi: true
    }
  ],
  template: `
    <div class="relative">
      <textarea
        [value]="value"
        [placeholder]="placeholder"
        [disabled]="disabled"
        [rows]="rows"
        [attr.maxlength]="maxLength"
        [attr.aria-label]="ariaLabel"
        [attr.aria-required]="ariaRequired ? 'true' : null"
        [attr.aria-invalid]="error ? 'true' : 'false'"
        [attr.aria-describedby]="error ? errorId : null"
        (input)="onInputChange($event)"
        (blur)="onBlur()"
        [class]="textareaClasses"
      ></textarea>

      <!-- Character Counter -->
      @if (maxLength) {
        <div class="absolute right-3 bottom-3 text-sm pointer-events-none" [class]="counterClasses">
          {{ value?.length || 0 }}/{{ maxLength }}
        </div>
      }

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
export class TextareaComponent implements ControlValueAccessor {
  /**
   * Placeholder text displayed when textarea is empty.
   */
  @Input() placeholder = '';

  /**
   * Number of visible text rows.
   * Defaults to 3.
   */
  @Input() rows = 3;

  /**
   * Maximum character length. When set, displays character counter.
   */
  @Input() maxLength?: number;

  /**
   * Error message to display below textarea.
   * When set, applies error styling to textarea.
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
   * Internal value of the textarea.
   */
  value = '';

  /**
   * Disabled state of the textarea.
   */
  disabled = false;

  /**
   * Unique ID for error message (for aria-describedby).
   */
  errorId = `textarea-error-${Math.random().toString(36).substr(2, 9)}`;

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
   * Computed CSS classes for the textarea element.
   * Applies different styles based on error and disabled states.
   */
  get textareaClasses(): string {
    const baseClasses = 'w-full px-4 py-2 rounded-md transition-colors outline-none resize-y';
    const focusClasses = 'focus:border-blue-500 focus:ring-2 focus:ring-blue-200';

    let stateClasses: string;
    if (this.error) {
      stateClasses = 'border-2 border-red-500 bg-red-50';
    } else if (this.disabled) {
      stateClasses = 'border border-gray-300 bg-gray-100 opacity-60 cursor-not-allowed';
    } else {
      stateClasses = 'border border-gray-300 bg-white';
    }

    // Add padding-bottom if character counter is shown
    const paddingClass = this.maxLength ? 'pb-8' : '';

    return `${baseClasses} ${focusClasses} ${stateClasses} ${paddingClass}`.trim();
  }

  /**
   * Computed CSS classes for the character counter.
   * Changes color when approaching limit (>90%).
   */
  get counterClasses(): string {
    const length = this.value?.length || 0;
    const percentage = this.maxLength ? (length / this.maxLength) * 100 : 0;

    return percentage > 90 ? 'text-amber-600' : 'text-gray-500';
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
   * Handles input change events.
   * Updates internal value and notifies Angular Forms.
   */
  onInputChange(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    this.value = textarea.value;
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
