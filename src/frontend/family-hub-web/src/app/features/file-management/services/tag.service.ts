import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import { TagDto, CreateTagInput, UpdateTagInput } from '../models/tag.models';
import { StoredFileDto } from '../models/file.models';
import {
  GET_TAGS,
  GET_FILES_BY_TAG,
  CREATE_TAG,
  UPDATE_TAG,
  DELETE_TAG,
  TAG_FILE,
  UNTAG_FILE,
} from '../graphql/tag.operations';

@Injectable({ providedIn: 'root' })
export class TagService {
  private readonly apollo = inject(Apollo);

  getTags(): Observable<TagDto[]> {
    return this.apollo
      .query<{ fileManagement: { tags: TagDto[] } }>({
        query: GET_TAGS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.tags),
        catchError((err) => {
          console.error('Failed to load tags:', err);
          return of([]);
        }),
      );
  }

  getFilesByTag(tagId: string): Observable<StoredFileDto[]> {
    return this.apollo
      .query<{ fileManagement: { filesByTag: StoredFileDto[] } }>({
        query: GET_FILES_BY_TAG,
        variables: { tagIds: [tagId] },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.filesByTag),
        catchError((err) => {
          console.error('Failed to load files by tag:', err);
          return of([]);
        }),
      );
  }

  createTag(input: CreateTagInput): Observable<TagDto | null> {
    return this.apollo
      .mutate<{ fileManagement: { createTag: TagDto } }>({
        mutation: CREATE_TAG,
        variables: { input },
      })
      .pipe(
        map((r) => r.data?.fileManagement.createTag ?? null),
        catchError((err) => {
          console.error('Failed to create tag:', err);
          return of(null);
        }),
      );
  }

  updateTag(input: UpdateTagInput): Observable<TagDto | null> {
    return this.apollo
      .mutate<{ fileManagement: { updateTag: TagDto } }>({
        mutation: UPDATE_TAG,
        variables: { input },
      })
      .pipe(
        map((r) => r.data?.fileManagement.updateTag ?? null),
        catchError((err) => {
          console.error('Failed to update tag:', err);
          return of(null);
        }),
      );
  }

  deleteTag(tagId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { deleteTag: boolean } }>({
        mutation: DELETE_TAG,
        variables: { tagId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.deleteTag ?? false),
        catchError((err) => {
          console.error('Failed to delete tag:', err);
          return of(false);
        }),
      );
  }

  tagFile(fileId: string, tagId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { tagFile: boolean } }>({
        mutation: TAG_FILE,
        variables: { fileId, tagId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.tagFile ?? false),
        catchError((err) => {
          console.error('Failed to tag file:', err);
          return of(false);
        }),
      );
  }

  untagFile(fileId: string, tagId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { untagFile: boolean } }>({
        mutation: UNTAG_FILE,
        variables: { fileId, tagId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.untagFile ?? false),
        catchError((err) => {
          console.error('Failed to untag file:', err);
          return of(false);
        }),
      );
  }
}
