import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import {
  StoredFileDto,
  UploadFileInput,
  RenameFileInput,
  MoveFileInput,
} from '../models/file.models';
import {
  GET_FILES,
  GET_FILE,
  UPLOAD_FILE,
  RENAME_FILE,
  MOVE_FILE,
  DELETE_FILE,
} from '../graphql/file.operations';

@Injectable({ providedIn: 'root' })
export class FileService {
  private readonly apollo = inject(Apollo);

  getFiles(folderId: string): Observable<StoredFileDto[]> {
    return this.apollo
      .query<{ fileManagement: { getFiles: StoredFileDto[] } }>({
        query: GET_FILES,
        variables: { folderId },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.getFiles),
        catchError((err) => {
          console.error('Failed to load files:', err);
          return of([]);
        }),
      );
  }

  getFile(fileId: string): Observable<StoredFileDto | null> {
    return this.apollo
      .query<{ fileManagement: { getFile: StoredFileDto } }>({
        query: GET_FILE,
        variables: { fileId },
      })
      .pipe(
        map((r) => r.data!.fileManagement.getFile),
        catchError((err) => {
          console.error('Failed to load file:', err);
          return of(null);
        }),
      );
  }

  uploadFile(input: UploadFileInput): Observable<StoredFileDto | null> {
    return this.apollo
      .mutate<{ fileManagement: { uploadFile: StoredFileDto } }>({
        mutation: UPLOAD_FILE,
        variables: { input },
      })
      .pipe(
        map((r) => r.data?.fileManagement.uploadFile ?? null),
        catchError((err) => {
          console.error('Failed to register file:', err);
          return of(null);
        }),
      );
  }

  renameFile(input: RenameFileInput): Observable<StoredFileDto | null> {
    return this.apollo
      .mutate<{ fileManagement: { renameFile: StoredFileDto } }>({
        mutation: RENAME_FILE,
        variables: { input },
      })
      .pipe(
        map((r) => r.data?.fileManagement.renameFile ?? null),
        catchError((err) => {
          console.error('Failed to rename file:', err);
          return of(null);
        }),
      );
  }

  moveFile(input: MoveFileInput): Observable<StoredFileDto | null> {
    return this.apollo
      .mutate<{ fileManagement: { moveFile: StoredFileDto } }>({
        mutation: MOVE_FILE,
        variables: { input },
      })
      .pipe(
        map((r) => r.data?.fileManagement.moveFile ?? null),
        catchError((err) => {
          console.error('Failed to move file:', err);
          return of(null);
        }),
      );
  }

  deleteFile(fileId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { deleteFile: boolean } }>({
        mutation: DELETE_FILE,
        variables: { fileId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.deleteFile ?? false),
        catchError((err) => {
          console.error('Failed to delete file:', err);
          return of(false);
        }),
      );
  }
}
