import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { ToastService, ToastType } from '../../../../core/services/toast.service';
import { IconComponent } from '../../atoms/icon/icon.component';

/**
 * Toast container component that displays global notifications
 *
 * **Purpose:** Renders toast notifications from ToastService with animations
 *
 * **Features:**
 * - Fixed position top-right (z-50)
 * - Slide-in/slide-out animations
 * - Color-coded by type (success/error/warning/info)
 * - Dismissible with close button
 * - ARIA roles for accessibility (role="alert")
 * - Auto-scrollable if many toasts
 *
 * **Usage:**
 * Place once in AppComponent template (global placement):
 * ```html
 * <app-toast-container />
 * ```
 *
 * **Accessibility:**
 * - role="alert" for screen readers
 * - aria-live="assertive" for immediate announcements
 * - aria-atomic="true" for complete message reading
 * - Keyboard accessible close button
 *
 * @example
 * ```html
 * <!-- In app.component.html -->
 * <router-outlet />
 * <app-toast-container />
 * ```
 */
@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule, IconComponent],
  templateUrl: './toast-container.component.html',
  styleUrls: ['./toast-container.component.scss'],
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate('200ms ease-out', style({ transform: 'translateX(0)', opacity: 1 })),
      ]),
      transition(':leave', [
        animate('150ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 })),
      ]),
    ]),
  ],
})
export class ToastContainerComponent {
  /**
   * Inject ToastService to access toast state
   */
  public toastService = inject(ToastService);

  /**
   * Gets Tailwind CSS classes for toast background and border based on type
   *
   * @param type - Toast type (success/error/warning/info)
   * @returns CSS class string
   */
  getToastClasses(type: ToastType): string {
    const baseClasses =
      'flex items-start gap-3 p-4 rounded-lg shadow-lg border-l-4 min-w-[320px] max-w-md';

    const typeClasses = {
      success: 'bg-green-50 border-green-500 text-green-900',
      error: 'bg-red-50 border-red-500 text-red-900',
      warning: 'bg-amber-50 border-amber-500 text-amber-900',
      info: 'bg-blue-50 border-blue-500 text-blue-900',
    };

    return `${baseClasses} ${typeClasses[type]}`;
  }

  /**
   * Gets icon name based on toast type
   *
   * @param type - Toast type (success/error/warning/info)
   * @returns Icon name for IconComponent
   */
  getIconName(type: ToastType): string {
    const iconMap = {
      success: 'check-circle',
      error: 'x-circle',
      warning: 'exclamation-triangle',
      info: 'information-circle',
    };

    return iconMap[type];
  }

  /**
   * Gets icon color classes based on toast type
   *
   * @param type - Toast type (success/error/warning/info)
   * @returns Tailwind color class for icon
   */
  getIconColorClass(type: ToastType): string {
    const colorMap = {
      success: 'text-green-600',
      error: 'text-red-600',
      warning: 'text-amber-600',
      info: 'text-blue-600',
    };

    return colorMap[type];
  }

  /**
   * Dismisses a toast by ID
   *
   * @param id - Toast UUID
   */
  dismiss(id: string): void {
    this.toastService.dismiss(id);
  }
}
