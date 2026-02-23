import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import { MediaStreamInfoDto } from '../models/media.models';
import { GET_MEDIA_STREAM_INFO, GENERATE_THUMBNAILS } from '../graphql/media.operations';
import { FileDownloadService } from './file-download.service';

@Injectable({ providedIn: 'root' })
export class MediaService {
  private readonly apollo = inject(Apollo);
  private readonly downloadService = inject(FileDownloadService);

  getMediaStreamInfo(fileId: string, familyId: string): Observable<MediaStreamInfoDto | null> {
    return this.apollo
      .query<{ fileManagement: { getMediaStreamInfo: MediaStreamInfoDto } }>({
        query: GET_MEDIA_STREAM_INFO,
        variables: { fileId, familyId },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data!.fileManagement.getMediaStreamInfo),
        catchError((err) => {
          console.error('Failed to load media info:', err);
          return of(null);
        }),
      );
  }

  generateThumbnails(fileId: string, familyId: string): Observable<number> {
    return this.apollo
      .mutate<{
        fileManagement: { generateThumbnails: { success: boolean; thumbnailsGenerated: number } };
      }>({
        mutation: GENERATE_THUMBNAILS,
        variables: { fileId, familyId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.generateThumbnails.thumbnailsGenerated ?? 0),
        catchError((err) => {
          console.error('Failed to generate thumbnails:', err);
          return of(0);
        }),
      );
  }

  getThumbnailUrl(storageKey: string): string {
    return this.downloadService.getStreamUrl(storageKey);
  }

  getStreamUrl(storageKey: string): string {
    return this.downloadService.getStreamUrl(storageKey);
  }
}
