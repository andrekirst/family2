import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, forkJoin, map, of } from 'rxjs';
import { EnvironmentConfigService } from '../../../core/config/environment-config.service';
import { MOVE_FILE, DELETE_FILE } from '../graphql/batch.operations';

@Injectable({ providedIn: 'root' })
export class BatchService {
  private readonly apollo = inject(Apollo);
  private readonly http = inject(HttpClient);
  private readonly config = inject(EnvironmentConfigService);

  batchMove(fileIds: string[], targetFolderId: string): Observable<boolean[]> {
    const ops = fileIds.map((fileId) =>
      this.apollo
        .mutate<{ fileManagement: { moveFile: { id: string } } }>({
          mutation: MOVE_FILE,
          variables: { input: { fileId, targetFolderId } },
        })
        .pipe(
          map((r) => !!r.data?.fileManagement.moveFile.id),
          catchError(() => of(false)),
        ),
    );
    return forkJoin(ops);
  }

  batchDelete(fileIds: string[]): Observable<boolean[]> {
    const ops = fileIds.map((fileId) =>
      this.apollo
        .mutate<{ fileManagement: { deleteFile: boolean } }>({
          mutation: DELETE_FILE,
          variables: { fileId },
        })
        .pipe(
          map((r) => r.data?.fileManagement.deleteFile ?? false),
          catchError(() => of(false)),
        ),
    );
    return forkJoin(ops);
  }

  downloadAsZip(fileIds: string[]): void {
    const url = `${this.config.apiBaseUrl}/files/batch-download`;
    this.http
      .post(url, { fileIds }, { responseType: 'blob' })
      .pipe(
        catchError((err) => {
          console.error('Failed to download ZIP:', err);
          return of(null);
        }),
      )
      .subscribe((blob) => {
        if (blob) {
          const a = document.createElement('a');
          a.href = URL.createObjectURL(blob);
          a.download = 'files.zip';
          a.click();
          URL.revokeObjectURL(a.href);
        }
      });
  }
}
