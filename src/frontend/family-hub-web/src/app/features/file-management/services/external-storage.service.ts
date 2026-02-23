import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import {
  ExternalConnectionDto,
  ConnectExternalStorageInput,
} from '../models/external-storage.models';
import {
  GET_EXTERNAL_CONNECTIONS,
  CONNECT_EXTERNAL_STORAGE,
  DISCONNECT_EXTERNAL_STORAGE,
} from '../graphql/external-storage.operations';

@Injectable({ providedIn: 'root' })
export class ExternalStorageService {
  private readonly apollo = inject(Apollo);

  getConnections(): Observable<ExternalConnectionDto[]> {
    return this.apollo
      .query<{ fileManagement: { getExternalConnections: ExternalConnectionDto[] } }>({
        query: GET_EXTERNAL_CONNECTIONS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.getExternalConnections),
        catchError((err) => {
          console.error('Failed to load external connections:', err);
          return of([]);
        }),
      );
  }

  connect(input: ConnectExternalStorageInput): Observable<string | null> {
    return this.apollo
      .mutate<{ fileManagement: { connectExternalStorage: { connectionId: string } } }>({
        mutation: CONNECT_EXTERNAL_STORAGE,
        variables: input,
      })
      .pipe(
        map((r) => r.data?.fileManagement.connectExternalStorage.connectionId ?? null),
        catchError((err) => {
          console.error('Failed to connect external storage:', err);
          return of(null);
        }),
      );
  }

  disconnect(connectionId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { disconnectExternalStorage: { success: boolean } } }>({
        mutation: DISCONNECT_EXTERNAL_STORAGE,
        variables: { connectionId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.disconnectExternalStorage.success ?? false),
        catchError((err) => {
          console.error('Failed to disconnect external storage:', err);
          return of(false);
        }),
      );
  }
}
