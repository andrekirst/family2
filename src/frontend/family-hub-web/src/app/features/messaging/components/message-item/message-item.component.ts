import { Component, computed, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AvatarDisplayComponent } from '../../../../core/avatar';

export interface MessageViewModel {
  id: string;
  senderId: string;
  senderName: string;
  senderAvatarId: string | null;
  content: string;
  sentAt: string;
}

@Component({
  selector: 'app-message-item',
  standalone: true,
  imports: [CommonModule, AvatarDisplayComponent],
  template: `
    <div class="flex gap-3 px-4 py-2 hover:bg-gray-50 transition-colors" data-testid="message-item">
      <app-avatar-display
        [avatarId]="message().senderAvatarId"
        [name]="message().senderName"
        size="tiny"
        class="flex-shrink-0 mt-0.5"
      />
      <div class="min-w-0 flex-1">
        <div class="flex items-baseline gap-2">
          <span class="text-sm font-semibold text-gray-900 truncate" data-testid="message-sender">
            {{ message().senderName }}
          </span>
          <span class="text-xs text-gray-400 flex-shrink-0" data-testid="message-time">
            {{ formattedTime() }}
          </span>
        </div>
        <p
          class="text-sm text-gray-700 whitespace-pre-wrap break-words"
          data-testid="message-content"
        >
          {{ message().content }}
        </p>
      </div>
    </div>
  `,
})
export class MessageItemComponent {
  message = input.required<MessageViewModel>();

  formattedTime = computed(() => {
    const date = new Date(this.message().sentAt);
    const now = new Date();
    const isToday = date.toDateString() === now.toDateString();

    const time = date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    if (isToday) return time;

    return `${date.toLocaleDateString([], { month: 'short', day: 'numeric' })} ${time}`;
  });
}
