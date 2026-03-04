import {
  Component,
  inject,
  effect,
  HostListener,
  ElementRef,
  viewChild,
  OnDestroy,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommandPaletteService } from '../../services/command-palette.service';
import { PaletteItem } from '../../models/search.models';

@Component({
  selector: 'app-command-palette',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (palette.isOpen()) {
      <div
        class="fixed inset-0 z-50 flex items-start justify-center pt-[15vh]"
        role="dialog"
        aria-modal="true"
        aria-label="Command palette"
        (click)="onBackdropClick($event)"
        data-testid="command-palette"
      >
        <!-- Backdrop -->
        <div class="absolute inset-0 bg-black/50 backdrop-blur-sm transition-opacity"></div>

        <!-- Modal -->
        <div
          class="relative w-full max-w-xl bg-white rounded-xl shadow-2xl border border-gray-200 overflow-hidden"
          (click)="$event.stopPropagation()"
        >
          <!-- Search input -->
          <div class="flex items-center border-b border-gray-200 px-4">
            <svg
              class="h-5 w-5 text-gray-400 flex-shrink-0"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
              />
            </svg>
            <input
              #searchInput
              type="text"
              class="flex-1 px-3 py-3.5 text-sm text-gray-900 placeholder-gray-400 outline-none bg-transparent"
              placeholder="Search or type a command..."
              [ngModel]="palette.query()"
              (ngModelChange)="onQueryChange($event)"
              data-testid="command-palette-input"
              autocomplete="off"
              spellcheck="false"
            />
            @if (palette.isLoading()) {
              <div class="flex items-center gap-1">
                <div class="h-1.5 w-1.5 rounded-full bg-blue-500 animate-pulse"></div>
                <div
                  class="h-1.5 w-1.5 rounded-full bg-blue-500 animate-pulse"
                  style="animation-delay: 150ms"
                ></div>
                <div
                  class="h-1.5 w-1.5 rounded-full bg-blue-500 animate-pulse"
                  style="animation-delay: 300ms"
                ></div>
              </div>
            }
            <kbd
              class="ml-2 px-1.5 py-0.5 text-xs text-gray-400 bg-gray-100 rounded border border-gray-200"
              >Esc</kbd
            >
          </div>

          <!-- Results list -->
          <div
            class="max-h-80 overflow-y-auto"
            role="listbox"
            [attr.aria-activedescendant]="
              palette.items().length > 0 ? 'palette-item-' + palette.selectedIndex() : null
            "
          >
            @if (palette.error()) {
              <div class="px-4 py-8 text-center text-sm text-red-500">
                {{ palette.error() }}
              </div>
            } @else if (palette.query() && !palette.isLoading() && !palette.hasItems()) {
              <div class="px-4 py-8 text-center text-sm text-gray-500">No results found</div>
            } @else {
              @for (item of palette.items(); track $index) {
                @if ($index === 0 || item.type !== palette.items()[$index - 1].type) {
                  <div class="px-3 pt-3 pb-1">
                    <span class="text-xs font-semibold text-gray-400 uppercase tracking-wider">
                      {{ getSectionLabel(item) }}
                    </span>
                  </div>
                }
                <div
                  [id]="'palette-item-' + $index"
                  role="option"
                  [attr.aria-selected]="$index === palette.selectedIndex()"
                  class="mx-2 px-3 py-2.5 flex items-center gap-3 rounded-lg cursor-pointer transition-colors"
                  [class.bg-blue-50]="$index === palette.selectedIndex()"
                  [class.text-blue-900]="$index === palette.selectedIndex()"
                  [class.hover:bg-gray-50]="$index !== palette.selectedIndex()"
                  (click)="palette.executeItem(item)"
                  (mouseenter)="palette.selectedIndex.set($index)"
                  data-testid="palette-item"
                >
                  <div
                    class="flex-shrink-0 w-8 h-8 rounded-lg flex items-center justify-center"
                    [class.bg-blue-100]="$index === palette.selectedIndex()"
                    [class.text-blue-600]="$index === palette.selectedIndex()"
                    [class.bg-gray-100]="$index !== palette.selectedIndex()"
                    [class.text-gray-500]="$index !== palette.selectedIndex()"
                  >
                    {{ getItemEmoji(item) }}
                  </div>
                  <div class="flex-1 min-w-0">
                    <div class="text-sm font-medium truncate">{{ item.title }}</div>
                    @if (item.description) {
                      <div class="text-xs text-gray-500 truncate">{{ item.description }}</div>
                    }
                  </div>
                  @if (item.module) {
                    <span
                      class="text-xs px-2 py-0.5 rounded-full bg-gray-100 text-gray-500 flex-shrink-0"
                    >
                      {{ item.module }}
                    </span>
                  }
                  @if ($index === palette.selectedIndex()) {
                    <kbd
                      class="text-xs px-1.5 py-0.5 bg-blue-100 text-blue-600 rounded border border-blue-200 flex-shrink-0"
                      >Enter</kbd
                    >
                  }
                </div>
              }
            }
          </div>

          <!-- Footer -->
          @if (palette.hasItems()) {
            <div
              class="border-t border-gray-200 px-4 py-2 flex items-center gap-4 text-xs text-gray-400"
            >
              <span class="flex items-center gap-1">
                <kbd class="px-1 py-0.5 bg-gray-100 rounded border border-gray-200">&uarr;</kbd>
                <kbd class="px-1 py-0.5 bg-gray-100 rounded border border-gray-200">&darr;</kbd>
                navigate
              </span>
              <span class="flex items-center gap-1">
                <kbd class="px-1 py-0.5 bg-gray-100 rounded border border-gray-200">Enter</kbd>
                select
              </span>
              <span class="flex items-center gap-1">
                <kbd class="px-1 py-0.5 bg-gray-100 rounded border border-gray-200">Esc</kbd>
                close
              </span>
            </div>
          }
        </div>
      </div>
    }
  `,
})
export class CommandPaletteComponent implements OnDestroy {
  protected readonly palette = inject(CommandPaletteService);
  private readonly searchInput = viewChild<ElementRef<HTMLInputElement>>('searchInput');
  private searchTimeout: ReturnType<typeof setTimeout> | null = null;
  private previousActiveElement: Element | null = null;

  constructor() {
    // Auto-focus input when opened
    effect(() => {
      if (this.palette.isOpen()) {
        this.previousActiveElement = document.activeElement;
        // Use setTimeout to wait for the DOM to render
        setTimeout(() => this.searchInput()?.nativeElement.focus(), 0);
      } else if (this.previousActiveElement instanceof HTMLElement) {
        this.previousActiveElement.focus();
        this.previousActiveElement = null;
      }
    });
  }

  ngOnDestroy(): void {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
  }

  @HostListener('document:keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    // Ctrl+K / Cmd+K to toggle
    if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
      event.preventDefault();
      this.palette.toggle();
      return;
    }

    if (!this.palette.isOpen()) return;

    switch (event.key) {
      case 'Escape':
        event.preventDefault();
        this.palette.close();
        break;
      case 'ArrowDown':
        event.preventDefault();
        this.palette.moveSelection('down');
        this.scrollSelectedIntoView();
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.palette.moveSelection('up');
        this.scrollSelectedIntoView();
        break;
      case 'Enter':
        event.preventDefault();
        this.palette.executeSelected();
        break;
      case 'Tab':
        // Trap focus within the modal
        event.preventDefault();
        break;
    }
  }

  onQueryChange(value: string): void {
    this.palette.query.set(value);

    // Debounce search (300ms)
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    this.searchTimeout = setTimeout(() => {
      this.palette.performSearch(value);
    }, 300);
  }

  onBackdropClick(event: Event): void {
    if (event.target === event.currentTarget) {
      this.palette.close();
    }
  }

  getSectionLabel(item: PaletteItem): string {
    switch (item.type) {
      case 'nlp':
        return 'Suggestions';
      case 'result':
        return 'Search Results';
      case 'command':
        return 'Commands';
      default:
        return '';
    }
  }

  getItemEmoji(item: PaletteItem): string {
    switch (item.type) {
      case 'nlp':
        return '\u2728';
      case 'command':
        return '\u26A1';
      default:
        return this.getModuleEmoji(item.module);
    }
  }

  private getModuleEmoji(module?: string): string {
    switch (module) {
      case 'family':
        return '\uD83D\uDC64';
      case 'calendar':
        return '\uD83D\uDCC5';
      default:
        return '\uD83D\uDD0D';
    }
  }

  private scrollSelectedIntoView(): void {
    const index = this.palette.selectedIndex();
    const element = document.getElementById(`palette-item-${index}`);
    element?.scrollIntoView({ block: 'nearest' });
  }
}
