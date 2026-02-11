import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TopBarService, TopBarAction } from '../../services/top-bar.service';
import { AuthService } from '../../../core/auth/auth.service';
import { UserService } from '../../../core/user/user.service';
import { ICONS } from '../../icons/icons';

@Component({
  selector: 'app-top-bar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <header
      class="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6 flex-shrink-0"
      data-testid="top-bar"
    >
      <!-- Left: Page title -->
      <h1 class="text-xl font-semibold text-gray-900 truncate">
        {{ topBarService.title() }}
      </h1>

      <!-- Right: Actions + Logout -->
      <div class="flex items-center gap-3">
        @for (action of topBarService.actions(); track action.id) {
          <button
            (click)="action.onClick()"
            [disabled]="action.disabled ?? false"
            class="px-4 py-2 text-sm font-medium rounded-md transition-colors disabled:opacity-50"
            [class.bg-blue-600]="action.variant === 'primary'"
            [class.text-white]="action.variant === 'primary'"
            [class.hover:bg-blue-700]="action.variant === 'primary'"
            [class.bg-white]="action.variant === 'secondary' || !action.variant"
            [class.text-gray-700]="action.variant === 'secondary' || !action.variant"
            [class.border]="action.variant === 'secondary' || !action.variant"
            [class.border-gray-300]="action.variant === 'secondary' || !action.variant"
            [class.hover:bg-gray-50]="action.variant === 'secondary' || !action.variant"
            [class.bg-red-600]="action.variant === 'danger'"
            [class.text-white]="action.variant === 'danger'"
            [class.hover:bg-red-700]="action.variant === 'danger'"
            [attr.data-testid]="action.testId ?? action.id"
          >
            {{ action.label }}
          </button>
        }

        <button
          (click)="logout()"
          class="flex items-center gap-2 px-3 py-2 text-sm text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-md transition-colors"
          data-testid="logout-button"
        >
          <span [innerHTML]="icons.LOGOUT"></span>
          <span>Logout</span>
        </button>
      </div>
    </header>
  `,
})
export class TopBarComponent {
  readonly topBarService = inject(TopBarService);
  private readonly authService = inject(AuthService);
  private readonly userService = inject(UserService);

  readonly icons = ICONS;

  logout(): void {
    this.userService.clearUser();
    this.authService.logout();
  }
}
