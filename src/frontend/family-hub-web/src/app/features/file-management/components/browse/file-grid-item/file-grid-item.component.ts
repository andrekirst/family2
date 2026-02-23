import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../../../../shared/icons/icons';
import { StoredFileDto } from '../../../models/file.models';
import { formatBytes } from '../../../utils/file-size.utils';
import { getFileIcon } from '../../../utils/mime-type.utils';

export interface FileAction {
  fileId: string;
  action: 'rename' | 'move' | 'delete' | 'download';
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
          class="absolute right-0 top-8 w-36 bg-white border border-gray-200 rounded-lg shadow-lg z-10 py-1"
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
  readonly file = input.required<StoredFileDto>();
  readonly selected = input(false);
  readonly clicked = output<string>();
  readonly actionTriggered = output<FileAction>();
  readonly showMenu = signal(false);

  readonly dotsIcon: SafeHtml;

  constructor(private readonly sanitizer: DomSanitizer) {
    this.dotsIcon = sanitizer.bypassSecurityTrustHtml(ICONS.DOTS_VERTICAL);
  }

  fileIcon(): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(getFileIcon(this.file().mimeType));
  }

  formatSize(bytes: number): string {
    return formatBytes(bytes);
  }

  toggleMenu(event: Event): void {
    event.stopPropagation();
    this.showMenu.update((v) => !v);
  }

  emitAction(action: FileAction['action']): void {
    this.showMenu.set(false);
    this.actionTriggered.emit({ fileId: this.file().id, action });
  }
}
