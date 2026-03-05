import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConversationDto } from '../../services/messaging.service';

@Component({
  selector: 'app-conversation-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex flex-col h-full bg-gray-50 border-r border-gray-200">
      <!-- Header -->
      <div class="flex items-center justify-between px-3 py-2 border-b border-gray-200">
        <h2 class="text-sm font-semibold text-gray-700" i18n="@@messaging.conversations">
          Conversations
        </h2>
        <button
          class="p-1 text-gray-400 hover:text-gray-600 rounded"
          (click)="createConversation.emit()"
          title="New conversation"
          i18n-title="@@messaging.newConversation"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 4v16m8-8H4"
            />
          </svg>
        </button>
      </div>

      <!-- Conversation list -->
      <div class="flex-1 overflow-y-auto">
        @for (conv of conversations(); track conv.id) {
          <button
            class="w-full text-left px-3 py-2.5 hover:bg-gray-100 transition-colors border-b border-gray-100"
            [class.bg-blue-50]="conv.id === selectedId()"
            [class.border-l-2]="conv.id === selectedId()"
            [class.border-l-blue-500]="conv.id === selectedId()"
            (click)="conversationSelected.emit(conv)"
            data-testid="conversation-item"
          >
            <div class="flex items-center gap-2">
              <!-- Type icon -->
              <span class="text-gray-400 flex-shrink-0">
                @switch (conv.type) {
                  @case ('Family') {
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"
                      />
                    </svg>
                  }
                  @case ('Direct') {
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                      />
                    </svg>
                  }
                  @default {
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"
                      />
                    </svg>
                  }
                }
              </span>

              <!-- Name -->
              <span
                class="text-sm truncate"
                [class.font-medium]="conv.id === selectedId()"
                [class.text-gray-900]="conv.id === selectedId()"
                [class.text-gray-700]="conv.id !== selectedId()"
              >
                {{ conv.name }}
              </span>
            </div>
          </button>
        } @empty {
          <div class="p-3 text-xs text-gray-400 text-center" i18n="@@messaging.noConversations">
            No conversations yet
          </div>
        }
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: flex;
        flex-direction: column;
        height: 100%;
      }
    `,
  ],
})
export class ConversationListComponent {
  readonly conversations = input<ConversationDto[]>([]);
  readonly selectedId = input<string | null>(null);
  readonly conversationSelected = output<ConversationDto>();
  readonly createConversation = output<void>();
}
