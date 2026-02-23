import { Injectable, signal, computed } from '@angular/core';

export type ViewMode = 'grid' | 'list';
export type SortField = 'name' | 'size' | 'createdAt' | 'updatedAt';
export type SortDirection = 'asc' | 'desc';

@Injectable({ providedIn: 'root' })
export class FileStateService {
  readonly currentFolderId = signal<string | null>(null);
  readonly selectedFileIds = signal<Set<string>>(new Set());
  readonly viewMode = signal<ViewMode>('grid');
  readonly sortBy = signal<SortField>('name');
  readonly sortDirection = signal<SortDirection>('asc');

  readonly hasSelection = computed(() => this.selectedFileIds().size > 0);
  readonly selectionCount = computed(() => this.selectedFileIds().size);

  navigateToFolder(folderId: string | null): void {
    this.currentFolderId.set(folderId);
    this.clearSelection();
  }

  toggleFileSelection(fileId: string): void {
    this.selectedFileIds.update((ids) => {
      const next = new Set(ids);
      if (next.has(fileId)) {
        next.delete(fileId);
      } else {
        next.add(fileId);
      }
      return next;
    });
  }

  selectFile(fileId: string): void {
    this.selectedFileIds.set(new Set([fileId]));
  }

  clearSelection(): void {
    this.selectedFileIds.set(new Set());
  }

  toggleViewMode(): void {
    this.viewMode.update((m) => (m === 'grid' ? 'list' : 'grid'));
  }

  setSort(field: SortField): void {
    if (this.sortBy() === field) {
      this.sortDirection.update((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      this.sortBy.set(field);
      this.sortDirection.set('asc');
    }
  }
}
