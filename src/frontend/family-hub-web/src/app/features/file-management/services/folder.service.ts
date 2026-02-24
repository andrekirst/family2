import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import {
  FolderDto,
  CreateFolderInput,
  RenameFolderInput,
  MoveFolderInput,
} from '../models/folder.models';
import {
  GET_FOLDERS,
  GET_FOLDER,
  GET_BREADCRUMB,
  CREATE_FOLDER,
  RENAME_FOLDER,
  MOVE_FOLDER,
  DELETE_FOLDER,
} from '../graphql/folder.operations';

@Injectable({ providedIn: 'root' })
export class FolderService {
  private readonly apollo = inject(Apollo);

  getFolders(parentFolderId: string): Observable<FolderDto[]> {
    return this.apollo
      .query<{ fileManagement: { folders: FolderDto[] } }>({
        query: GET_FOLDERS,
        variables: { parentFolderId },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.folders),
        catchError((err) => {
          console.error('Failed to load folders:', err);
          return of([]);
        }),
      );
  }

  getFolder(folderId: string): Observable<FolderDto | null> {
    return this.apollo
      .query<{ fileManagement: { folder: FolderDto } }>({
        query: GET_FOLDER,
        variables: { folderId },
      })
      .pipe(
        map((r) => r.data!.fileManagement.folder),
        catchError((err) => {
          console.error('Failed to load folder:', err);
          return of(null);
        }),
      );
  }

  getBreadcrumb(folderId: string): Observable<FolderDto[]> {
    return this.apollo
      .query<{ fileManagement: { breadcrumb: FolderDto[] } }>({
        query: GET_BREADCRUMB,
        variables: { folderId },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.breadcrumb),
        catchError((err) => {
          console.error('Failed to load breadcrumb:', err);
          return of([]);
        }),
      );
  }

  createFolder(input: CreateFolderInput): Observable<FolderDto | null> {
    return this.apollo
      .mutate<{ fileManagement: { createFolder: FolderDto } }>({
        mutation: CREATE_FOLDER,
        variables: { input },
      })
      .pipe(
        map((r) => r.data?.fileManagement.createFolder ?? null),
        catchError((err) => {
          console.error('Failed to create folder:', err);
          return of(null);
        }),
      );
  }

  renameFolder(input: RenameFolderInput): Observable<FolderDto | null> {
    return this.apollo
      .mutate<{ fileManagement: { renameFolder: FolderDto } }>({
        mutation: RENAME_FOLDER,
        variables: { input },
      })
      .pipe(
        map((r) => r.data?.fileManagement.renameFolder ?? null),
        catchError((err) => {
          console.error('Failed to rename folder:', err);
          return of(null);
        }),
      );
  }

  moveFolder(input: MoveFolderInput): Observable<FolderDto | null> {
    return this.apollo
      .mutate<{ fileManagement: { moveFolder: FolderDto } }>({
        mutation: MOVE_FOLDER,
        variables: { input },
      })
      .pipe(
        map((r) => r.data?.fileManagement.moveFolder ?? null),
        catchError((err) => {
          console.error('Failed to move folder:', err);
          return of(null);
        }),
      );
  }

  deleteFolder(folderId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { deleteFolder: boolean } }>({
        mutation: DELETE_FOLDER,
        variables: { folderId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.deleteFolder ?? false),
        catchError((err) => {
          console.error('Failed to delete folder:', err);
          return of(false);
        }),
      );
  }
}
