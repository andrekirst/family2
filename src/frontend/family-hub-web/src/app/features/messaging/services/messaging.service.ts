import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Apollo } from 'apollo-angular';
import {
  GET_FAMILY_MESSAGES,
  SEND_MESSAGE,
  MESSAGE_SENT_SUBSCRIPTION,
} from '../graphql/messaging.operations';
import { Observable, catchError, map, of } from 'rxjs';
import { EnvironmentConfigService } from '../../../core/config/environment-config.service';

export interface AttachmentDto {
  fileId: string;
  fileName: string;
  mimeType: string;
  fileSize: number;
  attachedAt: string;
}

export interface MessageDto {
  id: string;
  familyId: string;
  senderId: string;
  senderName: string;
  senderAvatarId: string | null;
  content: string;
  sentAt: string;
  attachments: AttachmentDto[];
}

export interface AttachmentInput {
  fileId: string;
  fileName: string;
  mimeType: string;
  fileSize: number;
}

export interface SendMessageInput {
  content: string;
  attachments?: AttachmentInput[];
}

@Injectable({
  providedIn: 'root',
})
export class MessagingService {
  private apollo = inject(Apollo);
  private http = inject(HttpClient);
  private config = inject(EnvironmentConfigService);

  getMessages(limit = 50, before?: string) {
    return this.apollo
      .query<{ messaging: { messages: MessageDto[] } }>({
        query: GET_FAMILY_MESSAGES,
        variables: { limit, before: before ?? null },
        fetchPolicy: 'network-only',
      })
      .pipe(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        map((result: any) => [...(result.data?.messaging?.messages ?? [])].reverse()),
        catchError((error: unknown) => {
          console.error('Failed to fetch messages:', error);
          return of([]);
        }),
      );
  }

  sendMessage(input: SendMessageInput) {
    return this.apollo
      .mutate<{ messaging: { sendMessage: MessageDto } }>({
        mutation: SEND_MESSAGE,
        variables: { input },
      })
      .pipe(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        map((result: any) => result.data?.messaging?.sendMessage ?? null),
        catchError((error: unknown) => {
          console.error('Failed to send message:', error);
          return of(null);
        }),
      );
  }

  subscribeToMessages(familyId: string) {
    return this.apollo
      .subscribe<{ messageSent: MessageDto }>({
        query: MESSAGE_SENT_SUBSCRIPTION,
        variables: { familyId },
      })
      .pipe(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        map((result: any) => result.data?.messageSent ?? null),
        catchError((error: unknown) => {
          console.error('Message subscription error:', error);
          return of(null);
        }),
      );
  }

  uploadFile(file: File): Observable<AttachmentDto> {
    const formData = new FormData();
    formData.append('file', file);

    const apiUrl = this.config.apiBaseUrl;
    return this.http.post<AttachmentInput>(`${apiUrl}/api/messaging/mock-upload`, formData).pipe(
      map(
        (result: AttachmentInput): AttachmentDto => ({
          fileId: result.fileId,
          fileName: result.fileName,
          mimeType: result.mimeType,
          fileSize: result.fileSize,
          attachedAt: new Date().toISOString(),
        }),
      ),
    );
  }
}
