import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import { AlbumDto, CreateAlbumInput } from '../models/album.models';
import {
  GET_ALBUMS,
  CREATE_ALBUM,
  RENAME_ALBUM,
  DELETE_ALBUM,
  ADD_FILE_TO_ALBUM,
  REMOVE_FILE_FROM_ALBUM,
} from '../graphql/album.operations';

@Injectable({ providedIn: 'root' })
export class AlbumService {
  private readonly apollo = inject(Apollo);

  getAlbums(): Observable<AlbumDto[]> {
    return this.apollo
      .query<{ fileManagement: { getAlbums: AlbumDto[] } }>({
        query: GET_ALBUMS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.getAlbums),
        catchError((err) => {
          console.error('Failed to load albums:', err);
          return of([]);
        }),
      );
  }

  createAlbum(input: CreateAlbumInput): Observable<string | null> {
    return this.apollo
      .mutate<{ fileManagement: { createAlbum: { albumId: string } } }>({
        mutation: CREATE_ALBUM,
        variables: { input },
      })
      .pipe(
        map((r) => r.data?.fileManagement.createAlbum.albumId ?? null),
        catchError((err) => {
          console.error('Failed to create album:', err);
          return of(null);
        }),
      );
  }

  renameAlbum(albumId: string, name: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { renameAlbum: { albumId: string } } }>({
        mutation: RENAME_ALBUM,
        variables: { input: { albumId, name } },
      })
      .pipe(
        map((r) => !!r.data?.fileManagement.renameAlbum.albumId),
        catchError((err) => {
          console.error('Failed to rename album:', err);
          return of(false);
        }),
      );
  }

  deleteAlbum(albumId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { deleteAlbum: { success: boolean } } }>({
        mutation: DELETE_ALBUM,
        variables: { albumId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.deleteAlbum.success ?? false),
        catchError((err) => {
          console.error('Failed to delete album:', err);
          return of(false);
        }),
      );
  }

  addFileToAlbum(albumId: string, fileId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { addFileToAlbum: { success: boolean } } }>({
        mutation: ADD_FILE_TO_ALBUM,
        variables: { albumId, fileId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.addFileToAlbum.success ?? false),
        catchError((err) => {
          console.error('Failed to add file to album:', err);
          return of(false);
        }),
      );
  }

  removeFileFromAlbum(albumId: string, fileId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { removeFileFromAlbum: { success: boolean } } }>({
        mutation: REMOVE_FILE_FROM_ALBUM,
        variables: { albumId, fileId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.removeFileFromAlbum.success ?? false),
        catchError((err) => {
          console.error('Failed to remove file from album:', err);
          return of(false);
        }),
      );
  }
}
