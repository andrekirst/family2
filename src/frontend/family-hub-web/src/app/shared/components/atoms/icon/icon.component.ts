import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Icon component that renders Heroicons as inline SVG elements.
 * Supports multiple sizes and custom styling through Tailwind CSS classes.
 *
 * @example
 * ```html
 * <app-icon name="users" size="md" customClass="text-blue-600"></app-icon>
 * ```
 */
@Component({
  selector: 'app-icon',
  standalone: true,
  imports: [CommonModule],
  template: `
    <svg
      [attr.class]="svgClasses"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.5"
      aria-hidden="true"
      focusable="false"
    >
      <path
        [attr.d]="iconPath"
        stroke-linecap="round"
        stroke-linejoin="round"
      />
    </svg>
  `,
  styles: []
})
export class IconComponent {
  /**
   * Icon name from Heroicons library.
   * Supported icons: users, x-mark, check, plus, minus, trash, pencil
   */
  @Input() name: string = '';

  /**
   * Icon size variant.
   * - sm: 16px (w-4 h-4)
   * - md: 20px (w-5 h-5) - default
   * - lg: 24px (w-6 h-6)
   */
  @Input() size: 'sm' | 'md' | 'lg' = 'md';

  /**
   * Additional CSS classes to apply to the SVG element.
   * Useful for colors, transformations, etc.
   */
  @Input() customClass: string = '';

  /**
   * Heroicons SVG path data.
   * Limited set of commonly used icons for MVP.
   * Source: https://heroicons.com/ (Outline style)
   */
  private readonly iconPaths: Record<string, string> = {
    // Users icon (group of people)
    'users': 'M15 19.128a9.38 9.38 0 0 0 2.625.372 9.337 9.337 0 0 0 4.121-.952 4.125 4.125 0 0 0-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 0 1 8.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0 1 11.964-3.07M12 6.375a3.375 3.375 0 1 1-6.75 0 3.375 3.375 0 0 1 6.75 0Zm8.25 2.25a2.625 2.625 0 1 1-5.25 0 2.625 2.625 0 0 1 5.25 0Z',

    // X-mark icon (close/cancel)
    'x-mark': 'M6 18 18 6M6 6l12 12',

    // Check icon (success/complete)
    'check': 'M4.5 12.75l6 6 9-13.5',

    // Plus icon (add)
    'plus': 'M12 4.5v15m7.5-7.5h-15',

    // Minus icon (remove)
    'minus': 'M5 12h14',

    // Trash icon (delete)
    'trash': 'm14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0',

    // Pencil icon (edit)
    'pencil': 'm16.862 4.487 1.687-1.688a1.875 1.875 0 1 1 2.652 2.652L10.582 16.07a4.5 4.5 0 0 1-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 0 1 1.13-1.897l8.932-8.931Zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0 1 15.75 21H5.25A2.25 2.25 0 0 1 3 18.75V8.25A2.25 2.25 0 0 1 5.25 6H10',

    // Fallback icon (question mark circle for unknown icons)
    'fallback': 'M9.879 7.519c1.171-1.025 3.071-1.025 4.242 0 1.172 1.025 1.172 2.687 0 3.712-.203.179-.43.326-.67.442-.745.361-1.45.999-1.45 1.827v.75M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9 5.25h.008v.008H12v-.008Z'
  };

  /**
   * Computed property that returns the SVG path for the current icon.
   * Returns fallback icon path if the requested icon is not found.
   */
  get iconPath(): string {
    return this.iconPaths[this.name] || this.iconPaths['fallback'];
  }

  /**
   * Computed property that combines size classes with custom classes.
   * Ensures proper CSS class ordering for Tailwind.
   */
  get svgClasses(): string {
    const sizeClasses = {
      'sm': 'w-4 h-4',
      'md': 'w-5 h-5',
      'lg': 'w-6 h-6'
    };

    const baseClasses = sizeClasses[this.size] || sizeClasses['md'];
    return this.customClass ? `${baseClasses} ${this.customClass}` : baseClasses;
  }
}
