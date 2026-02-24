import { Component, ElementRef, inject, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../../../../shared/icons/icons';
import { FavoriteService } from '../../../services/favorite.service';
import { StoredFileDto } from '../../../models/file.models';
import { formatBytes } from '../../../utils/file-size.utils';
import { getFileIcon } from '../../../utils/mime-type.utils';
import { FileAction, FavoriteToggleEvent } from '../file-grid-item/file-grid-item.component';

@Component({
  selector: 'app-file-list-item',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="group relative flex items-center gap-4 px-4 py-3 hover:bg-gray-50 cursor-pointer border-b border-gray-100 transition-colors"
      [class.bg-blue-50]="selected()"
      (click)="clicked.emit(file().id)"
      [attr.data-testid]="'file-list-' + file().id"
    >
      <!-- Favorite star -->
      <button
        (click)="toggleFavorite($event)"
        class="flex-shrink-0 p-0.5 rounded transition-all"
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

      <span [innerHTML]="fileIcon()" class="text-gray-400 flex-shrink-0"></span>
      <span class="text-sm text-gray-900 truncate flex-1">{{ file().name }}</span>
      <span class="text-xs text-gray-500 w-20 text-right flex-shrink-0">{{
        formatSize(file().size)
      }}</span>
      <span class="text-xs text-gray-500 w-32 text-right flex-shrink-0">{{
        file().updatedAt | date: 'mediumDate'
      }}</span>
      <button
        (click)="toggleMenu($event)"
        class="p-1 rounded opacity-0 group-hover:opacity-100 hover:bg-gray-200 transition-all flex-shrink-0"
        [attr.aria-label]="'Actions for ' + file().name"
      >
        <span [innerHTML]="dotsIcon"></span>
      </button>

      @if (showMenu()) {
        <div
          class="absolute right-4 w-36 bg-white border border-gray-200 rounded-lg shadow-lg z-10 py-1"
          [class.top-full]="!menuOpenUp()"
          [class.mt-1]="!menuOpenUp()"
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
export class FileListItemComponent {
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
