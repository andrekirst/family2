import { Component, inject, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AvatarService, UploadAvatarInput } from './avatar.service';

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_SIZE_BYTES = 5 * 1024 * 1024; // 5 MB

@Component({
  selector: 'app-avatar-upload',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-4">
      @if (!previewUrl()) {
        <!-- File selection area -->
        <div
          class="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center cursor-pointer hover:border-blue-400 transition-colors"
          (click)="fileInput.click()"
          (dragover)="onDragOver($event)"
          (drop)="onDrop($event)"
        >
          <svg
            class="mx-auto h-12 w-12 text-gray-400"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
            />
          </svg>
          <p class="mt-2 text-sm text-gray-600">Click or drag image here</p>
          <p class="text-xs text-gray-400 mt-1">JPEG, PNG, or WebP. Max 5 MB.</p>
        </div>
        <input
          #fileInput
          type="file"
          accept="image/jpeg,image/png,image/webp"
          class="hidden"
          (change)="onFileSelected($event)"
        />
      } @else {
        <!-- Preview with actions -->
        <div class="flex flex-col items-center space-y-3">
          <img
            [src]="previewUrl()"
            alt="Avatar preview"
            class="w-32 h-32 rounded-full object-cover border-2 border-gray-200"
          />
          <div class="flex gap-2">
            <button
              (click)="uploadAvatar()"
              [disabled]="isUploading()"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
            >
              {{ isUploading() ? 'Uploading...' : 'Save Avatar' }}
            </button>
            <button
              (click)="cancelPreview()"
              [disabled]="isUploading()"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200"
            >
              Cancel
            </button>
          </div>
        </div>
      }

      @if (errorMessage()) {
        <p class="text-sm text-red-600" role="alert">{{ errorMessage() }}</p>
      }
    </div>
  `,
})
export class AvatarUploadComponent {
  private avatarService = inject(AvatarService);

  avatarUploaded = output<string>();

  previewUrl = signal<string | null>(null);
  isUploading = signal(false);
  errorMessage = signal<string | null>(null);

  private selectedFile: File | null = null;

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const file = event.dataTransfer?.files[0];
    if (file) this.handleFile(file);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.handleFile(file);
    input.value = ''; // Reset for re-selection
  }

  private handleFile(file: File): void {
    this.errorMessage.set(null);

    if (!ALLOWED_TYPES.includes(file.type)) {
      this.errorMessage.set('Only JPEG, PNG, and WebP images are supported.');
      return;
    }

    if (file.size > MAX_SIZE_BYTES) {
      this.errorMessage.set('Image must not exceed 5 MB.');
      return;
    }

    this.selectedFile = file;

    // Generate preview
    const reader = new FileReader();
    reader.onload = () => {
      this.previewUrl.set(reader.result as string);
    };
    reader.readAsDataURL(file);
  }

  cancelPreview(): void {
    this.previewUrl.set(null);
    this.selectedFile = null;
    this.errorMessage.set(null);
  }

  uploadAvatar(): void {
    if (!this.selectedFile) return;

    this.isUploading.set(true);
    this.errorMessage.set(null);

    const reader = new FileReader();
    reader.onload = () => {
      const base64 = (reader.result as string).split(',')[1]; // Remove data:...;base64, prefix

      const input: UploadAvatarInput = {
        imageBase64: base64,
        fileName: this.selectedFile!.name,
        mimeType: this.selectedFile!.type,
      };

      this.avatarService.uploadAvatar(input).subscribe({
        next: (avatarId) => {
          this.isUploading.set(false);
          if (avatarId) {
            this.previewUrl.set(null);
            this.selectedFile = null;
            this.avatarUploaded.emit(avatarId);
          } else {
            this.errorMessage.set('Failed to upload avatar. Please try again.');
          }
        },
        error: () => {
          this.isUploading.set(false);
          this.errorMessage.set('An error occurred. Please try again.');
        },
      });
    };
    reader.readAsDataURL(this.selectedFile);
  }
}
