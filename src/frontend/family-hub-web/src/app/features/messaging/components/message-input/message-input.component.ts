import { Component, ElementRef, ViewChild, inject, output, signal, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MessagingService } from '../../services/messaging.service';

export interface PendingAttachment {
  storageKey: string;
  fileName: string;
  mimeType: string;
  fileSize: number;
  checksum: string;
}

export interface MessageSendPayload {
  content: string;
  attachments: PendingAttachment[];
}

@Component({
  selector: 'app-message-input',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, CommonModule],
  styles: [
    `
      :host {
        display: block;
        flex-shrink: 0;
      }
    `,
  ],
  template: `
    <div
      class="flex-shrink-0 border-t border-gray-200 bg-white px-3 py-[13px]"
      [class.ring-2]="isDragging()"
      [class.ring-blue-400]="isDragging()"
      [class.bg-blue-50]="isDragging()"
      (dragover)="onDragOver($event)"
      (dragleave)="onDragLeave($event)"
      (drop)="onDrop($event)"
      data-testid="message-input"
    >
      <!-- Pending attachment chips -->
      @if (pendingAttachments().length > 0) {
        <div class="flex flex-wrap gap-1.5 pb-2">
          @for (attachment of pendingAttachments(); track attachment.storageKey) {
            <span
              class="inline-flex items-center gap-1 rounded-full bg-gray-100 px-2.5 py-1 text-xs text-gray-700"
              data-testid="pending-attachment"
            >
              <span class="max-w-[120px] truncate">{{ attachment.fileName }}</span>
              <span class="text-gray-400">({{ formatFileSize(attachment.fileSize) }})</span>
              <button
                (click)="removeAttachment(attachment.storageKey)"
                class="ml-0.5 text-gray-400 hover:text-gray-600"
                data-testid="remove-attachment"
              >
                &times;
              </button>
            </span>
          }
        </div>
      }

      @if (isUploading()) {
        <div class="pb-2 text-xs text-gray-400" i18n="@@messaging.uploading">Uploading...</div>
      }

      <div class="flex items-end gap-2">
        <!-- Paperclip attachment button -->
        <button
          (click)="fileInput.click()"
          class="flex-shrink-0 rounded-lg p-2.5 text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
          title="Attach file"
          i18n-title="@@messaging.attachFile"
          data-testid="attach-button"
        >
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
              d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13"
            />
          </svg>
        </button>

        <input
          #fileInput
          type="file"
          multiple
          class="hidden"
          (change)="onFilesSelected($event)"
          data-testid="file-input"
        />

        <textarea
          class="flex-1 resize-none rounded-lg border border-gray-300 px-3 py-2.5 text-sm text-gray-900 placeholder-gray-400 focus:border-blue-500 focus:ring-1 focus:ring-blue-500 outline-none transition-colors"
          [rows]="1"
          [maxLength]="4000"
          placeholder="Type a message..."
          i18n-placeholder="@@messaging.inputPlaceholder"
          [(ngModel)]="content"
          (input)="updateCanSend()"
          (keydown)="onKeyDown($event)"
          (paste)="onPaste($event)"
          data-testid="message-textarea"
        ></textarea>
        <button
          (click)="send()"
          [disabled]="!canSend()"
          class="flex-shrink-0 rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          data-testid="send-button"
        >
          <span i18n="@@messaging.send">Send</span>
        </button>
      </div>
    </div>
  `,
})
export class MessageInputComponent {
  private readonly messagingService = inject(MessagingService);

  readonly messageSend = output<MessageSendPayload>();

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  content = '';
  canSend = signal(false);
  pendingAttachments = signal<PendingAttachment[]>([]);
  isDragging = signal(false);
  isUploading = signal(false);

  updateCanSend(): void {
    this.canSend.set(this.content.trim().length > 0 || this.pendingAttachments().length > 0);
  }

  onKeyDown(event: KeyboardEvent): void {
    this.updateCanSend();
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  send(): void {
    const trimmed = this.content.trim();
    const attachments = this.pendingAttachments();
    if (!trimmed && attachments.length === 0) return;

    this.messageSend.emit({
      content: trimmed,
      attachments,
    });
    this.content = '';
    this.pendingAttachments.set([]);
    this.canSend.set(false);
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.uploadFiles(Array.from(input.files));
      input.value = '';
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
    if (event.dataTransfer?.files) {
      this.uploadFiles(Array.from(event.dataTransfer.files));
    }
  }

  onPaste(event: ClipboardEvent): void {
    const items = event.clipboardData?.items;
    if (!items) return;

    const files: File[] = [];
    for (let i = 0; i < items.length; i++) {
      if (items[i].kind === 'file') {
        const file = items[i].getAsFile();
        if (file) files.push(file);
      }
    }
    if (files.length > 0) {
      event.preventDefault();
      this.uploadFiles(files);
    }
  }

  removeAttachment(storageKey: string): void {
    this.pendingAttachments.update((list) => list.filter((a) => a.storageKey !== storageKey));
    this.updateCanSend();
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  uploadFiles(files: File[]): void {
    this.isUploading.set(true);
    let completed = 0;

    for (const file of files) {
      this.messagingService.uploadFile(file).subscribe({
        next: (result) => {
          this.pendingAttachments.update((list) => [
            ...list,
            {
              storageKey: result.storageKey,
              fileName: result.fileName,
              mimeType: result.mimeType,
              fileSize: result.size,
              checksum: result.checksum,
            },
          ]);
          this.updateCanSend();
        },
        error: (err) => console.error('Upload failed:', err),
        complete: () => {
          completed++;
          if (completed === files.length) {
            this.isUploading.set(false);
          }
        },
      });
    }
  }
}
