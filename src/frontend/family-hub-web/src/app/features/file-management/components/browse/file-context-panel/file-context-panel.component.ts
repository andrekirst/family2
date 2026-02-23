import { Component, inject, input, output, signal, OnInit, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../../../../shared/icons/icons';
import { FileService } from '../../../services/file.service';
import { FileDownloadService } from '../../../services/file-download.service';
import { FavoriteService } from '../../../services/favorite.service';
import { TagService } from '../../../services/tag.service';
import { StoredFileDto } from '../../../models/file.models';
import { TagDto } from '../../../models/tag.models';
import { formatBytes } from '../../../utils/file-size.utils';
import { getFileIcon } from '../../../utils/mime-type.utils';
import { TagChipComponent } from '../../tags/tag-chip.component';

@Component({
  selector: 'app-file-context-panel',
  standalone: true,
  imports: [CommonModule, FormsModule, TagChipComponent],
  template: `
    @if (file()) {
      <div class="p-4 space-y-4">
        <!-- File icon / preview -->
        <div class="flex justify-center py-4 relative">
          <div class="text-gray-400" [innerHTML]="largeFileIcon()"></div>
          <!-- Favorite star -->
          <button
            (click)="toggleFavorite()"
            class="absolute top-2 right-2 p-1 rounded-full hover:bg-gray-100 transition-colors"
            [attr.aria-label]="isFavorite() ? unfavoriteLabel : favoriteLabel"
          >
            <span
              [innerHTML]="isFavorite() ? starFilledIcon : starIcon"
              [class.text-yellow-500]="isFavorite()"
              [class.text-gray-300]="!isFavorite()"
            ></span>
          </button>
        </div>

        <!-- Name (inline-editable) -->
        @if (isEditing()) {
          <div class="flex items-center gap-2">
            <input
              type="text"
              [(ngModel)]="editName"
              (keydown.enter)="saveRename()"
              (keydown.escape)="cancelRename()"
              class="flex-1 px-2 py-1 border border-blue-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              autofocus
            />
            <button
              (click)="saveRename()"
              class="text-blue-600 text-sm font-medium"
              i18n="@@files.context.save"
            >
              Save
            </button>
          </div>
        } @else {
          <h3
            class="text-base font-semibold text-gray-900 text-center cursor-pointer hover:text-blue-600 transition-colors"
            (click)="startRename()"
            [attr.title]="file()!.name"
          >
            {{ file()!.name }}
          </h3>
        }

        <!-- Tags section -->
        <div class="space-y-2">
          <div class="flex items-center justify-between">
            <span
              class="text-xs font-medium text-gray-500 uppercase tracking-wider"
              i18n="@@files.context.tags"
              >Tags</span
            >
            <button
              (click)="toggleTagPicker()"
              class="text-xs text-blue-600 hover:text-blue-700 font-medium transition-colors"
              i18n="@@files.context.addTag"
            >
              + Add
            </button>
          </div>
          <div class="flex flex-wrap gap-1">
            @for (tag of fileTags(); track tag.id) {
              <app-tag-chip [tag]="tag" [removable]="true" (removed)="removeTag($event)" />
            }
            @if (fileTags().length === 0) {
              <span class="text-xs text-gray-400" i18n="@@files.context.noTags">No tags</span>
            }
          </div>

          <!-- Tag picker dropdown -->
          @if (showTagPicker()) {
            <div class="border border-gray-200 rounded-lg p-2 bg-white shadow-sm space-y-1">
              @for (tag of availableTags(); track tag.id) {
                <button
                  (click)="addTag(tag)"
                  class="flex items-center gap-2 w-full px-2 py-1.5 text-sm rounded hover:bg-gray-100 transition-colors"
                >
                  <span
                    class="w-3 h-3 rounded-full flex-shrink-0"
                    [style.background-color]="tag.color"
                  ></span>
                  <span class="text-gray-700">{{ tag.name }}</span>
                </button>
              }
              @if (availableTags().length === 0) {
                <p class="text-xs text-gray-400 text-center py-1" i18n="@@files.context.noMoreTags">
                  No more tags available
                </p>
              }
            </div>
          }
        </div>

        <!-- Details -->
        <div class="space-y-3 text-sm">
          <div class="flex justify-between">
            <span class="text-gray-500" i18n="@@files.context.size">Size</span>
            <span class="text-gray-900 font-medium">{{ formatSize(file()!.size) }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500" i18n="@@files.context.type">Type</span>
            <span class="text-gray-900 font-medium">{{ file()!.mimeType }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500" i18n="@@files.context.created">Created</span>
            <span class="text-gray-900 font-medium">{{ file()!.createdAt | date: 'medium' }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500" i18n="@@files.context.modified">Modified</span>
            <span class="text-gray-900 font-medium">{{ file()!.updatedAt | date: 'medium' }}</span>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex flex-col gap-2 pt-2 border-t border-gray-200">
          <button
            (click)="download()"
            class="flex items-center gap-2 px-3 py-2 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition-colors"
          >
            <span [innerHTML]="downloadIcon"></span>
            <span i18n="@@files.action.download">Download</span>
          </button>
          <button
            (click)="moveRequested.emit(file()!.id)"
            class="flex items-center gap-2 px-3 py-2 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition-colors"
          >
            <span [innerHTML]="folderIcon"></span>
            <span i18n="@@files.action.move">Move</span>
          </button>
          <button
            (click)="confirmDelete()"
            class="flex items-center gap-2 px-3 py-2 text-sm text-red-600 rounded-lg hover:bg-red-50 transition-colors"
          >
            <span [innerHTML]="closeIcon"></span>
            <span i18n="@@files.action.delete">Delete</span>
          </button>
        </div>

        <!-- Delete confirmation -->
        @if (showDeleteConfirm()) {
          <div class="p-3 bg-red-50 border border-red-200 rounded-lg">
            <p class="text-sm text-red-700 mb-3" i18n="@@files.context.deleteConfirm">
              Are you sure you want to delete this file?
            </p>
            <div class="flex gap-2">
              <button
                (click)="showDeleteConfirm.set(false)"
                class="flex-1 px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
                i18n="@@common.cancel"
              >
                Cancel
              </button>
              <button
                (click)="doDelete()"
                class="flex-1 px-3 py-1.5 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700"
                i18n="@@common.confirm"
              >
                Confirm
              </button>
            </div>
          </div>
        }
      </div>
    }
  `,
})
export class FileContextPanelComponent implements OnInit, OnChanges {
  private readonly fileService = inject(FileService);
  private readonly downloadService = inject(FileDownloadService);
  private readonly favoriteService = inject(FavoriteService);
  private readonly tagService = inject(TagService);

  readonly fileId = input.required<string>();
  readonly moveRequested = output<string>();
  readonly deleted = output<void>();
  readonly renamed = output<void>();

  readonly file = signal<StoredFileDto | null>(null);
  readonly isEditing = signal(false);
  readonly showDeleteConfirm = signal(false);
  readonly isFavorite = signal(false);
  readonly fileTags = signal<TagDto[]>([]);
  readonly allTags = signal<TagDto[]>([]);
  readonly showTagPicker = signal(false);
  readonly availableTags = signal<TagDto[]>([]);
  editName = '';

  readonly downloadIcon: SafeHtml;
  readonly folderIcon: SafeHtml;
  readonly closeIcon: SafeHtml;
  readonly starIcon: SafeHtml;
  readonly starFilledIcon: SafeHtml;
  private readonly sanitizer: DomSanitizer;

  readonly favoriteLabel = $localize`:@@files.context.favorite:Add to favorites`;
  readonly unfavoriteLabel = $localize`:@@files.context.unfavorite:Remove from favorites`;

  constructor(sanitizer: DomSanitizer) {
    this.sanitizer = sanitizer;
    this.downloadIcon = sanitizer.bypassSecurityTrustHtml(ICONS.DOWNLOAD);
    this.folderIcon = sanitizer.bypassSecurityTrustHtml(ICONS.FOLDER);
    this.closeIcon = sanitizer.bypassSecurityTrustHtml(ICONS.CLOSE);
    this.starIcon = sanitizer.bypassSecurityTrustHtml(ICONS.STAR);
    this.starFilledIcon = sanitizer.bypassSecurityTrustHtml(ICONS.STAR_FILLED);
  }

  ngOnInit(): void {
    this.loadFile();
    this.loadAllTags();
  }

  ngOnChanges(): void {
    this.loadFile();
    this.showTagPicker.set(false);
  }

  largeFileIcon(): SafeHtml {
    const f = this.file();
    if (!f) return '';
    const svg = getFileIcon(f.mimeType).replace('h-5 w-5', 'h-16 w-16');
    return this.sanitizer.bypassSecurityTrustHtml(svg);
  }

  formatSize(bytes: number): string {
    return formatBytes(bytes);
  }

  download(): void {
    const f = this.file();
    if (f) {
      this.downloadService.download(f.storageKey, f.name);
    }
  }

  toggleFavorite(): void {
    const f = this.file();
    if (!f) return;
    this.favoriteService.toggleFavorite(f.id).subscribe(() => {
      this.isFavorite.update((v) => !v);
    });
  }

  toggleTagPicker(): void {
    this.showTagPicker.update((v) => !v);
    if (this.showTagPicker()) {
      this.updateAvailableTags();
    }
  }

  addTag(tag: TagDto): void {
    const f = this.file();
    if (!f) return;
    this.tagService.tagFile(f.id, tag.id).subscribe(() => {
      this.fileTags.update((tags) => [...tags, tag]);
      this.updateAvailableTags();
    });
  }

  removeTag(tag: TagDto): void {
    const f = this.file();
    if (!f) return;
    this.tagService.untagFile(f.id, tag.id).subscribe(() => {
      this.fileTags.update((tags) => tags.filter((t) => t.id !== tag.id));
      this.updateAvailableTags();
    });
  }

  startRename(): void {
    this.editName = this.file()?.name ?? '';
    this.isEditing.set(true);
  }

  cancelRename(): void {
    this.isEditing.set(false);
  }

  saveRename(): void {
    const name = this.editName.trim();
    const f = this.file();
    if (!name || !f || name === f.name) {
      this.isEditing.set(false);
      return;
    }
    this.fileService.renameFile({ fileId: f.id, newName: name }).subscribe((updated) => {
      if (updated) {
        this.file.set(updated);
        this.renamed.emit();
      }
      this.isEditing.set(false);
    });
  }

  confirmDelete(): void {
    this.showDeleteConfirm.set(true);
  }

  doDelete(): void {
    const f = this.file();
    if (!f) return;
    this.fileService.deleteFile(f.id).subscribe((ok) => {
      if (ok) {
        this.deleted.emit();
      }
      this.showDeleteConfirm.set(false);
    });
  }

  private loadFile(): void {
    this.fileService.getFile(this.fileId()).subscribe((f) => this.file.set(f));
    // TODO: Load file-specific tags and favorite status from backend
    // For now, reset state on file change
    this.fileTags.set([]);
    this.isFavorite.set(false);
  }

  private loadAllTags(): void {
    this.tagService.getTags().subscribe((tags) => {
      this.allTags.set(tags);
      this.updateAvailableTags();
    });
  }

  private updateAvailableTags(): void {
    const fileTagIds = new Set(this.fileTags().map((t) => t.id));
    this.availableTags.set(this.allTags().filter((t) => !fileTagIds.has(t.id)));
  }
}
