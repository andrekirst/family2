import { Component, ElementRef, inject, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../../../../shared/icons/icons';
import { FavoriteService } from '../../../services/favorite.service';
import { StoredFileDto } from '../../../models/file.models';
import { formatBytes } from '../../../utils/file-size.utils';
import { getFileIcon } from '../../../utils/mime-type.utils';

export interface FileAction {
  fileId: string;
  action: 'rename' | 'move' | 'delete' | 'download';
}

export interface FavoriteToggleEvent {
  fileId: string;
  isFavorited: boolean;
}

@Component({
  selector: 'app-file-grid-item',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="group relative flex flex-col items-center p-4 rounded-lg border border-gray-200 hover:border-blue-300 hover:bg-blue-50/50 cursor-pointer transition-colors"
      [class.border-blue-500]="selected()"
      [class.bg-blue-50]="selected()"
      (click)="clicked.emit(file().id)"
      [attr.data-testid]="'file-' + file().id"
    >
      <!-- Favorite star (top-left) -->
      <button
        (click)="toggleFavorite($event)"
        class="absolute top-2 left-2 p-1 rounded transition-all"
        [class.opacity-100]="isFavorited()"
        [class.opacity-0]="!isFavorited()"
        [class.group-hover:opacity-100]="!isFavorited()"
        [attr.aria-label]="isFavorited() ? 'Remove from favorites' : 'Add to favorites'"
      >
        <span
          [innerHTML]="isFavorited() ? starFilledIcon : starIcon"
          [class.text-yellow-500]="isFavorited()"
          [class.text-gray-300]="!isFavorited()"
          class="hover:text-yellow-500 transition-colors"
        ></span>
      </button>

      <div class="text-gray-400 mb-3" [innerHTML]="fileIcon()"></div>
      <p
        class="text-sm font-medium text-gray-900 truncate w-full text-center"
        [attr.title]="file().name"
      >
        {{ file().name }}
      </p>
      <p class="text-xs text-gray-500 mt-1">{{ formatSize(file().size) }}</p>

      <button
        (click)="toggleMenu($event)"
        class="absolute top-2 right-2 p-1 rounded opacity-0 group-hover:opacity-100 hover:bg-gray-200 transition-all"
        [attr.aria-label]="'Actions for ' + file().name"
      >
        <span [innerHTML]="dotsIcon"></span>
      </button>

      @if (showMenu()) {
        <div
          class="absolute right-0 w-36 bg-white border border-gray-200 rounded-lg shadow-lg z-10 py-1"
          [class.top-8]="!menuOpenUp()"
          [class.bottom-full]="menuOpenUp()"
          [class.mb-1]="menuOpenUp()"
          (click)="$event.stopPropagation()"
        >
          <button
            (click)="emitAction('download')"
            class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-100"
            i18n="@@files.action.download"
          >
            Download
          </button>
          <button
            (click)="emitAction('rename')"
            class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-100"
            i18n="@@files.action.rename"
          >
            Rename
          </button>
          <button
            (click)="emitAction('move')"
            class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-100"
            i18n="@@files.action.move"
          >
            Move
          </button>
          <button
            (click)="emitAction('delete')"
            class="w-full text-left px-3 py-2 text-sm text-red-600 hover:bg-red-50"
            i18n="@@files.action.delete"
          >
            Delete
          </button>
        </div>
      }
    </div>
  `,
})
export class FileGridItemComponent {
  private readonly favoriteService = inject(FavoriteService);

  readonly file = input.required<StoredFileDto>();
  readonly selected = input(false);
  readonly isFavorited = input(false);
  readonly clicked = output<string>();
  readonly actionTriggered = output<FileAction>();
  readonly favoriteToggled = output<FavoriteToggleEvent>();
  readonly showMenu = signal(false);
  readonly menuOpenUp = signal(false);

  readonly dotsIcon: SafeHtml;
  readonly starIcon: SafeHtml;
  readonly starFilledIcon: SafeHtml;

  constructor(
    private readonly sanitizer: DomSanitizer,
    private readonly elRef: ElementRef<HTMLElement>,
  ) {
    this.dotsIcon = sanitizer.bypassSecurityTrustHtml(ICONS.DOTS_VERTICAL);
    this.starIcon = sanitizer.bypassSecurityTrustHtml(ICONS.STAR);
    this.starFilledIcon = sanitizer.bypassSecurityTrustHtml(ICONS.STAR_FILLED);
  }

  fileIcon(): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(getFileIcon(this.file().mimeType));
  }

  formatSize(bytes: number): string {
    return formatBytes(bytes);
  }

  toggleMenu(event: Event): void {
    event.stopPropagation();
    const opening = !this.showMenu();
    if (opening) {
      const rect = this.elRef.nativeElement.getBoundingClientRect();
      const spaceBelow = window.innerHeight - rect.bottom;
      this.menuOpenUp.set(spaceBelow < 200);
    }
    this.showMenu.set(opening);
  }

  toggleFavorite(event: Event): void {
    event.stopPropagation();
    this.favoriteService.toggleFavorite(this.file().id).subscribe((isFavorited) => {
      this.favoriteToggled.emit({ fileId: this.file().id, isFavorited });
    });
  }

  emitAction(action: FileAction['action']): void {
    this.showMenu.set(false);
    this.actionTriggered.emit({ fileId: this.file().id, action });
  }
}
