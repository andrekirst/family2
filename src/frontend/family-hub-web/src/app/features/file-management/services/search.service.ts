import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import { FileSearchResultDto, SavedSearchDto, SearchFilters } from '../models/search.models';
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

  getRecentSearches(): Observable<SavedSearchDto[]> {
    return this.apollo
      .query<{ fileManagement: { getRecentSearches: SavedSearchDto[] } }>({
        query: GET_RECENT_SEARCHES,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.getRecentSearches),
        catchError((err) => {
          console.error('Failed to load recent searches:', err);
          return of([]);
        }),
      );
  }

  getSavedSearches(): Observable<SavedSearchDto[]> {
    return this.apollo
      .query<{ fileManagement: { getSavedSearches: SavedSearchDto[] } }>({
        query: GET_SAVED_SEARCHES,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.getSavedSearches),
        catchError((err) => {
          console.error('Failed to load saved searches:', err);
          return of([]);
        }),
      );
  }

  saveSearch(query: string, filters: SearchFilters): Observable<SavedSearchDto | null> {
    return this.apollo
      .mutate<{ fileManagement: { saveSearch: SavedSearchDto } }>({
        mutation: SAVE_SEARCH,
        variables: { query, filters },
      })
      .pipe(
        map((r) => r.data?.fileManagement.saveSearch ?? null),
        catchError((err) => {
          console.error('Failed to save search:', err);
          return of(null);
        }),
      );
  }

  deleteSavedSearch(savedSearchId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { deleteSavedSearch: boolean } }>({
        mutation: DELETE_SAVED_SEARCH,
        variables: { savedSearchId },
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
