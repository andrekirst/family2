import { Injectable, signal } from '@angular/core';

/**
 * Toast notification type
 */
export type ToastType = 'success' | 'error' | 'warning' | 'info';

/**
 * Toast notification interface
 */
export interface Toast {
  /** Unique identifier for the toast */
  id: string;
  /** Type of toast (determines styling and icon) */
  type: ToastType;
  /** Message to display */
  message: string;
  /** Duration in milliseconds (0 = manual dismiss only) */
  duration: number;
  /** Timestamp when toast was created */
  createdAt: Date;
}

/**
 * Toast notification service
 *
 * **Purpose:** Provides global toast notifications for user feedback
 *
 * **Features:**
 * - Signal-based reactive state
 * - Auto-dismiss with configurable duration
 * - Max 5 toasts visible (FIFO queue)
 * - Type-specific styling (success/error/warning/info)
 * - Manual dismiss support
 *
 * **Usage:**
 * ```typescript
 * private toastService = inject(ToastService);
 *
 * // Success toast (auto-dismiss after 5s)
 * this.toastService.success('Family created successfully!');
 *
 * // Error toast (stays until manually dismissed)
 * this.toastService.error('Failed to create family');
 *
 * // Warning toast with custom duration
 * this.toastService.warning('Session expiring soon', 10000);
 * ```
 *
 * @example
 * ```typescript
 * @Component({
 *   // ...
 * })
 * export class MyComponent {
 *   private toastService = inject(ToastService);
 *
 *   async saveData() {
 *     try {
 *       await this.apiService.save();
 *       this.toastService.success('Data saved successfully');
 *     } catch (error) {
 *       this.toastService.error('Failed to save data');
 *     }
 *   }
 * }
 * ```
 */
@Injectable({
  providedIn: 'root',
})
export class ToastService {
  /**
   * Maximum number of toasts to display simultaneously
   */
  private readonly MAX_TOASTS = 5;

  /**
   * Default duration for auto-dismiss (5 seconds)
   */
  private readonly DEFAULT_DURATION = 5000;

  /**
   * Internal signal for toast state
   * Writable only within this service
   */
  private toastsSignal = signal<Toast[]>([]);

  /**
   * Public read-only signal for toast state
   * Components subscribe to this to display toasts
   */
  public toasts = this.toastsSignal.asReadonly();

  /**
   * Map of active timeout IDs for auto-dismiss
   * Key: toast.id, Value: setTimeout return value
   */
  private timeoutMap = new Map<string, number>();

  /**
   * Shows a success toast (green styling)
   * Auto-dismisses after 5 seconds by default
   *
   * @param message - Success message to display
   * @param duration - Duration in milliseconds (default: 5000)
   */
  success(message: string, duration: number = this.DEFAULT_DURATION): void {
    this.addToast({
      type: 'success',
      message,
      duration,
    });
  }

  /**
   * Shows an error toast (red styling)
   * Stays until manually dismissed by default (duration = 0)
   *
   * @param message - Error message to display
   * @param duration - Duration in milliseconds (default: 0 = manual dismiss)
   */
  error(message: string, duration = 0): void {
    this.addToast({
      type: 'error',
      message,
      duration,
    });
  }

  /**
   * Shows a warning toast (yellow/amber styling)
   * Auto-dismisses after 5 seconds by default
   *
   * @param message - Warning message to display
   * @param duration - Duration in milliseconds (default: 5000)
   */
  warning(message: string, duration: number = this.DEFAULT_DURATION): void {
    this.addToast({
      type: 'warning',
      message,
      duration,
    });
  }

  /**
   * Shows an info toast (blue styling)
   * Auto-dismisses after 5 seconds by default
   *
   * @param message - Info message to display
   * @param duration - Duration in milliseconds (default: 5000)
   */
  info(message: string, duration: number = this.DEFAULT_DURATION): void {
    this.addToast({
      type: 'info',
      message,
      duration,
    });
  }

  /**
   * Manually dismisses a toast by ID
   * Clears any pending auto-dismiss timeout
   *
   * @param id - UUID of toast to dismiss
   */
  dismiss(id: string): void {
    // Clear timeout if exists
    const timeoutId = this.timeoutMap.get(id);
    if (timeoutId !== undefined) {
      clearTimeout(timeoutId);
      this.timeoutMap.delete(id);
    }

    // Remove toast from array
    this.toastsSignal.update((toasts) => toasts.filter((t) => t.id !== id));
  }

  /**
   * Dismisses all active toasts
   * Clears all pending auto-dismiss timeouts
   */
  dismissAll(): void {
    // Clear all timeouts
    this.timeoutMap.forEach((timeoutId) => clearTimeout(timeoutId));
    this.timeoutMap.clear();

    // Clear all toasts
    this.toastsSignal.set([]);
  }

  /**
   * Internal method to add a toast to the queue
   * Enforces MAX_TOASTS limit (FIFO removal)
   * Sets up auto-dismiss timeout if duration > 0
   *
   * @param options - Toast options (type, message, duration)
   */
  private addToast(options: { type: ToastType; message: string; duration: number }): void {
    // Generate unique ID
    const id = this.generateId();

    // Create toast object
    const toast: Toast = {
      id,
      type: options.type,
      message: options.message,
      duration: options.duration,
      createdAt: new Date(),
    };

    // Add to toasts array
    this.toastsSignal.update((toasts) => {
      const newToasts = [...toasts, toast];

      // Enforce MAX_TOASTS limit (FIFO - remove oldest)
      if (newToasts.length > this.MAX_TOASTS) {
        const removedToast = newToasts.shift()!; // Remove first (oldest)

        // Clear timeout for removed toast
        const timeoutId = this.timeoutMap.get(removedToast.id);
        if (timeoutId !== undefined) {
          clearTimeout(timeoutId);
          this.timeoutMap.delete(removedToast.id);
        }
      }

      return newToasts;
    });

    // Set up auto-dismiss if duration > 0
    if (options.duration > 0) {
      const timeoutId = window.setTimeout(() => {
        this.dismiss(id);
      }, options.duration);

      this.timeoutMap.set(id, timeoutId);
    }
  }

  /**
   * Generates a simple UUID v4
   * Used for unique toast identification
   *
   * @returns UUID string
   */
  private generateId(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      const r = (Math.random() * 16) | 0;
      const v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }
}
