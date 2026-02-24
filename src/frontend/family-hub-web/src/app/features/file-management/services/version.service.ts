import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import { FileVersionDto } from '../models/version.models';
import { GET_FILE_VERSIONS, RESTORE_FILE_VERSION } from '../graphql/version.operations';

@Injectable({ providedIn: 'root' })
export class VersionService {
  private readonly apollo = inject(Apollo);

  getFileVersions(fileId: string, familyId: string): Observable<FileVersionDto[]> {
    return this.apollo
      .query<{ fileManagement: { fileVersions: FileVersionDto[] } }>({
        query: GET_FILE_VERSIONS,
        variables: { fileId, familyId },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.fileVersions),
        catchError((err) => {
          console.error('Failed to load file versions:', err);
          return of([]);
        }),
      );
  }

  restoreVersion(
    versionId: string,
    fileId: string,
  ): Observable<{ newVersionId: string; newVersionNumber: number } | null> {
    return this.apollo
      .mutate<{
        fileManagement: {
          restoreFileVersion: {
            success: boolean;
            newVersionId: string;
            newVersionNumber: number;
          };
        };
      }>({
        mutation: RESTORE_FILE_VERSION,
        variables: { versionId, fileId },
      })
      .pipe(
        map((r) => {
          const result = r.data?.fileManagement.restoreFileVersion;
          return result?.success
            ? { newVersionId: result.newVersionId, newVersionNumber: result.newVersionNumber }
            : null;
        }),
        catchError((err) => {
          console.error('Failed to restore version:', err);
          return of(null);
        }),
      );
  }
}
