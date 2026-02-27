import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PhotoGridComponent } from '../photo-grid/photo-grid.component';
import { PhotoViewerComponent } from '../photo-viewer/photo-viewer.component';
import { PhotosService } from '../../services/photos.service';
import {
  PhotoDto,
  AdjacentPhotosDto,
  PhotoViewMode,
  PHOTOS_CONSTANTS,
} from '../../models/photos.models';
import { UserService } from '../../../../core/user/user.service';

@Component({
  selector: 'app-photos-page',
  standalone: true,
  imports: [CommonModule, PhotoGridComponent, PhotoViewerComponent],
  template: `
    <div class="p-4 md:p-6">
      <!-- Photo count -->
      @if (totalCount() > 0) {
        <div class="flex items-center justify-end mb-4">
          <span class="text-sm text-gray-500 dark:text-gray-400">
            {{ totalCount() }} photo{{ totalCount() === 1 ? '' : 's' }}
          </span>
        </div>
      }

      <!-- Grid View -->
      <app-photo-grid
        [photos]="photos()"
        [isLoading]="isLoading()"
        [isLoadingMore]="isLoadingMore()"
        [hasMore]="hasMore()"
        (photoSelected)="onPhotoSelected($event)"
        (loadMore)="onLoadMore()"
      />

      <!-- Image Viewer Overlay -->
      @if (viewMode() === 'viewer' && selectedPhoto()) {
        <app-photo-viewer
          [photo]="selectedPhoto()!"
          [adjacentPhotos]="adjacentPhotos()"
          [canDelete]="true"
          (close)="onViewerClose()"
          (navigatePrevious)="onNavigatePrevious()"
          (navigateNext)="onNavigateNext()"
          (captionUpdated)="onCaptionUpdated($event)"
          (deleteRequested)="onDeleteRequested($event)"
        />
      }
    </div>
  `,
})
export class PhotosPageComponent implements OnInit {
  private photosService = inject(PhotosService);
  private userService = inject(UserService);

  viewMode = signal<PhotoViewMode>('grid');
  photos = signal<PhotoDto[]>([]);
  selectedPhoto = signal<PhotoDto | null>(null);
  adjacentPhotos = signal<AdjacentPhotosDto | null>(null);
  isLoading = signal(true);
  isLoadingMore = signal(false);
  hasMore = signal(false);
  totalCount = signal(0);

  private currentSkip = 0;

  ngOnInit(): void {
    this.loadPhotos();
  }

  private loadPhotos(): void {
    const familyId = this.userService.currentUser()?.familyId;
    if (!familyId) return;

    this.isLoading.set(true);
    this.currentSkip = 0;

    this.photosService.getPhotos(familyId, 0, PHOTOS_CONSTANTS.PAGE_SIZE).subscribe((page) => {
      this.photos.set(page.items);
      this.totalCount.set(page.totalCount);
      this.hasMore.set(page.hasMore);
      this.currentSkip = page.items.length;
      this.isLoading.set(false);
    });
  }

  onLoadMore(): void {
    const familyId = this.userService.currentUser()?.familyId;
    if (!familyId) return;

    this.isLoadingMore.set(true);

    this.photosService
      .getPhotos(familyId, this.currentSkip, PHOTOS_CONSTANTS.PAGE_SIZE)
      .subscribe((page) => {
        this.photos.update((current) => [...current, ...page.items]);
        this.totalCount.set(page.totalCount);
        this.hasMore.set(page.hasMore);
        this.currentSkip += page.items.length;
        this.isLoadingMore.set(false);
      });
  }

  onPhotoSelected(photo: PhotoDto): void {
    this.selectedPhoto.set(photo);
    this.viewMode.set('viewer');
    this.loadAdjacentPhotos(photo);
  }

  onViewerClose(): void {
    this.viewMode.set('grid');
    this.selectedPhoto.set(null);
    this.adjacentPhotos.set(null);
  }

  onNavigatePrevious(): void {
    const prev = this.adjacentPhotos()?.previous;
    if (prev) {
      this.selectedPhoto.set(prev);
      this.loadAdjacentPhotos(prev);
    }
  }

  onNavigateNext(): void {
    const next = this.adjacentPhotos()?.next;
    if (next) {
      this.selectedPhoto.set(next);
      this.loadAdjacentPhotos(next);
    }
  }

  onCaptionUpdated(event: { photoId: string; caption: string | null }): void {
    this.photosService
      .updatePhotoCaption(event.photoId, { caption: event.caption })
      .subscribe((updated) => {
        if (updated) {
          this.photos.update((current) => current.map((p) => (p.id === updated.id ? updated : p)));
          if (this.selectedPhoto()?.id === updated.id) {
            this.selectedPhoto.set(updated);
          }
        }
      });
  }

  onDeleteRequested(photoId: string): void {
    this.photosService.deletePhoto(photoId).subscribe((success) => {
      if (success) {
        this.photos.update((current) => current.filter((p) => p.id !== photoId));
        this.totalCount.update((count) => count - 1);
        this.onViewerClose();
      }
    });
  }

  private loadAdjacentPhotos(photo: PhotoDto): void {
    const familyId = this.userService.currentUser()?.familyId;
    if (!familyId) return;

    this.photosService
      .getAdjacentPhotos(familyId, photo.id, photo.createdAt)
      .subscribe((adjacent) => {
        this.adjacentPhotos.set(adjacent);
      });
  }
}
