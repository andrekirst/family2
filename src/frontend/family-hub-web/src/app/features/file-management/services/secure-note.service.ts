import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import {
  SecureNoteDto,
  CreateSecureNoteInput,
  UpdateSecureNoteInput,
} from '../models/secure-note.models';
import {
  GET_SECURE_NOTES,
  CREATE_SECURE_NOTE,
  UPDATE_SECURE_NOTE,
  DELETE_SECURE_NOTE,
} from '../graphql/secure-note.operations';

@Injectable({ providedIn: 'root' })
export class SecureNoteService {
  private readonly apollo = inject(Apollo);

  getNotes(category?: string): Observable<SecureNoteDto[]> {
    return this.apollo
      .query<{ fileManagement: { secureNotes: SecureNoteDto[] } }>({
        query: GET_SECURE_NOTES,
        variables: { category: category ?? null },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.secureNotes),
        catchError((err) => {
          console.error('Failed to load secure notes:', err);
          return of([]);
        }),
      );
  }

  createNote(input: CreateSecureNoteInput): Observable<string | null> {
    return this.apollo
      .mutate<{ fileManagement: { createSecureNote: { noteId: string } } }>({
        mutation: CREATE_SECURE_NOTE,
        variables: input,
      })
      .pipe(
        map((r) => r.data?.fileManagement.createSecureNote.noteId ?? null),
        catchError((err) => {
          console.error('Failed to create secure note:', err);
          return of(null);
        }),
      );
  }

  updateNote(input: UpdateSecureNoteInput): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { updateSecureNote: { success: boolean } } }>({
        mutation: UPDATE_SECURE_NOTE,
        variables: input,
      })
      .pipe(
        map((r) => r.data?.fileManagement.updateSecureNote.success ?? false),
        catchError((err) => {
          console.error('Failed to update secure note:', err);
          return of(false);
        }),
      );
  }

  deleteNote(noteId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { deleteSecureNote: { success: boolean } } }>({
        mutation: DELETE_SECURE_NOTE,
        variables: { noteId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.deleteSecureNote.success ?? false),
        catchError((err) => {
          console.error('Failed to delete secure note:', err);
          return of(false);
        }),
      );
  }
}
