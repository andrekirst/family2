import { Component, inject, output, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TagService } from '../../services/tag.service';
import { TagDto } from '../../models/tag.models';
import { TagChipComponent } from './tag-chip.component';

const DEFAULT_COLORS = [
  '#EF4444',
  '#F97316',
  '#EAB308',
  '#22C55E',
  '#3B82F6',
  '#8B5CF6',
  '#EC4899',
  '#6B7280',
];

@Component({
  selector: 'app-tag-manager-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, TagChipComponent],
  template: `
    <div
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50"
      (click)="closed.emit()"
    >
      <div
        class="bg-white rounded-xl shadow-xl w-full max-w-md mx-4 p-6"
        (click)="$event.stopPropagation()"
      >
        <h2 class="text-lg font-semibold text-gray-900 mb-4" i18n="@@files.tags.manageTitle">
          Manage Tags
        </h2>

        <!-- Create new tag -->
        <div class="flex items-center gap-2 mb-4">
          <input
            type="text"
            [(ngModel)]="newTagName"
            class="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            [placeholder]="newTagPlaceholder"
            (keydown.enter)="createTag()"
          />
          <div class="flex gap-1">
            @for (color of colors; track color) {
              <button
                (click)="selectedColor = color"
                class="w-6 h-6 rounded-full border-2 transition-all"
                [style.background-color]="color"
                [class.border-gray-900]="selectedColor === color"
                [class.border-transparent]="selectedColor !== color"
                [class.scale-110]="selectedColor === color"
              ></button>
            }
          </div>
          <button
            (click)="createTag()"
            [disabled]="!newTagName.trim()"
            class="px-3 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors"
            i18n="@@files.tags.add"
          >
            Add
          </button>
        </div>

        <!-- Existing tags -->
        <div class="space-y-2 max-h-60 overflow-y-auto">
          @for (tag of tags(); track tag.id) {
            <div class="flex items-center justify-between py-2 px-3 rounded-lg hover:bg-gray-50">
              <div class="flex items-center gap-2">
                <app-tag-chip [tag]="tag" />
                <span class="text-xs text-gray-500">{{ tag.fileCount }} files</span>
              </div>
              <button
                (click)="deleteTag(tag.id)"
                class="text-xs text-red-500 hover:text-red-700 transition-colors"
                i18n="@@files.action.delete"
              >
                Delete
              </button>
            </div>
          }
          @if (tags().length === 0) {
            <p class="text-sm text-gray-400 text-center py-4" i18n="@@files.tags.noTags">
              No tags yet. Create one above.
            </p>
          }
        </div>

        <!-- Footer -->
        <div class="flex justify-end mt-4 pt-3 border-t border-gray-200">
          <button
            (click)="closed.emit()"
            class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            i18n="@@common.close"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  `,
})
export class TagManagerDialogComponent implements OnInit {
  private readonly tagService = inject(TagService);

  readonly closed = output<void>();
  readonly tagsChanged = output<void>();
  readonly tags = signal<TagDto[]>([]);
  readonly colors = DEFAULT_COLORS;

  newTagName = '';
  selectedColor = DEFAULT_COLORS[4]; // blue default

  readonly newTagPlaceholder = $localize`:@@files.tags.namePlaceholder:Tag name`;

  ngOnInit(): void {
    this.loadTags();
  }

  createTag(): void {
    const name = this.newTagName.trim();
    if (!name) return;

    this.tagService.createTag({ name, color: this.selectedColor }).subscribe((tag) => {
      if (tag) {
        this.newTagName = '';
        this.loadTags();
        this.tagsChanged.emit();
      }
    });
  }

  deleteTag(tagId: string): void {
    this.tagService.deleteTag(tagId).subscribe((ok) => {
      if (ok) {
        this.loadTags();
        this.tagsChanged.emit();
      }
    });
  }

  private loadTags(): void {
    this.tagService.getTags().subscribe((tags) => this.tags.set(tags));
  }
}
