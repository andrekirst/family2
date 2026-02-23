import { Component, inject, input, output, signal, OnInit, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../../../../shared/icons/icons';
import { FileService } from '../../../services/file.service';
import { FileDownloadService } from '../../../services/file-download.service';
import { StoredFileDto } from '../../../models/file.models';
import { formatBytes } from '../../../utils/file-size.utils';
import { getFileIcon } from '../../../utils/mime-type.utils';

@Component({
  selector: 'app-file-context-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    @if (file()) {
      <div class="p-4 space-y-4">
        <!-- File icon / preview -->
        <div class="flex justify-center py-4">
          <div class="text-gray-400" [innerHTML]="largeFileIcon()"></div>
        </div>

        <!-- Name (inline-editable) -->
        @if (isEditing()) {
          <div class="flex items-center gap-2">
            <input
              type="text"
              [(ngModel)]="editName"
              (keydown.enter)="saveRename()"
              (keydown.escape)="cancelRename()"
              class="flex-1 px-2 py-1 border border-blue-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              autofocus
            />
            <button
              (click)="saveRename()"
              class="text-blue-600 text-sm font-medium"
              i18n="@@files.context.save"
            >
              Save
            </button>
          </div>
        } @else {
          <h3
            class="text-base font-semibold text-gray-900 text-center cursor-pointer hover:text-blue-600 transition-colors"
            (click)="startRename()"
            [attr.title]="file()!.name"
          >
            {{ file()!.name }}
          </h3>
        }

        <!-- Details -->
        <div class="space-y-3 text-sm">
          <div class="flex justify-between">
            <span class="text-gray-500" i18n="@@files.context.size">Size</span>
            <span class="text-gray-900 font-medium">{{ formatSize(file()!.size) }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500" i18n="@@files.context.type">Type</span>
            <span class="text-gray-900 font-medium">{{ file()!.mimeType }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500" i18n="@@files.context.created">Created</span>
            <span class="text-gray-900 font-medium">{{ file()!.createdAt | date: 'medium' }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500" i18n="@@files.context.modified">Modified</span>
            <span class="text-gray-900 font-medium">{{ file()!.updatedAt | date: 'medium' }}</span>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex flex-col gap-2 pt-2 border-t border-gray-200">
          <button
            (click)="download()"
            class="flex items-center gap-2 px-3 py-2 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition-colors"
          >
            <span [innerHTML]="downloadIcon"></span>
            <span i18n="@@files.action.download">Download</span>
          </button>
          <button
            (click)="moveRequested.emit(file()!.id)"
            class="flex items-center gap-2 px-3 py-2 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition-colors"
          >
            <span [innerHTML]="folderIcon"></span>
            <span i18n="@@files.action.move">Move</span>
          </button>
          <button
            (click)="confirmDelete()"
            class="flex items-center gap-2 px-3 py-2 text-sm text-red-600 rounded-lg hover:bg-red-50 transition-colors"
          >
            <span [innerHTML]="closeIcon"></span>
            <span i18n="@@files.action.delete">Delete</span>
          </button>
        </div>

        <!-- Delete confirmation -->
        @if (showDeleteConfirm()) {
          <div class="p-3 bg-red-50 border border-red-200 rounded-lg">
            <p class="text-sm text-red-700 mb-3" i18n="@@files.context.deleteConfirm">
              Are you sure you want to delete this file?
            </p>
            <div class="flex gap-2">
              <button
                (click)="showDeleteConfirm.set(false)"
                class="flex-1 px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
                i18n="@@common.cancel"
              >
                Cancel
              </button>
              <button
                (click)="doDelete()"
                class="flex-1 px-3 py-1.5 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700"
                i18n="@@common.confirm"
              >
                Confirm
              </button>
            </div>
          </div>
        }
      </div>
    }
  `,
})
export class FileContextPanelComponent implements OnInit, OnChanges {
  private readonly fileService = inject(FileService);
  private readonly downloadService = inject(FileDownloadService);

  readonly fileId = input.required<string>();
  readonly moveRequested = output<string>();
  readonly deleted = output<void>();
  readonly renamed = output<void>();

  readonly file = signal<StoredFileDto | null>(null);
  readonly isEditing = signal(false);
  readonly showDeleteConfirm = signal(false);
  editName = '';

  readonly downloadIcon: SafeHtml;
  readonly folderIcon: SafeHtml;
  readonly closeIcon: SafeHtml;
  private readonly sanitizer: DomSanitizer;

  constructor(sanitizer: DomSanitizer) {
    this.sanitizer = sanitizer;
    this.downloadIcon = sanitizer.bypassSecurityTrustHtml(ICONS.DOWNLOAD);
    this.folderIcon = sanitizer.bypassSecurityTrustHtml(ICONS.FOLDER);
    this.closeIcon = sanitizer.bypassSecurityTrustHtml(ICONS.CLOSE);
  }

  ngOnInit(): void {
    this.loadFile();
  }

  ngOnChanges(): void {
    this.loadFile();
  }

  largeFileIcon(): SafeHtml {
    const f = this.file();
    if (!f) return '';
    const svg = getFileIcon(f.mimeType).replace('h-5 w-5', 'h-16 w-16');
    return this.sanitizer.bypassSecurityTrustHtml(svg);
  }

  formatSize(bytes: number): string {
    return formatBytes(bytes);
  }

  download(): void {
    const f = this.file();
    if (f) {
      this.downloadService.download(f.storageKey, f.name);
    }
  }

  startRename(): void {
    this.editName = this.file()?.name ?? '';
    this.isEditing.set(true);
  }

  cancelRename(): void {
    this.isEditing.set(false);
  }

  saveRename(): void {
    const name = this.editName.trim();
    const f = this.file();
    if (!name || !f || name === f.name) {
      this.isEditing.set(false);
      return;
    }
    this.fileService.renameFile({ fileId: f.id, newName: name }).subscribe((updated) => {
      if (updated) {
        this.file.set(updated);
        this.renamed.emit();
      }
      this.isEditing.set(false);
    });
  }

  confirmDelete(): void {
    this.showDeleteConfirm.set(true);
  }

  doDelete(): void {
    const f = this.file();
    if (!f) return;
    this.fileService.deleteFile(f.id).subscribe((ok) => {
      if (ok) {
        this.deleted.emit();
      }
      this.showDeleteConfirm.set(false);
    });
  }

  private loadFile(): void {
    this.fileService.getFile(this.fileId()).subscribe((f) => this.file.set(f));
  }
}
