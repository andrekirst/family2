import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer } from '@angular/platform-browser';
import { ICONS } from '../../../../../shared/icons/icons';
import { FolderDto } from '../../../models/folder.models';

export interface FolderAction {
  folderId: string;
  action: 'rename' | 'move' | 'delete';
}

@Component({
  selector: 'app-folder-item',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="group relative flex items-center gap-3 p-3 rounded-lg border border-gray-200 hover:border-blue-300 hover:bg-blue-50/50 cursor-pointer transition-colors"
      (click)="clicked.emit(folder().id)"
      (keydown.enter)="clicked.emit(folder().id)"
      tabindex="0"
      role="button"
      [attr.data-testid]="'folder-' + folder().id"
    >
      <span [innerHTML]="folderIcon" class="text-blue-500 flex-shrink-0"></span>
      <span class="text-sm font-medium text-gray-900 truncate flex-1">{{ folder().name }}</span>
      <button
        (click)="toggleMenu($event)"
        class="p-1 rounded opacity-0 group-hover:opacity-100 hover:bg-gray-200 transition-all"
        [attr.aria-label]="'Actions for ' + folder().name"
      >
        <span [innerHTML]="dotsIcon"></span>
      </button>

      @if (showMenu()) {
        <div
          class="absolute right-0 top-full mt-1 w-36 bg-white border border-gray-200 rounded-lg shadow-lg z-10 py-1"
          (click)="$event.stopPropagation()"
        >
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
export class FolderItemComponent {
  readonly folder = input.required<FolderDto>();
  readonly clicked = output<string>();
  readonly actionTriggered = output<FolderAction>();
  readonly showMenu = signal(false);

  readonly folderIcon;
  readonly dotsIcon;

  constructor(sanitizer: DomSanitizer) {
    this.folderIcon = sanitizer.bypassSecurityTrustHtml(ICONS.FOLDER);
    this.dotsIcon = sanitizer.bypassSecurityTrustHtml(ICONS.DOTS_VERTICAL);
  }

  toggleMenu(event: Event): void {
    event.stopPropagation();
    this.showMenu.update((v) => !v);
  }

  emitAction(action: FolderAction['action']): void {
    this.showMenu.set(false);
    this.actionTriggered.emit({ folderId: this.folder().id, action });
  }
}
