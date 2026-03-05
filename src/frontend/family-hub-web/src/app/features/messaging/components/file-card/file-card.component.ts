import { Component, computed, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { AttachmentDto } from '../../services/messaging.service';
import { EnvironmentConfigService } from '../../../../core/config/environment-config.service';

@Component({
  selector: 'app-file-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      (click)="download()"
      class="inline-flex items-center gap-2 rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-sm hover:bg-gray-100 transition-colors cursor-pointer max-w-xs text-left"
      data-testid="file-card"
    >
      <!-- File type icon -->
      <div class="flex-shrink-0 text-gray-400">
        @if (isImage()) {
          <svg
            class="h-5 w-5 text-blue-500"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="2"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
            />
          </svg>
        } @else {
          <svg
            class="h-5 w-5"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="2"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z"
            />
          </svg>
        }
      </div>

      <!-- File info -->
      <div class="min-w-0 flex-1">
        <div class="truncate font-medium text-gray-700" data-testid="file-card-name">
          {{ attachment().fileName || 'Attached file' }}
        </div>
        <div class="text-xs text-gray-400">
          {{ formatFileSize(attachment().fileSize) }}
        </div>
      </div>

      <!-- Download icon -->
      <svg
        class="h-4 w-4 flex-shrink-0 text-gray-400"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        stroke-width="2"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
        />
      </svg>
    </button>
  `,
})
export class FileCardComponent {
  private readonly config = inject(EnvironmentConfigService);
  private readonly http = inject(HttpClient);

  attachment = input.required<AttachmentDto>();

  isImage = computed(() => this.attachment().mimeType?.startsWith('image/') ?? false);

  downloadUrl = computed(() => {
    const att = this.attachment();
    const key = att.storageKey ?? att.fileId;
    return `${this.config.apiBaseUrl}/api/files/${key}/download`;
  });

  download(): void {
    this.http.get(this.downloadUrl(), { responseType: 'blob' }).subscribe((blob) => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = this.attachment().fileName || 'download';
      a.click();
      URL.revokeObjectURL(url);
    });
  }

  formatFileSize(bytes: number): string {
    if (!bytes) return '';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
