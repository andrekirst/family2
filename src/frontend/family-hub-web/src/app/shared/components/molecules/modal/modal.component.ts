import {
  Component,
  Input,
  Output,
  EventEmitter,
  AfterViewInit,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../../atoms/icon/icon.component';

/**
 * Modal dialog component with overlay and accessibility features.
 * Supports backdrop dismiss, keyboard navigation (Escape key), and focus management.
 *
 * @example
 * ```html
 * <app-modal
 *   [isOpen]="showModal"
 *   title="Create Family"
 *   [closeable]="false"
 *   (closeModal)="onModalClose()"
 * >
 *   <form>
 *     <!-- Modal content here -->
 *   </form>
 * </app-modal>
 * ```
 */
@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    @if (isOpen) {
      <div
        class="modal-overlay fixed inset-0 z-50 flex items-center justify-center transition-opacity duration-200"
      >
        <!-- Backdrop -->
        <div
          class="modal-backdrop absolute inset-0 bg-black bg-opacity-50"
          role="button"
          tabindex="0"
          (click)="onBackdropClick()"
          (keydown.enter)="onBackdropClick()"
          (keydown.space)="onBackdropClick()"
        ></div>

        <!-- Modal Container -->
        <div
          #modalDialog
          role="dialog"
          aria-modal="true"
          [attr.aria-labelledby]="titleId"
          tabindex="0"
          (keydown)="onKeydown($event)"
          class="modal-container relative bg-white rounded-lg shadow-xl max-w-md w-full mx-4 transition-transform duration-200 focus:outline-none"
          (click)="onContainerClick($event)"
        >
          <!-- Header -->
          <div class="modal-header flex items-center justify-between p-6 border-b border-gray-200">
            <h2 [id]="titleId" class="modal-title text-xl font-semibold text-gray-900">
              {{ title }}
            </h2>

            <!-- Close Button (only if closeable) -->
            @if (closeable) {
              <button
                type="button"
                class="modal-close-button text-gray-400 hover:text-gray-600 transition-colors"
                (click)="close()"
                aria-label="Close modal"
              >
                <app-icon name="x-mark" size="md"></app-icon>
              </button>
            }
          </div>

          <!-- Body -->
          <div class="modal-body p-6">
            <ng-content></ng-content>
          </div>
        </div>
      </div>
    }
  `,
  styles: [],
})
export class ModalComponent implements AfterViewInit {
  /**
   * Controls whether the modal is visible.
   */
  @Input() isOpen = false;

  /**
   * Modal title displayed in the header.
   */
  @Input() title = '';

  /**
   * Whether the modal can be closed by clicking backdrop, close button, or Escape key.
   * Set to false for blocking modals (e.g., required actions).
   * Defaults to true.
   */
  @Input() closeable = true;

  /**
   * Event emitted when the modal requests to be closed.
   * Parent component should set isOpen to false.
   */
  @Output() closeModal = new EventEmitter<void>();

  /**
   * Reference to the modal dialog element for focus management.
   */
  @ViewChild('modalDialog') modalDialog?: ElementRef<HTMLDivElement>;

  /**
   * Unique ID for the title element (for aria-labelledby).
   */
  titleId = `modal-title-${Math.random().toString(36).substr(2, 9)}`;

  /**
   * After view initialization, focus the modal dialog.
   * This ensures keyboard navigation starts from the modal.
   */
  ngAfterViewInit(): void {
    if (this.isOpen && this.modalDialog) {
      setTimeout(() => {
        this.modalDialog?.nativeElement.focus();
      }, 0);
    }
  }

  /**
   * Handles keyboard events on the modal.
   * Closes modal on Escape key if closeable is true.
   */
  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape' && this.closeable) {
      event.preventDefault();
      event.stopPropagation();
      this.close();
    }
  }

  /**
   * Handles backdrop click events.
   * Closes modal if closeable is true.
   */
  onBackdropClick(): void {
    if (this.closeable) {
      this.close();
    }
  }

  /**
   * Handles container click events.
   * Prevents event from bubbling to backdrop (which would close the modal).
   */
  onContainerClick(event: Event): void {
    event.stopPropagation();
  }

  /**
   * Emits the closeModal event.
   * Parent component should handle this by setting isOpen to false.
   */
  close(): void {
    this.closeModal.emit();
  }
}
