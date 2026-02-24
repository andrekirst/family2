import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../../../shared/icons/icons';
import { AlbumService } from '../../services/album.service';
import { MediaService } from '../../services/media.service';
import { AlbumDto } from '../../models/album.models';
import { CreateAlbumDialogComponent } from './create-album-dialog.component';

@Component({
  selector: 'app-albums-page',
  standalone: true,
  imports: [CommonModule, CreateAlbumDialogComponent],
  template: `
    <div class="h-full flex flex-col">
      <!-- Toolbar -->
      <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200 bg-white">
        <h2 class="text-lg font-semibold text-gray-900" i18n="@@files.albums.title">Albums</h2>
        <button
          (click)="showCreateDialog.set(true)"
          class="flex items-center gap-2 px-3 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors"
        >
          <span [innerHTML]="plusIcon"></span>
          <span i18n="@@files.albums.create">New Album</span>
        </button>
      </div>

      <!-- Albums grid -->
      <div class="flex-1 overflow-y-auto p-6">
        @if (loading()) {
          <div class="flex items-center justify-center py-16">
            <div
              class="animate-spin rounded-full h-8 w-8 border-2 border-blue-600 border-t-transparent"
            ></div>
          </div>
        } @else if (albums().length === 0) {
          <div class="flex flex-col items-center justify-center py-16 text-gray-400">
            <span [innerHTML]="photoIconLg" class="mb-3"></span>
            <p class="text-lg font-medium" i18n="@@files.albums.empty">No albums yet</p>
            <p class="text-sm mt-1" i18n="@@files.albums.emptyDesc">
              Create an album to organize your photos.
            </p>
            <button
              (click)="showCreateDialog.set(true)"
              class="mt-4 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors"
              i18n="@@files.albums.create"
            >
              New Album
            </button>
          </div>
        } @else {
          <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
            @for (album of albums(); track album.id) {
              <div
                class="group cursor-pointer rounded-xl overflow-hidden border border-gray-200 hover:border-blue-300 hover:shadow-md transition-all"
                (click)="openAlbum(album.id)"
              >
                <!-- Cover image -->
                <div class="aspect-square bg-gray-100 flex items-center justify-center relative">
                  @if (album.coverFileId) {
                    <img
                      [src]="getCoverUrl(album.coverFileId)"
                      [alt]="album.name"
                      class="w-full h-full object-cover"
                    />
                  } @else {
                    <span [innerHTML]="photoIcon" class="text-gray-300"></span>
                  }
                  <!-- Overlay on hover -->
                  <div
                    class="absolute inset-0 bg-black/0 group-hover:bg-black/10 transition-colors"
                  ></div>
                </div>
                <!-- Info -->
                <div class="p-3">
                  <p class="text-sm font-medium text-gray-900 truncate" [attr.title]="album.name">
                    {{ album.name }}
                  </p>
                  <p class="text-xs text-gray-500 mt-0.5">
                    {{ album.itemCount }}
                    <span i18n="@@files.albums.photos">photos</span>
                  </p>
                </div>
              </div>
            }
          </div>
        }
      </div>
    </div>

    @if (showCreateDialog()) {
      <app-create-album-dialog
        (created)="onAlbumCreated($event)"
        (cancelled)="showCreateDialog.set(false)"
      />
    }
  `,
})
export class AlbumsPageComponent implements OnInit {
  private readonly albumService = inject(AlbumService);
  private readonly mediaService = inject(MediaService);
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);

  readonly albums = signal<AlbumDto[]>([]);
  readonly loading = signal(true);
  readonly showCreateDialog = signal(false);

  readonly plusIcon: SafeHtml;
  readonly photoIcon: SafeHtml;
  readonly photoIconLg: SafeHtml;

  constructor() {
    this.plusIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.PLUS);
    this.photoIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.PHOTO);
    const lgPhoto = ICONS.PHOTO.replace('h-5 w-5', 'h-12 w-12');
    this.photoIconLg = this.sanitizer.bypassSecurityTrustHtml(lgPhoto);
  }

  ngOnInit(): void {
    this.loadAlbums();
  }

  openAlbum(albumId: string): void {
    this.router.navigate(['/files/albums', albumId]);
  }

  getCoverUrl(coverFileId: string): string {
    return this.mediaService.getThumbnailUrl(coverFileId);
  }

  onAlbumCreated(albumId: string): void {
    this.showCreateDialog.set(false);
    this.loadAlbums();
  }

  private loadAlbums(): void {
    this.albumService.getAlbums().subscribe((albums) => {
      this.albums.set(albums);
      this.loading.set(false);
    });
  }
}
