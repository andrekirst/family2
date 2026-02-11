import { Component, EventEmitter, HostListener, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50"
      (click)="onDismiss()"
      role="dialog"
      aria-modal="true"
      [attr.aria-labelledby]="'confirmation-dialog-title'"
      data-testid="confirmation-dialog-overlay"
    >
      <div
        class="bg-white rounded-2xl max-w-md w-[90%] shadow-xl p-8"
        (click)="$event.stopPropagation()"
        data-testid="confirmation-dialog"
      >
        <!-- Icon + Text -->
        <div class="flex items-start gap-4">
          <div
            [ngClass]="iconContainerClasses"
            class="w-12 h-12 rounded-xl flex items-center justify-center shrink-0"
            data-testid="confirmation-dialog-icon"
          >
            @switch (icon) {
              @case ('trash') {
                <svg
                  class="w-6 h-6"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="1.5"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0"
                  />
                </svg>
              }
              @case ('warning') {
                <svg
                  class="w-6 h-6"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="1.5"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"
                  />
                </svg>
              }
              @default {
                <svg
                  class="w-6 h-6"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="1.5"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z"
                  />
                </svg>
              }
            }
          </div>
          <div>
            <h2
              id="confirmation-dialog-title"
              class="text-base font-semibold text-gray-900"
              data-testid="confirmation-dialog-title"
            >
              {{ title }}
            </h2>
            <p class="text-sm text-gray-500 mt-1" data-testid="confirmation-dialog-message">
              {{ message }}
            </p>
          </div>
        </div>

        <!-- Buttons -->
        <div class="flex items-center justify-end gap-3 mt-6">
          <button
            type="button"
            (click)="onDismiss()"
            [disabled]="isLoading"
            class="font-semibold text-sm text-gray-900 hover:text-gray-600 px-4 py-2.5 disabled:opacity-50"
            data-testid="confirmation-dialog-cancel"
          >
            {{ cancelLabel }}
          </button>
          <button
            type="button"
            (click)="onConfirm()"
            [disabled]="isLoading"
            [ngClass]="confirmButtonClasses"
            class="rounded-lg px-5 py-2.5 text-sm font-semibold text-white disabled:opacity-50"
            data-testid="confirmation-dialog-confirm"
          >
            {{ isLoading ? 'Processing...' : confirmLabel }}
          </button>
        </div>
      </div>
    </div>
  `,
})
export class ConfirmationDialogComponent {
  @Input() title = 'Confirm';
  @Input() message = 'Are you sure?';
  @Input() confirmLabel = 'Confirm';
  @Input() cancelLabel = 'Cancel';
  @Input() variant: 'danger' | 'warning' | 'info' = 'info';
  @Input() icon: 'trash' | 'warning' | 'info' = 'info';
  @Input() isLoading = false;

  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (!this.isLoading) {
      this.onDismiss();
    }
  }

  get confirmButtonClasses(): Record<string, boolean> {
    return {
      'bg-red-500 hover:bg-red-600': this.variant === 'danger',
      'bg-amber-500 hover:bg-amber-600': this.variant === 'warning',
      'bg-blue-500 hover:bg-blue-600': this.variant === 'info',
    };
  }

  get iconContainerClasses(): Record<string, boolean> {
    return {
      'bg-red-50 text-red-500': this.variant === 'danger',
      'bg-amber-50 text-amber-500': this.variant === 'warning',
      'bg-blue-50 text-blue-500': this.variant === 'info',
    };
  }

  onConfirm(): void {
    this.confirmed.emit();
  }

  onDismiss(): void {
    if (!this.isLoading) {
      this.cancelled.emit();
    }
  }
}
