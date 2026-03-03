import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import {
  GET_FAMILY_MESSAGES,
  SEND_MESSAGE,
  MESSAGE_SENT_SUBSCRIPTION,
} from '../graphql/messaging.operations';
import { catchError, map, of } from 'rxjs';

export interface MessageDto {
  id: string;
  familyId: string;
  senderId: string;
  senderName: string;
  senderAvatarId: string | null;
  content: string;
  sentAt: string;
}

export interface SendMessageInput {
  content: string;
}

@Injectable({
  providedIn: 'root',
})
export class MessagingService {
  private apollo = inject(Apollo);

  getMessages(limit = 50, before?: string) {
    return this.apollo
      .query<{ messaging: { messages: MessageDto[] } }>({
        query: GET_FAMILY_MESSAGES,
        variables: { limit, before: before ?? null },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.messaging?.messages ?? []),
        catchError((error) => {
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
        map((result) => result.data?.messaging?.sendMessage ?? null),
        catchError((error) => {
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
        map((result) => result.data?.messageSent ?? null),
        catchError((error) => {
          console.error('Message subscription error:', error);
          return of(null);
        }),
      );
  }
}
