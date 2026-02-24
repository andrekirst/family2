import { Component, output } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { ICONS } from '../../../../../shared/icons/icons';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center py-16 text-center">
      <div class="text-gray-300 mb-4" [innerHTML]="folderIcon"></div>
      <h3 class="text-lg font-medium text-gray-900 mb-1" i18n="@@files.empty.title">
        This folder is empty
      </h3>
      <p class="text-sm text-gray-500 mb-6" i18n="@@files.empty.description">
        Upload files or create a subfolder to get started.
      </p>
      <div class="flex gap-3">
        <button
          (click)="uploadClicked.emit()"
          class="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors"
        >
          <span [innerHTML]="uploadIcon"></span>
          <span i18n="@@files.action.upload">Upload</span>
        </button>
        <button
          (click)="createFolderClicked.emit()"
          class="inline-flex items-center gap-2 px-4 py-2 bg-white text-gray-700 text-sm font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
        >
          <span [innerHTML]="plusIcon"></span>
          <span i18n="@@files.action.newFolder">New Folder</span>
        </button>
      </div>
    </div>
  `,
})
export class EmptyStateComponent {
  readonly uploadClicked = output<void>();
  readonly createFolderClicked = output<void>();

  readonly folderIcon;
  readonly uploadIcon;
  readonly plusIcon;

  constructor(sanitizer: DomSanitizer) {
    this.folderIcon = sanitizer.bypassSecurityTrustHtml(
      ICONS.FOLDER.replace('h-5 w-5', 'h-16 w-16'),
    );
    this.uploadIcon = sanitizer.bypassSecurityTrustHtml(ICONS.UPLOAD);
    this.plusIcon = sanitizer.bypassSecurityTrustHtml(ICONS.PLUS);
  }
}
