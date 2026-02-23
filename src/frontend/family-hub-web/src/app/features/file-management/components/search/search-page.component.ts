import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';
import { ICONS } from '../../../../shared/icons/icons';
import { SearchService } from '../../services/search.service';
import { TagService } from '../../services/tag.service';
import { FileDownloadService } from '../../services/file-download.service';
import { FileSearchResultDto, SearchFilters, SavedSearchDto } from '../../models/search.models';
import { TagDto } from '../../models/tag.models';
import { formatBytes } from '../../utils/file-size.utils';
import { getFileIcon } from '../../utils/mime-type.utils';

const MIME_CATEGORIES = [
  { label: $localize`:@@files.search.filterImages:Images`, value: 'image/*' },
  { label: $localize`:@@files.search.filterDocuments:Documents`, value: 'application/pdf' },
  { label: $localize`:@@files.search.filterVideos:Videos`, value: 'video/*' },
  { label: $localize`:@@files.search.filterAudio:Audio`, value: 'audio/*' },
];

@Component({
  selector: 'app-search-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex h-full">
      <!-- Sidebar: Saved Searches -->
      <div class="w-64 border-r border-gray-200 bg-gray-50 flex flex-col flex-shrink-0">
        <div class="px-4 py-3 border-b border-gray-200">
          <h3
            class="text-xs font-medium text-gray-500 uppercase tracking-wider"
            i18n="@@files.search.savedSearches"
          >
            Saved Searches
          </h3>
        </div>
        <div class="flex-1 overflow-y-auto">
          @for (saved of savedSearches(); track saved.id) {
            <div
              class="flex items-center justify-between px-4 py-2.5 hover:bg-gray-100 cursor-pointer transition-colors group"
              (click)="applySavedSearch(saved)"
            >
              <div class="flex items-center gap-2 min-w-0">
                <span [innerHTML]="searchIcon" class="text-gray-400 flex-shrink-0"></span>
                <span class="text-sm text-gray-700 truncate">{{ saved.query }}</span>
              </div>
              <button
                (click)="deleteSavedSearch(saved.id, $event)"
                class="opacity-0 group-hover:opacity-100 p-0.5 rounded text-gray-400 hover:text-red-500 transition-all"
                [attr.aria-label]="'Delete saved search'"
              >
                <span [innerHTML]="closeIcon"></span>
              </button>
            </div>
          }
          @if (savedSearches().length === 0) {
            <p class="text-xs text-gray-400 text-center py-6" i18n="@@files.search.noSavedSearches">
              No saved searches yet
            </p>
          }
        </div>

        <!-- Recent Searches -->
        <div class="border-t border-gray-200">
          <div class="px-4 py-3">
            <h3
              class="text-xs font-medium text-gray-500 uppercase tracking-wider"
              i18n="@@files.search.recentSearches"
            >
              Recent
            </h3>
          </div>
          <div class="max-h-32 overflow-y-auto">
            @for (recent of recentSearches(); track recent.id) {
              <button
                (click)="applySavedSearch(recent)"
                class="w-full flex items-center gap-2 px-4 py-2 text-sm text-gray-500 hover:bg-gray-100 transition-colors"
              >
                <span class="truncate">{{ recent.query }}</span>
              </button>
            }
          </div>
        </div>
      </div>

      <!-- Main search area -->
      <div class="flex-1 flex flex-col overflow-hidden">
        <!-- Search bar -->
        <div class="px-6 py-4 border-b border-gray-200 bg-white space-y-3">
          <div class="flex items-center gap-3">
            <div class="relative flex-1">
              <span
                [innerHTML]="searchIcon"
                class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"
              ></span>
              <input
                type="text"
                [(ngModel)]="query"
                (ngModelChange)="onQueryChange($event)"
                class="w-full pl-10 pr-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                [placeholder]="searchPlaceholder"
                data-testid="search-input"
              />
            </div>
            <button
              (click)="toggleFilters()"
              class="flex items-center gap-1 px-3 py-2.5 text-sm rounded-lg border transition-colors"
              [class.border-blue-500]="showFilters()"
              [class.text-blue-600]="showFilters()"
              [class.border-gray-300]="!showFilters()"
              [class.text-gray-600]="!showFilters()"
              data-testid="filter-toggle"
            >
              <span i18n="@@files.search.filters">Filters</span>
              @if (activeFilterCount() > 0) {
                <span
                  class="inline-flex items-center justify-center w-5 h-5 text-xs font-medium text-white bg-blue-600 rounded-full"
                >
                  {{ activeFilterCount() }}
                </span>
              }
            </button>
            <button
              (click)="saveCurrentSearch()"
              [disabled]="!query.trim()"
              class="px-3 py-2.5 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 transition-colors"
              i18n="@@files.search.save"
            >
              Save
            </button>
          </div>

          <!-- Filters panel -->
          @if (showFilters()) {
            <div
              class="flex flex-wrap items-center gap-4 pt-3 border-t border-gray-100"
              data-testid="filter-panel"
            >
              <!-- MIME type filter -->
              <div class="flex items-center gap-2">
                <span class="text-xs font-medium text-gray-500" i18n="@@files.search.filterType"
                  >Type</span
                >
                <div class="flex gap-1">
                  @for (cat of mimeCategories; track cat.value) {
                    <button
                      (click)="toggleMimeFilter(cat.value)"
                      class="px-2.5 py-1 text-xs rounded-full border transition-colors"
                      [class.bg-blue-100]="isMimeSelected(cat.value)"
                      [class.border-blue-300]="isMimeSelected(cat.value)"
                      [class.text-blue-700]="isMimeSelected(cat.value)"
                      [class.bg-white]="!isMimeSelected(cat.value)"
                      [class.border-gray-300]="!isMimeSelected(cat.value)"
                      [class.text-gray-600]="!isMimeSelected(cat.value)"
                    >
                      {{ cat.label }}
                    </button>
                  }
                </div>
              </div>

              <!-- Date range -->
              <div class="flex items-center gap-2">
                <span class="text-xs font-medium text-gray-500" i18n="@@files.search.filterDate"
                  >Date</span
                >
                <input
                  type="date"
                  [(ngModel)]="filterDateFrom"
                  (ngModelChange)="applyFilters()"
                  class="px-2 py-1 text-xs border border-gray-300 rounded"
                />
                <span class="text-xs text-gray-400">—</span>
                <input
                  type="date"
                  [(ngModel)]="filterDateTo"
                  (ngModelChange)="applyFilters()"
                  class="px-2 py-1 text-xs border border-gray-300 rounded"
                />
              </div>

              <!-- Tag filter -->
              @if (allTags().length > 0) {
                <div class="flex items-center gap-2">
                  <span class="text-xs font-medium text-gray-500" i18n="@@files.search.filterTags"
                    >Tags</span
                  >
                  <div class="flex gap-1">
                    @for (tag of allTags(); track tag.id) {
                      <button
                        (click)="toggleTagFilter(tag.id)"
                        class="px-2.5 py-1 text-xs rounded-full border transition-colors"
                        [style.background-color]="isTagSelected(tag.id) ? tag.color + '20' : ''"
                        [style.border-color]="isTagSelected(tag.id) ? tag.color : ''"
                        [style.color]="isTagSelected(tag.id) ? tag.color : ''"
                        [class.bg-white]="!isTagSelected(tag.id)"
                        [class.border-gray-300]="!isTagSelected(tag.id)"
                        [class.text-gray-600]="!isTagSelected(tag.id)"
                      >
                        {{ tag.name }}
                      </button>
                    }
                  </div>
                </div>
              }

              <!-- Clear filters -->
              @if (activeFilterCount() > 0) {
                <button
                  (click)="clearFilters()"
                  class="text-xs text-red-500 hover:text-red-700 transition-colors"
                  i18n="@@files.search.clearFilters"
                >
                  Clear all
                </button>
              }
            </div>
          }
        </div>

        <!-- Results -->
        <div class="flex-1 overflow-y-auto">
          @if (searching()) {
            <div class="flex items-center justify-center py-16">
              <div
                class="animate-spin rounded-full h-8 w-8 border-2 border-blue-600 border-t-transparent"
              ></div>
            </div>
          } @else if (results().length > 0) {
            <div class="divide-y divide-gray-100">
              @for (result of results(); track result.id) {
                <div
                  class="flex items-center gap-4 px-6 py-3 hover:bg-gray-50 cursor-pointer transition-colors"
                  (click)="downloadResult(result)"
                >
                  <span
                    [innerHTML]="getResultIcon(result.mimeType)"
                    class="text-gray-400 flex-shrink-0"
                  ></span>
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-gray-900 truncate">
                      {{ result.name }}
                    </p>
                    <p class="text-xs text-gray-500">
                      {{ formatSize(result.size) }} · {{ result.createdAt | date: 'mediumDate' }}
                    </p>
                  </div>
                  @if (result.relevance) {
                    <span class="text-xs text-gray-400 flex-shrink-0">
                      {{ (result.relevance * 100).toFixed(0) }}%
                    </span>
                  }
                </div>
              }
            </div>

            <!-- Load more -->
            @if (hasMore()) {
              <div class="flex justify-center py-4">
                <button
                  (click)="loadMore()"
                  class="px-4 py-2 text-sm text-blue-600 hover:text-blue-700 font-medium transition-colors"
                  i18n="@@files.search.loadMore"
                >
                  Load more results
                </button>
              </div>
            }
          } @else if (hasSearched()) {
            <div class="flex flex-col items-center justify-center py-16 text-gray-400">
              <span [innerHTML]="searchIconLg" class="mb-3"></span>
              <p class="text-sm font-medium" i18n="@@files.search.noResults">No results found</p>
              <p class="text-xs mt-1" i18n="@@files.search.tryDifferent">
                Try a different search term or adjust your filters.
              </p>
            </div>
          } @else {
            <div class="flex flex-col items-center justify-center py-16 text-gray-400">
              <span [innerHTML]="searchIconLg" class="mb-3"></span>
              <p class="text-sm font-medium" i18n="@@files.search.startSearching">
                Search across all your files
              </p>
              <p class="text-xs mt-1" i18n="@@files.search.startSearchingDesc">
                Enter a search term to find files by name or content.
              </p>
            </div>
          }
        </div>
      </div>
    </div>
  `,
})
export class SearchPageComponent implements OnInit {
  private readonly searchService = inject(SearchService);
  private readonly tagService = inject(TagService);
  private readonly downloadService = inject(FileDownloadService);
  private readonly sanitizer = inject(DomSanitizer);

  readonly searchIcon: SafeHtml;
  readonly searchIconLg: SafeHtml;
  readonly closeIcon: SafeHtml;

  readonly results = signal<FileSearchResultDto[]>([]);
  readonly savedSearches = signal<SavedSearchDto[]>([]);
  readonly recentSearches = signal<SavedSearchDto[]>([]);
  readonly allTags = signal<TagDto[]>([]);
  readonly searching = signal(false);
  readonly showFilters = signal(false);
  readonly hasSearched = signal(false);
  readonly hasMore = signal(false);

  query = '';
  filterDateFrom = '';
  filterDateTo = '';
  private selectedMimeTypes = new Set<string>();
  private selectedTagIds = new Set<string>();
  private currentSkip = 0;
  private readonly pageSize = 20;

  readonly mimeCategories = MIME_CATEGORIES;
  readonly searchPlaceholder = $localize`:@@files.search.placeholder:Search files...`;

  private readonly searchSubject = new Subject<string>();

  constructor(sanitizer: DomSanitizer) {
    this.searchIcon = sanitizer.bypassSecurityTrustHtml(ICONS.SEARCH);
    const lgSearch = ICONS.SEARCH.replace('h-5 w-5', 'h-12 w-12');
    this.searchIconLg = sanitizer.bypassSecurityTrustHtml(lgSearch);
    this.closeIcon = sanitizer.bypassSecurityTrustHtml(ICONS.CLOSE);

    this.searchSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((q) => {
          if (!q.trim()) {
            this.results.set([]);
            this.hasSearched.set(false);
            return of([]);
          }
          this.searching.set(true);
          this.currentSkip = 0;
          return this.searchService.searchFiles(
            q,
            this.buildFilters(),
            'relevance',
            0,
            this.pageSize,
          );
        }),
      )
      .subscribe((results) => {
        this.results.set(results);
        this.hasMore.set(results.length === this.pageSize);
        this.searching.set(false);
        if (this.query.trim()) {
          this.hasSearched.set(true);
        }
      });
  }

  ngOnInit(): void {
    this.searchService.getSavedSearches().subscribe((s) => this.savedSearches.set(s));
    this.searchService.getRecentSearches().subscribe((s) => this.recentSearches.set(s));
    this.tagService.getTags().subscribe((tags) => this.allTags.set(tags));
  }

  onQueryChange(query: string): void {
    this.searchSubject.next(query);
  }

  toggleFilters(): void {
    this.showFilters.update((v) => !v);
  }

  activeFilterCount(): number {
    let count = 0;
    if (this.selectedMimeTypes.size > 0) count++;
    if (this.filterDateFrom || this.filterDateTo) count++;
    if (this.selectedTagIds.size > 0) count++;
    return count;
  }

  toggleMimeFilter(mime: string): void {
    if (this.selectedMimeTypes.has(mime)) {
      this.selectedMimeTypes.delete(mime);
    } else {
      this.selectedMimeTypes.add(mime);
    }
    this.applyFilters();
  }

  isMimeSelected(mime: string): boolean {
    return this.selectedMimeTypes.has(mime);
  }

  toggleTagFilter(tagId: string): void {
    if (this.selectedTagIds.has(tagId)) {
      this.selectedTagIds.delete(tagId);
    } else {
      this.selectedTagIds.add(tagId);
    }
    this.applyFilters();
  }

  isTagSelected(tagId: string): boolean {
    return this.selectedTagIds.has(tagId);
  }

  clearFilters(): void {
    this.selectedMimeTypes.clear();
    this.selectedTagIds.clear();
    this.filterDateFrom = '';
    this.filterDateTo = '';
    this.applyFilters();
  }

  applyFilters(): void {
    if (this.query.trim()) {
      this.searchSubject.next(this.query);
    }
  }

  loadMore(): void {
    this.currentSkip += this.pageSize;
    this.searchService
      .searchFiles(this.query, this.buildFilters(), 'relevance', this.currentSkip, this.pageSize)
      .subscribe((more) => {
        this.results.update((existing) => [...existing, ...more]);
        this.hasMore.set(more.length === this.pageSize);
      });
  }

  applySavedSearch(saved: SavedSearchDto): void {
    this.query = saved.query;
    if (saved.filters) {
      this.selectedMimeTypes = new Set(saved.filters.mimeTypes ?? []);
      this.selectedTagIds = new Set(saved.filters.tagIds ?? []);
      this.filterDateFrom = saved.filters.dateFrom ?? '';
      this.filterDateTo = saved.filters.dateTo ?? '';
    }
    this.searchSubject.next(this.query);
  }

  saveCurrentSearch(): void {
    if (!this.query.trim()) return;
    this.searchService.saveSearch(this.query, this.buildFilters()).subscribe((saved) => {
      if (saved) {
        this.savedSearches.update((list) => [saved, ...list]);
      }
    });
  }

  deleteSavedSearch(id: string, event: Event): void {
    event.stopPropagation();
    this.searchService.deleteSavedSearch(id).subscribe((ok) => {
      if (ok) {
        this.savedSearches.update((list) => list.filter((s) => s.id !== id));
      }
    });
  }

  downloadResult(result: FileSearchResultDto): void {
    // For search results, we use the file id to fetch the storage key
    // In practice the StoredFileDto would have a storageKey, but search results may not
    // For now, just use the id as a navigation trigger
    this.downloadService.download(result.id, result.name);
  }

  getResultIcon(mimeType: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(getFileIcon(mimeType));
  }

  formatSize(bytes: number): string {
    return formatBytes(bytes);
  }

  private buildFilters(): SearchFilters {
    return {
      mimeTypes: this.selectedMimeTypes.size > 0 ? [...this.selectedMimeTypes] : undefined,
      dateFrom: this.filterDateFrom || undefined,
      dateTo: this.filterDateTo || undefined,
      tagIds: this.selectedTagIds.size > 0 ? [...this.selectedTagIds] : undefined,
    };
  }
}
