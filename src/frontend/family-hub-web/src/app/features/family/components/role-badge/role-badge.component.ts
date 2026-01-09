import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserRole, ROLE_LABELS, ROLE_COLORS } from '../../models/family.models';

/**
 * Reusable badge component for displaying user roles.
 * Provides consistent styling across the application.
 *
 * @example
 * ```html
 * <app-role-badge [role]="'OWNER'" />
 * <app-role-badge [role]="member.role" size="sm" />
 * ```
 */
@Component({
  selector: 'app-role-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './role-badge.component.html',
  styleUrl: './role-badge.component.scss',
})
export class RoleBadgeComponent {
  /**
   * User role to display.
   */
  @Input({ required: true }) role!: UserRole;

  /**
   * Badge size variant.
   * @default 'md'
   */
  @Input() size: 'sm' | 'md' | 'lg' = 'md';

  /**
   * Gets the display label for the role.
   */
  get label(): string {
    return ROLE_LABELS[this.role];
  }

  /**
   * Gets Tailwind CSS classes for the role color.
   */
  get colorClasses(): string {
    return ROLE_COLORS[this.role];
  }

  /**
   * Gets Tailwind CSS classes for the size variant.
   */
  get sizeClasses(): string {
    switch (this.size) {
      case 'sm':
        return 'text-xs px-2 py-0.5';
      case 'lg':
        return 'text-base px-4 py-2';
      case 'md':
      default:
        return 'text-sm px-3 py-1';
    }
  }

  /**
   * Gets combined CSS classes for the badge.
   */
  get badgeClasses(): string {
    return `inline-flex items-center rounded-full font-medium ${this.sizeClasses} ${this.colorClasses}`;
  }
}
