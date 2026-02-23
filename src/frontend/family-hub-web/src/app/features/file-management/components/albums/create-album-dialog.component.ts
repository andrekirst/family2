import { Component, inject, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AlbumService } from '../../services/album.service';

@Component({
  selector: 'app-create-album-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50"
      (click)="cancelled.emit()"
    >
      <div
        class="bg-white rounded-xl shadow-xl w-full max-w-sm mx-4 p-6"
        (click)="$event.stopPropagation()"
      >
        <h2 class="text-lg font-semibold text-gray-900 mb-4" i18n="@@files.albums.createTitle">
          Create Album
        </h2>
        <div class="space-y-3">
          <input
            type="text"
            [(ngModel)]="name"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            [placeholder]="namePlaceholder"
            (keydown.enter)="create()"
            autofocus
          />
          <textarea
            [(ngModel)]="description"
            rows="2"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-none"
            [placeholder]="descPlaceholder"
          ></textarea>
        </div>
        <div class="flex justify-end gap-3 mt-4">
          <button
            (click)="cancelled.emit()"
            class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            i18n="@@common.cancel"
          >
            Cancel
          </button>
          <button
            (click)="create()"
            [disabled]="!name.trim()"
            class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors"
            i18n="@@files.createFolder.create"
          >
            Create
          </button>
        </div>
      </div>
    </div>
  `,
})
export class CreateAlbumDialogComponent {
  private readonly albumService = inject(AlbumService);

  readonly created = output<string>();
  readonly cancelled = output<void>();

  name = '';
  description = '';

  readonly namePlaceholder = $localize`:@@files.albums.namePlaceholder:Album name`;
  readonly descPlaceholder = $localize`:@@files.albums.descPlaceholder:Description (optional)`;

  create(): void {
    const name = this.name.trim();
    if (!name) return;
    this.albumService
      .createAlbum({ name, description: this.description.trim() || undefined })
      .subscribe((albumId) => {
        if (albumId) {
          this.created.emit(albumId);
        }
      });
  }
}
