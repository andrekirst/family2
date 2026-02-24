import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import {
  FileSearchResultDto,
  RecentSearchDto,
  SavedSearchDto,
  SearchFilters,
} from '../models/search.models';
import {
  SEARCH_FILES,
  GET_RECENT_SEARCHES,
  GET_SAVED_SEARCHES,
  SAVE_SEARCH,
  DELETE_SAVED_SEARCH,
} from '../graphql/search.operations';

@Injectable({ providedIn: 'root' })
export class SearchService {
  private readonly apollo = inject(Apollo);

  searchFiles(
    query: string,
    filters?: SearchFilters,
    sortBy = 'relevance',
    skip = 0,
    take = 20,
  ): Observable<FileSearchResultDto[]> {
    return this.apollo
      .query<{ fileManagement: { searchFiles: FileSearchResultDto[] } }>({
        query: SEARCH_FILES,
        variables: {
          query,
          mimeTypes: filters?.mimeTypes,
          dateFrom: filters?.dateFrom,
          dateTo: filters?.dateTo,
          tagIds: filters?.tagIds,
          folderId: filters?.folderId,
          sortBy,
          skip,
          take,
        },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.searchFiles),
        catchError((err) => {
          console.error('Failed to search files:', err);
          return of([]);
        }),
      );
  }

  getRecentSearches(): Observable<RecentSearchDto[]> {
    return this.apollo
      .query<{ fileManagement: { recentSearches: RecentSearchDto[] } }>({
        query: GET_RECENT_SEARCHES,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.recentSearches),
        catchError((err) => {
          console.error('Failed to load recent searches:', err);
          return of([]);
        }),
      );
  }

  getSavedSearches(): Observable<SavedSearchDto[]> {
    return this.apollo
      .query<{ fileManagement: { savedSearches: SavedSearchDto[] } }>({
        query: GET_SAVED_SEARCHES,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.savedSearches),
        catchError((err) => {
          console.error('Failed to load saved searches:', err);
          return of([]);
        }),
      );
  }

  saveSearch(name: string, query: string, filtersJson?: string): Observable<string | null> {
    return this.apollo
      .mutate<{ fileManagement: { saveSearch: { success: boolean; savedSearchId: string } } }>({
        mutation: SAVE_SEARCH,
        variables: { name, query, filtersJson },
      })
      .pipe(
        map((r) => r.data?.fileManagement.saveSearch.savedSearchId ?? null),
        catchError((err) => {
          console.error('Failed to save search:', err);
          return of(null);
        }),
      );
  }

  deleteSavedSearch(searchId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { deleteSavedSearch: boolean } }>({
        mutation: DELETE_SAVED_SEARCH,
        variables: { searchId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.deleteSavedSearch ?? false),
        catchError((err) => {
          console.error('Failed to delete saved search:', err);
          return of(false);
        }),
      );
  }
}
