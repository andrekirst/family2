import {
  Component,
  inject,
  input,
  output,
  signal,
  computed,
  HostListener,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EnvironmentConfigService } from '../../../../core/config/environment-config.service';
import { SecureSrcDirective } from '../../../../shared/directives/secure-src.directive';
import { PhotoDto, AdjacentPhotosDto } from '../../models/photos.models';

@Component({
  selector: 'app-photo-viewer',
  standalone: true,
  imports: [CommonModule, FormsModule, SecureSrcDirective],
  template: `
    <div class="fixed inset-0 z-50 bg-black/90 flex flex-col" (click)="onBackdropClick($event)">
      <!-- Top bar -->
      <div class="flex items-center justify-between px-4 py-3 text-white">
        <div class="flex items-center gap-3">
          <button
            class="p-2 hover:bg-white/10 rounded-lg transition-colors"
            (click)="close.emit()"
            title="Close (Esc)"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
          <span class="text-sm opacity-75">{{ photo().fileName }}</span>
        </div>
        <div class="flex items-center gap-2">
          @if (canDelete()) {
            <button
              class="p-2 hover:bg-red-500/20 text-red-400 rounded-lg transition-colors"
              (click)="onDelete()"
              title="Delete photo"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                />
              </svg>
            </button>
          }
        </div>
      </div>

      <!-- Main image area -->
      <div class="flex-1 flex items-center justify-center relative min-h-0">
        <!-- Previous button -->
        @if (adjacentPhotos()?.previous) {
          <button
            class="absolute left-4 z-10 p-3 bg-black/50 hover:bg-black/70 text-white rounded-full transition-colors"
            (click)="navigatePrevious.emit(); $event.stopPropagation()"
            title="Previous (←)"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M15 19l-7-7 7-7"
              />
            </svg>
          </button>
        }

        <!-- Photo -->
        <img
          [appSecureSrc]="imageUrl()"
          [alt]="photo().caption || photo().fileName"
          class="max-h-full max-w-full object-contain select-none"
          (click)="$event.stopPropagation()"
        />

        <!-- Next button -->
        @if (adjacentPhotos()?.next) {
          <button
            class="absolute right-4 z-10 p-3 bg-black/50 hover:bg-black/70 text-white rounded-full transition-colors"
            (click)="navigateNext.emit(); $event.stopPropagation()"
            title="Next (→)"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 5l7 7-7 7"
              />
            </svg>
          </button>
        }
      </div>

      <!-- Bottom caption bar -->
      <div class="px-4 py-3 text-white" (click)="$event.stopPropagation()">
        @if (isEditingCaption()) {
          <div class="flex items-center gap-2 max-w-lg mx-auto">
            <input
              type="text"
              class="flex-1 bg-white/10 border border-white/20 rounded-lg px-3 py-2 text-white placeholder-white/50 focus:outline-none focus:ring-2 focus:ring-blue-500"
              [ngModel]="editCaptionValue()"
              (ngModelChange)="editCaptionValue.set($event)"
              placeholder="Add a caption..."
              maxlength="500"
              (keydown.enter)="saveCaption()"
              (keydown.escape)="cancelCaptionEdit()"
            />
            <button
              class="px-3 py-2 bg-blue-600 hover:bg-blue-700 rounded-lg text-sm transition-colors"
              (click)="saveCaption()"
            >
              Save
            </button>
            <button
              class="px-3 py-2 bg-white/10 hover:bg-white/20 rounded-lg text-sm transition-colors"
              (click)="cancelCaptionEdit()"
            >
              Cancel
            </button>
          </div>
        } @else {
          <div
            class="text-center cursor-pointer hover:bg-white/5 rounded-lg py-2 transition-colors max-w-lg mx-auto"
            (click)="startCaptionEdit()"
          >
            @if (photo().caption) {
              <p class="text-sm">{{ photo().caption }}</p>
            } @else {
              <p class="text-sm text-white/50 italic">Click to add a caption...</p>
            }
          </div>
        }
        <div class="flex items-center justify-center gap-4 mt-2 text-xs text-white/50">
          <span>{{ photo().createdAt | date: 'medium' }}</span>
          <span>{{ formatFileSize(photo().fileSizeBytes) }}</span>
        </div>
      </div>
    </div>
  `,
})
export class PhotoViewerComponent implements OnInit, OnDestroy {
  private readonly envConfig = inject(EnvironmentConfigService);

  photo = input.required<PhotoDto>();
  adjacentPhotos = input<AdjacentPhotosDto | null>(null);
  canDelete = input(false);

  close = output<void>();
  navigatePrevious = output<void>();
  navigateNext = output<void>();
  captionUpdated = output<{ photoId: string; caption: string | null }>();
  deleteRequested = output<string>();

  isEditingCaption = signal(false);
  editCaptionValue = signal('');
  imageUrl = computed(() => `${this.envConfig.apiBaseUrl}${this.photo().storagePath}`);

  ngOnInit(): void {
    document.body.style.overflow = 'hidden';
  }

  ngOnDestroy(): void {
    document.body.style.overflow = '';
  }

  @HostListener('document:keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    if (this.isEditingCaption()) return;

    switch (event.key) {
      case 'Escape':
        this.close.emit();
        break;
      case 'ArrowLeft':
        if (this.adjacentPhotos()?.previous) {
          this.navigatePrevious.emit();
        }
        break;
      case 'ArrowRight':
        if (this.adjacentPhotos()?.next) {
          this.navigateNext.emit();
        }
        break;
    }
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.close.emit();
    }
  }

  startCaptionEdit(): void {
    this.editCaptionValue.set(this.photo().caption ?? '');
    this.isEditingCaption.set(true);
  }

  saveCaption(): void {
    const newCaption = this.editCaptionValue().trim() || null;
    this.isEditingCaption.set(false);
    this.captionUpdated.emit({ photoId: this.photo().id, caption: newCaption });
  }

  cancelCaptionEdit(): void {
    this.isEditingCaption.set(false);
  }

  onDelete(): void {
    this.deleteRequested.emit(this.photo().id);
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
