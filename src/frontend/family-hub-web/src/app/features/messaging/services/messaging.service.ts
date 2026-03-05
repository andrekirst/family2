import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Apollo } from 'apollo-angular';
import {
  GET_FAMILY_MESSAGES,
  SEND_MESSAGE,
  MESSAGE_SENT_SUBSCRIPTION,
  GET_CONVERSATIONS,
  CREATE_CONVERSATION,
  GET_CONVERSATION_MESSAGES,
} from '../graphql/messaging.operations';
import { Observable, catchError, map, of } from 'rxjs';
import { EnvironmentConfigService } from '../../../core/config/environment-config.service';

export interface AttachmentDto {
  fileId: string;
  fileName: string;
  mimeType: string;
  fileSize: number;
  storageKey: string | null;
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
  conversationId: string | null;
  attachments: AttachmentDto[];
}

export interface AttachmentInput {
  storageKey: string;
  fileName: string;
  mimeType: string;
  fileSize: number;
  checksum: string;
}

export interface UploadResponse {
  storageKey: string;
  mimeType: string;
  size: number;
  checksum: string;
}

export interface SendMessageInput {
  content: string;
  attachments?: AttachmentInput[];
  conversationId?: string;
}

export interface ConversationMemberDto {
  id: string;
  userId: string;
  role: string;
  joinedAt: string;
  leftAt: string | null;
}

export interface ConversationDto {
  id: string;
  name: string;
  type: string;
  familyId: string;
  createdBy: string;
  folderId: string | null;
  createdAt: string;
  members: ConversationMemberDto[];
}

export interface CreateConversationInput {
  name: string;
  type: string;
  memberIds: string[];
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

  getConversationMessages(
    conversationId: string,
    limit = 50,
    before?: string,
  ): Observable<MessageDto[]> {
    return this.apollo
      .query<{ messaging: { conversationMessages: MessageDto[] } }>({
        query: GET_CONVERSATION_MESSAGES,
        variables: { conversationId, limit, before: before ?? null },
        fetchPolicy: 'network-only',
      })
      .pipe(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        map((result: any) => [...(result.data?.messaging?.conversationMessages ?? [])].reverse()),
        catchError((error: unknown) => {
          console.error('Failed to fetch conversation messages:', error);
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

  getConversations(): Observable<ConversationDto[]> {
    return this.apollo
      .query<{ messaging: { conversations: ConversationDto[] } }>({
        query: GET_CONVERSATIONS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        map((result: any) => result.data?.messaging?.conversations ?? []),
        catchError((error: unknown) => {
          console.error('Failed to fetch conversations:', error);
          return of([]);
        }),
      );
  }

  createConversation(input: CreateConversationInput): Observable<ConversationDto | null> {
    return this.apollo
      .mutate<{ messaging: { createConversation: ConversationDto } }>({
        mutation: CREATE_CONVERSATION,
        variables: { input },
      })
      .pipe(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        map((result: any) => result.data?.messaging?.createConversation ?? null),
        catchError((error: unknown) => {
          console.error('Failed to create conversation:', error);
          return of(null);
        }),
      );
  }

  uploadFile(file: File): Observable<UploadResponse & { fileName: string }> {
    const formData = new FormData();
    formData.append('file', file);

    const apiUrl = this.config.apiBaseUrl;
    return this.http.post<UploadResponse>(`${apiUrl}/api/files/upload`, formData).pipe(
      map((result: UploadResponse) => ({
        ...result,
        fileName: file.name,
      })),
    );
  }
}
