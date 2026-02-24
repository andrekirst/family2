import { Component, inject, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer } from '@angular/platform-browser';
import { ICONS } from '../../../../../shared/icons/icons';
import { FileUploadService, UploadProgress } from '../../../services/file-upload.service';
import { FileService } from '../../../services/file.service';
import { FileStateService } from '../../../services/file-state.service';

interface FileUploadEntry {
  file: File;
  progress: number;
  status: 'pending' | 'uploading' | 'registering' | 'done' | 'error';
  error?: string;
}

@Component({
  selector: 'app-upload-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50" (click)="close()">
      <div
        class="bg-white rounded-xl shadow-xl w-full max-w-lg mx-4 p-6"
        (click)="$event.stopPropagation()"
      >
        <h2 class="text-lg font-semibold text-gray-900 mb-4" i18n="@@files.upload.title">
          Upload Files
        </h2>

        <!-- Drop zone -->
        <div
          class="border-2 border-dashed rounded-lg p-8 text-center transition-colors"
          [class.border-blue-400]="isDragging()"
          [class.bg-blue-50]="isDragging()"
          [class.border-gray-300]="!isDragging()"
          (dragover)="onDragOver($event)"
          (dragleave)="isDragging.set(false)"
          (drop)="onDrop($event)"
        >
          <div class="text-gray-400 mb-2" [innerHTML]="uploadIcon"></div>
          <p class="text-sm text-gray-600 mb-2" i18n="@@files.upload.dropHere">
            Drag & drop files here
          </p>
          <p class="text-xs text-gray-400 mb-3" i18n="@@files.upload.or">or</p>
          <label
            class="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 cursor-pointer transition-colors"
          >
            <span i18n="@@files.upload.browse">Browse Files</span>
            <input
              type="file"
              multiple
              class="hidden"
              (change)="onFileSelected($event)"
              data-testid="file-input"
            />
          </label>
        </div>

        <!-- Upload list -->
        @if (uploads().length > 0) {
          <div class="mt-4 max-h-48 overflow-y-auto space-y-2">
            @for (entry of uploads(); track entry.file.name) {
              <div class="flex items-center gap-3">
                <span class="text-sm text-gray-700 truncate flex-1">{{ entry.file.name }}</span>
                @if (entry.status === 'uploading' || entry.status === 'registering') {
                  <div class="w-24 h-2 bg-gray-200 rounded-full overflow-hidden">
                    <div
                      class="h-full bg-blue-500 rounded-full transition-all"
                      [style.width.%]="entry.progress"
                    ></div>
                  </div>
                }
                @if (entry.status === 'done') {
                  <span class="text-green-600 text-xs font-medium" i18n="@@files.upload.done"
                    >Done</span
                  >
                }
                @if (entry.status === 'error') {
                  <span class="text-red-600 text-xs font-medium">{{ entry.error }}</span>
                }
              </div>
            }
          </div>
        }

        <!-- Footer -->
        <div class="flex justify-end gap-3 mt-4">
          <button
            (click)="close()"
            class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            i18n="@@common.close"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  `,
})
export class UploadDialogComponent {
  private readonly uploadService = inject(FileUploadService);
  private readonly fileService = inject(FileService);
  private readonly fileState = inject(FileStateService);

  readonly closed = output<void>();
  readonly fileUploaded = output<void>();
  readonly uploads = signal<FileUploadEntry[]>([]);
  readonly isDragging = signal(false);

  readonly uploadIcon;

  constructor(sanitizer: DomSanitizer) {
    this.uploadIcon = sanitizer.bypassSecurityTrustHtml(
      ICONS.UPLOAD.replace('h-5 w-5', 'h-10 w-10'),
    );
  }

  close(): void {
    this.closed.emit();
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(true);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
    const files = event.dataTransfer?.files;
    if (files) {
      this.addFiles(Array.from(files));
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.addFiles(Array.from(input.files));
    }
  }

  private addFiles(files: File[]): void {
    const entries: FileUploadEntry[] = files.map((file) => ({
      file,
      progress: 0,
      status: 'pending' as const,
    }));
    this.uploads.update((current) => [...current, ...entries]);
    entries.forEach((entry) => this.processUpload(entry));
  }

  private processUpload(entry: FileUploadEntry): void {
    const folderId = this.fileState.currentFolderId();
    entry.status = 'uploading';
    this.updateEntry(entry);

    this.uploadService.upload(entry.file, folderId ?? '').subscribe({
      next: (progress: UploadProgress) => {
        entry.progress = progress.percent;
        if (progress.done && progress.result) {
          entry.status = 'registering';
          this.updateEntry(entry);

          // Register file metadata via GraphQL
          this.fileService
            .uploadFile({
              name: entry.file.name,
              mimeType: progress.result.mimeType,
              size: progress.result.size,
              storageKey: progress.result.storageKey,
              checksum: progress.result.checksum,
              folderId: folderId ?? '',
            })
            .subscribe({
              next: () => {
                entry.status = 'done';
                entry.progress = 100;
                this.updateEntry(entry);
                this.fileUploaded.emit();
              },
              error: () => {
                entry.status = 'error';
                entry.error = 'Failed to register';
                this.updateEntry(entry);
              },
            });
        } else {
          this.updateEntry(entry);
        }
      },
      error: () => {
        entry.status = 'error';
        entry.error = 'Upload failed';
        this.updateEntry(entry);
      },
    });
  }

  private updateEntry(entry: FileUploadEntry): void {
    this.uploads.update((list) => [...list]);
  }
}
