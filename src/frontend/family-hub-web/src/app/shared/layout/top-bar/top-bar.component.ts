import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TopBarService } from '../../services/top-bar.service';
import { CommandPaletteService } from '../../services/command-palette.service';

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

      <!-- Center: Search trigger -->
      <button
        (click)="commandPalette.open()"
        class="flex items-center gap-2 px-3 py-1.5 text-sm text-gray-500 bg-gray-50 border border-gray-200 rounded-lg hover:bg-gray-100 hover:border-gray-300 transition-colors cursor-pointer"
        data-testid="search-trigger"
      >
        <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
          />
        </svg>
        <span class="hidden sm:inline">Search...</span>
        <kbd
          class="hidden sm:inline-flex ml-1 px-1.5 py-0.5 text-xs text-gray-400 bg-white rounded border border-gray-200"
          >Ctrl+K</kbd
        >
      </button>

      <!-- Right: Actions -->
      <div class="flex items-center gap-3">
        @for (action of topBarService.actions(); track action.id) {
          <button
            (click)="action.onClick()"
            [disabled]="action.disabled ?? false"
            class="px-4 py-2 text-sm font-medium rounded-md transition-colors disabled:opacity-50"
            [class.bg-blue-600]="action.variant === 'primary'"
            [class.text-white]="action.variant === 'primary' || action.variant === 'danger'"
            [class.hover:bg-blue-700]="action.variant === 'primary'"
            [class.bg-white]="action.variant === 'secondary' || !action.variant"
            [class.text-gray-700]="action.variant === 'secondary' || !action.variant"
            [class.border]="action.variant === 'secondary' || !action.variant"
            [class.border-gray-300]="action.variant === 'secondary' || !action.variant"
            [class.hover:bg-gray-50]="action.variant === 'secondary' || !action.variant"
            [class.bg-red-600]="action.variant === 'danger'"
            [class.hover:bg-red-700]="action.variant === 'danger'"
            [attr.data-testid]="action.testId ?? action.id"
          >
            {{ action.label }}
          </button>
        }
      </div>
    </header>
  `,
})
export class TopBarComponent {
  readonly topBarService = inject(TopBarService);
  protected readonly commandPalette = inject(CommandPaletteService);
}
