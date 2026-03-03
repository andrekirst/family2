import { Component, OnInit, OnDestroy, inject, signal, computed, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { MessagingService, MessageDto } from '../../services/messaging.service';
import { UserService } from '../../../../core/user/user.service';
import { MessageListComponent } from '../message-list/message-list.component';
import {
  MessageInputComponent,
  MessageSendPayload,
} from '../message-input/message-input.component';
import { MessageViewModel } from '../message-item/message-item.component';

@Component({
  selector: 'app-messaging-page',
  standalone: true,
  imports: [CommonModule, MessageListComponent, MessageInputComponent],
  template: `
    <div class="flex flex-col flex-1 min-h-0 bg-white" data-testid="messaging-page">
      <!-- Header -->
      <div class="flex items-center border-b border-gray-200 px-4 py-3 flex-shrink-0">
        <h1 class="text-lg font-semibold text-gray-900" i18n="@@messaging.title">Messages</h1>
      </div>

      @if (!familyId()) {
        <div class="flex flex-col items-center justify-center flex-1 text-gray-400">
          <p class="text-sm" i18n="@@messaging.noFamily">Join a family to start messaging.</p>
        </div>
      } @else {
        <div
          class="relative flex flex-col flex-1 min-h-0"
          (dragover)="onDragOver($event)"
          (dragleave)="onDragLeave($event)"
          (drop)="onDrop($event)"
        >
          <!-- Drop overlay -->
          @if (isDragging()) {
            <div
              class="absolute inset-0 z-10 flex items-center justify-center bg-blue-50/80 border-2 border-dashed border-blue-400 rounded-lg pointer-events-none"
            >
              <span class="text-blue-600 font-medium text-sm" i18n="@@messaging.dropFiles"
                >Drop files to attach</span
              >
            </div>
          }

          <!-- Message list -->
          <app-message-list
            #messageList
            [messages]="messageViewModels()"
            [isLoadingOlder]="isLoadingOlder()"
            (loadOlder)="loadOlderMessages()"
          />

          <!-- Message input -->
          <app-message-input #messageInput (messageSend)="onSendMessage($event)" />
        </div>
      }
    </div>
  `,
  styles: [
    `
      :host {
        display: flex;
        flex-direction: column;
        flex: 1;
        min-height: 0;
        overflow: hidden;
      }
    `,
  ],
})
export class MessagingPageComponent implements OnInit, OnDestroy {
  private readonly messagingService = inject(MessagingService);
  private readonly userService = inject(UserService);

  private subscription: Subscription | null = null;
  private readonly rawMessages = signal<MessageDto[]>([]);

  @ViewChild('messageList') private messageList!: MessageListComponent;
  @ViewChild('messageInput') private messageInput!: MessageInputComponent;

  readonly isDragging = signal(false);
  readonly isLoading = signal(false);
  readonly isLoadingOlder = signal(false);
  readonly hasMoreMessages = signal(true);

  readonly familyId = computed(() => this.userService.currentUser()?.familyId ?? null);
  readonly currentUserId = computed(() => this.userService.currentUser()?.id ?? null);

  readonly messageViewModels = computed<MessageViewModel[]>(() =>
    this.rawMessages().map((m) => ({
      id: m.id,
      senderId: m.senderId,
      senderName: m.senderName,
      senderAvatarId: m.senderAvatarId,
      content: m.content,
      sentAt: m.sentAt,
      attachments: m.attachments ?? [],
    })),
  );

  ngOnInit(): void {
    if (this.familyId()) {
      this.loadMessages();
      this.startSubscription();
    }
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  private loadMessages(): void {
    this.isLoading.set(true);
    this.messagingService.getMessages(50).subscribe({
      next: (messages) => {
        this.rawMessages.set(messages);
        this.hasMoreMessages.set(messages.length >= 50);
        this.isLoading.set(false);

        // Scroll to bottom after initial load
        setTimeout(() => this.messageList?.scrollToBottom(), 0);
      },
      error: () => this.isLoading.set(false),
    });
  }

  loadOlderMessages(): void {
    if (this.isLoadingOlder() || !this.hasMoreMessages()) return;

    const messages = this.rawMessages();
    if (messages.length === 0) return;

    const oldestSentAt = messages[0].sentAt;
    this.isLoadingOlder.set(true);

    this.messagingService.getMessages(50, oldestSentAt).subscribe({
      next: (olderMessages) => {
        if (olderMessages.length > 0) {
          this.rawMessages.update((current) => [...olderMessages, ...current]);
        }
        this.hasMoreMessages.set(olderMessages.length >= 50);
        this.isLoadingOlder.set(false);
      },
      error: () => this.isLoadingOlder.set(false),
    });
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
    if (event.dataTransfer?.files?.length) {
      this.messageInput.uploadFiles(Array.from(event.dataTransfer.files));
    }
  }

  onSendMessage(payload: MessageSendPayload): void {
    const input = {
      content: payload.content,
      ...(payload.attachments.length > 0 ? { attachments: payload.attachments } : {}),
    };
    this.messagingService.sendMessage(input).subscribe({
      next: (message) => {
        if (message) {
          // Avoid duplicates if subscription already delivered it
          this.rawMessages.update((current) =>
            current.some((m) => m.id === message.id) ? current : [...current, message],
          );
        }
      },
    });
  }

  private startSubscription(): void {
    const fid = this.familyId();
    if (!fid) return;

    this.subscription = this.messagingService.subscribeToMessages(fid).subscribe({
      next: (message) => {
        if (message) {
          this.rawMessages.update((current) =>
            current.some((m) => m.id === message.id) ? current : [...current, message],
          );
        }
      },
    });
  }
}
