import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType } from '@angular/common/http';
import { Observable, map, filter } from 'rxjs';
import { EnvironmentConfigService } from '../../../core/config/environment-config.service';
import { UploadResult } from '../models/file.models';

const CHUNK_SIZE = 5 * 1024 * 1024; // 5 MB

export interface UploadProgress {
  percent: number;
  done: boolean;
  result?: UploadResult;
}

@Injectable({ providedIn: 'root' })
export class FileUploadService {
  private readonly http = inject(HttpClient);
  private readonly envConfig = inject(EnvironmentConfigService);

  private get baseUrl(): string {
    return `${this.envConfig.apiBaseUrl}/api/files`;
  }

  upload(file: File, folderId: string): Observable<UploadProgress> {
    if (file.size > CHUNK_SIZE) {
      return this.chunkedUpload(file, folderId);
    }
    return this.simpleUpload(file, folderId);
  }

  private simpleUpload(file: File, _folderId: string): Observable<UploadProgress> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http
      .post<UploadResult>(`${this.baseUrl}/upload`, formData, {
        reportProgress: true,
        observe: 'events',
      })
      .pipe(
        filter(
          (event: HttpEvent<UploadResult>) =>
            event.type === HttpEventType.UploadProgress || event.type === HttpEventType.Response,
        ),
        map((event: HttpEvent<UploadResult>) => {
          if (event.type === HttpEventType.UploadProgress) {
            return {
              percent: event.total ? Math.round((100 * event.loaded) / event.total) : 0,
              done: false,
            };
          }
          // Response
          return {
            percent: 100,
            done: true,
            result: (event as { body: UploadResult }).body,
          };
        }),
      );
  }

  private chunkedUpload(file: File, _folderId: string): Observable<UploadProgress> {
    return new Observable<UploadProgress>((subscriber) => {
      this.performChunkedUpload(file, subscriber);
    });
  }

  private async performChunkedUpload(
    file: File,
    subscriber: {
      next: (value: UploadProgress) => void;
      complete: () => void;
      error: (err: unknown) => void;
    },
  ): Promise<void> {
    try {
      // Initiate
      const { uploadId } = (await this.http
        .post<{ uploadId: string }>(`${this.baseUrl}/upload/initiate`, {})
        .toPromise()) as { uploadId: string };

      const totalChunks = Math.ceil(file.size / CHUNK_SIZE);

      // Upload chunks
      for (let i = 0; i < totalChunks; i++) {
        const start = i * CHUNK_SIZE;
        const end = Math.min(start + CHUNK_SIZE, file.size);
        const chunk = file.slice(start, end);

        const chunkForm = new FormData();
        chunkForm.append('file', chunk);

        await this.http
          .post(`${this.baseUrl}/upload/${uploadId}/chunk?chunkIndex=${i}`, chunkForm)
          .toPromise();

        subscriber.next({
          percent: Math.round(((i + 1) / totalChunks) * 95),
          done: false,
        });
      }

      // Complete
      const result = await this.http
        .post<UploadResult>(`${this.baseUrl}/upload/${uploadId}/complete`, {
          fileName: file.name,
        })
        .toPromise();

      subscriber.next({ percent: 100, done: true, result: result! });
      subscriber.complete();
    } catch (err) {
      subscriber.error(err);
    }
  }
}
