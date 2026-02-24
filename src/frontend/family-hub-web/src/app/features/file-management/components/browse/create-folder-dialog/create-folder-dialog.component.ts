import { Component, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-create-folder-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50"
      (click)="cancelled.emit()"
    >
      <div
        class="bg-white rounded-xl shadow-xl w-full max-w-sm mx-4 p-6"
        (click)="$event.stopPropagation()"
      >
        <h2 class="text-lg font-semibold text-gray-900 mb-4" i18n="@@files.createFolder.title">
          New Folder
        </h2>
        <input
          type="text"
          [(ngModel)]="folderName"
          (keydown.enter)="submit()"
          class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          [placeholder]="folderNamePlaceholder"
          autofocus
          data-testid="folder-name-input"
        />
        <div class="flex justify-end gap-3 mt-4">
          <button
            (click)="cancelled.emit()"
            class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            i18n="@@common.cancel"
          >
            Cancel
          </button>
          <button
            (click)="submit()"
            [disabled]="!folderName.trim()"
            class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors"
            i18n="@@files.createFolder.create"
            data-testid="create-folder-submit"
          >
            Create
          </button>
        </div>
      </div>
    </div>
  `,
})
export class CreateFolderDialogComponent {
  readonly created = output<string>();
  readonly cancelled = output<void>();

  folderName = '';
  readonly folderNamePlaceholder = $localize`:@@files.createFolder.placeholder:Folder name`;

  submit(): void {
    const name = this.folderName.trim();
    if (name) {
      this.created.emit(name);
    }
  }
}
