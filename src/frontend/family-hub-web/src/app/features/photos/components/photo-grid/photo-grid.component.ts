import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PhotoDto } from '../../models/photos.models';

@Component({
  selector: 'app-photo-grid',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (isLoading()) {
      <div
        class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6 gap-2"
      >
        @for (_ of skeletonItems; track $index) {
          <div class="aspect-square bg-gray-200 dark:bg-gray-700 rounded-lg animate-pulse"></div>
        }
      </div>
    } @else if (photos().length === 0) {
      <div class="flex flex-col items-center justify-center py-20 text-gray-500 dark:text-gray-400">
        <svg
          class="w-16 h-16 mb-4 opacity-50"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="1.5"
            d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
          />
        </svg>
        <p class="text-lg font-medium">No photos yet</p>
        <p class="text-sm mt-1">Upload your first family photo to get started.</p>
      </div>
    } @else {
      <div
        class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6 gap-2"
      >
        @for (photo of photos(); track photo.id) {
          <button
            class="aspect-square relative group overflow-hidden rounded-lg bg-gray-100 dark:bg-gray-800 cursor-pointer focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
            (click)="onPhotoClick(photo)"
          >
            <img
              [src]="photo.storagePath"
              [alt]="photo.caption || photo.fileName"
              class="w-full h-full object-cover transition-transform duration-200 group-hover:scale-105"
              loading="lazy"
            />
            @if (photo.caption) {
              <div
                class="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/60 to-transparent p-2 opacity-0 group-hover:opacity-100 transition-opacity duration-200"
              >
                <p class="text-white text-xs truncate">{{ photo.caption }}</p>
              </div>
            }
          </button>
        }
      </div>

      @if (hasMore()) {
        <div class="flex justify-center mt-6">
          <button
            class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
            [disabled]="isLoadingMore()"
            (click)="loadMore.emit()"
          >
            {{ isLoadingMore() ? 'Loading...' : 'Load More' }}
          </button>
        </div>
      }
    }
  `,
})
export class PhotoGridComponent {
  photos = input.required<PhotoDto[]>();
  isLoading = input(false);
  isLoadingMore = input(false);
  hasMore = input(false);

  photoSelected = output<PhotoDto>();
  loadMore = output<void>();

  readonly skeletonItems = Array.from({ length: 12 });

  onPhotoClick(photo: PhotoDto): void {
    this.photoSelected.emit(photo);
  }
}
