import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import { StoredFileDto } from '../models/file.models';
import { GET_FAVORITES, TOGGLE_FAVORITE } from '../graphql/favorite.operations';

@Injectable({ providedIn: 'root' })
export class FavoriteService {
  private readonly apollo = inject(Apollo);

  getFavorites(): Observable<StoredFileDto[]> {
    return this.apollo
      .query<{ fileManagement: { getFavorites: StoredFileDto[] } }>({
        query: GET_FAVORITES,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.getFavorites),
        catchError((err) => {
          console.error('Failed to load favorites:', err);
          return of([]);
        }),
      );
  }

  toggleFavorite(fileId: string): Observable<StoredFileDto | null> {
    return this.apollo
      .mutate<{ fileManagement: { toggleFavorite: StoredFileDto } }>({
        mutation: TOGGLE_FAVORITE,
        variables: { fileId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.toggleFavorite ?? null),
        catchError((err) => {
          console.error('Failed to toggle favorite:', err);
          return of(null);
        }),
      );
  }
}
