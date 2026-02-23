import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../../../shared/icons/icons';
import { AlbumService } from '../../services/album.service';
import { FileService } from '../../services/file.service';
import { MediaService } from '../../services/media.service';
import { AlbumDto } from '../../models/album.models';
import { StoredFileDto } from '../../models/file.models';

@Component({
  selector: 'app-album-detail',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="h-full flex flex-col">
      <!-- Header -->
      <div class="flex items-center gap-4 px-6 py-4 border-b border-gray-200 bg-white">
        <button
          (click)="goBack()"
          class="p-2 rounded-lg hover:bg-gray-100 text-gray-500 transition-colors"
        >
          <span [innerHTML]="chevronLeftIcon"></span>
        </button>
        @if (album()) {
          <div class="flex-1 min-w-0">
            <h2 class="text-lg font-semibold text-gray-900 truncate">{{ album()!.name }}</h2>
            @if (album()!.description) {
              <p class="text-sm text-gray-500 truncate">{{ album()!.description }}</p>
            }
          </div>
          <span class="text-sm text-gray-500">
            {{ album()!.itemCount }}
            <span i18n="@@files.albums.photos">photos</span>
          </span>
          <button
            (click)="confirmDelete()"
            class="px-3 py-2 text-sm text-red-600 rounded-lg hover:bg-red-50 transition-colors"
            i18n="@@files.action.delete"
          >
            Delete
          </button>
        }
      </div>

      <!-- Photo grid -->
      <div class="flex-1 overflow-y-auto p-6">
        @if (loading()) {
          <div class="flex items-center justify-center py-16">
            <div
              class="animate-spin rounded-full h-8 w-8 border-2 border-blue-600 border-t-transparent"
            ></div>
          </div>
        } @else if (files().length === 0) {
          <div class="flex flex-col items-center justify-center py-16 text-gray-400">
            <span [innerHTML]="photoIconLg" class="mb-3"></span>
            <p class="text-sm font-medium" i18n="@@files.albums.noPhotos">
              No photos in this album yet
            </p>
            <p class="text-xs mt-1" i18n="@@files.albums.noPhotosDesc">
              Add photos from the file browser using the context menu.
            </p>
          </div>
        } @else {
          <div
            class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-2"
          >
            @for (file of files(); track file.id) {
              <div class="group relative aspect-square rounded-lg overflow-hidden bg-gray-100">
                @if (isImage(file.mimeType)) {
                  <img
                    [src]="getStreamUrl(file.storageKey)"
                    [alt]="file.name"
                    class="w-full h-full object-cover"
                    loading="lazy"
                  />
                } @else {
                  <div class="w-full h-full flex items-center justify-center">
                    <span [innerHTML]="photoIcon" class="text-gray-300"></span>
                  </div>
                }
                <!-- Hover overlay -->
                <div
                  class="absolute inset-0 bg-black/0 group-hover:bg-black/30 transition-colors flex items-end"
                >
                  <div
                    class="w-full p-2 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-between"
                  >
                    <span class="text-xs text-white truncate">{{ file.name }}</span>
                    <button
                      (click)="removeFile(file.id, $event)"
                      class="p-1 rounded bg-black/40 text-white hover:bg-red-600 transition-colors"
                      [attr.aria-label]="'Remove ' + file.name"
                    >
                      <span [innerHTML]="closeIcon"></span>
                    </button>
                  </div>
                </div>
              </div>
            }
          </div>
        }
      </div>

      <!-- Delete album confirmation -->
      @if (showDeleteConfirm()) {
        <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div class="bg-white rounded-xl shadow-xl w-full max-w-sm mx-4 p-6">
            <p class="text-sm text-gray-700 mb-4" i18n="@@files.albums.deleteConfirm">
              Are you sure you want to delete this album? Photos will not be deleted.
            </p>
            <div class="flex justify-end gap-3">
              <button
                (click)="showDeleteConfirm.set(false)"
                class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
                i18n="@@common.cancel"
              >
                Cancel
              </button>
              <button
                (click)="doDelete()"
                class="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700"
                i18n="@@common.confirm"
              >
                Confirm
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
})
export class AlbumDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly albumService = inject(AlbumService);
  private readonly fileService = inject(FileService);
  private readonly mediaService = inject(MediaService);
  private readonly sanitizer = inject(DomSanitizer);

  readonly album = signal<AlbumDto | null>(null);
  readonly files = signal<StoredFileDto[]>([]);
  readonly loading = signal(true);
  readonly showDeleteConfirm = signal(false);

  readonly chevronLeftIcon: SafeHtml;
  readonly photoIcon: SafeHtml;
  readonly photoIconLg: SafeHtml;
  readonly closeIcon: SafeHtml;

  private albumId = '';

  constructor() {
    this.chevronLeftIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.CHEVRON_LEFT);
    this.photoIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.PHOTO);
    const lgPhoto = ICONS.PHOTO.replace('h-5 w-5', 'h-12 w-12');
    this.photoIconLg = this.sanitizer.bypassSecurityTrustHtml(lgPhoto);
    this.closeIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.CLOSE);
  }

  ngOnInit(): void {
    this.albumId = this.route.snapshot.paramMap.get('albumId') ?? '';
    this.loadAlbumData();
  }

  goBack(): void {
    this.router.navigate(['/files/albums']);
  }

  isImage(mimeType: string): boolean {
    return mimeType.startsWith('image/');
  }

  getStreamUrl(storageKey: string): string {
    return this.mediaService.getStreamUrl(storageKey);
  }

  removeFile(fileId: string, event: Event): void {
    event.stopPropagation();
    this.albumService.removeFileFromAlbum(this.albumId, fileId).subscribe((ok) => {
      if (ok) {
        this.files.update((f) => f.filter((file) => file.id !== fileId));
      }
    });
  }

  confirmDelete(): void {
    this.showDeleteConfirm.set(true);
  }

  doDelete(): void {
    this.albumService.deleteAlbum(this.albumId).subscribe((ok) => {
      if (ok) {
        this.goBack();
      }
      this.showDeleteConfirm.set(false);
    });
  }

  private loadAlbumData(): void {
    // Load album metadata — the file list comes from getFiles filtered by album
    // For now, we use the album's getAlbums and filter client-side
    this.albumService.getAlbums().subscribe((albums) => {
      const album = albums.find((a) => a.id === this.albumId);
      if (album) {
        this.album.set(album);
      }
      this.loading.set(false);
    });
    // TODO: Backend should provide getAlbumFiles(albumId) query
    // For now files list stays empty until that's available
    this.files.set([]);
  }
}
