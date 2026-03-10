import { Component, ElementRef, ViewChild, AfterViewChecked, input, output, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MessageItemComponent, MessageViewModel } from '../message-item/message-item.component';

@Component({
  selector: 'app-message-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MessageItemComponent],
  styles: [
    `
      :host {
        display: flex;
        flex-direction: column;
        flex: 1;
        min-height: 0;
      }
    `,
  ],
  template: `
    <div
      #scrollContainer
      class="flex-1 overflow-y-auto"
      (scroll)="onScroll()"
      data-testid="message-list"
    >
      @if (isLoadingOlder()) {
        <div class="flex justify-center py-3">
          <div
            class="h-5 w-5 animate-spin rounded-full border-2 border-gray-300 border-t-blue-600"
          ></div>
        </div>
      }
      @if (messages().length === 0 && !isLoadingOlder()) {
        <div class="flex flex-col items-center justify-center h-full text-gray-400">
          <p class="text-sm" i18n="@@messaging.noMessages">
            No messages yet. Start the conversation!
          </p>
        </div>
      }
      @for (msg of messages(); track msg.id) {
        <app-message-item [message]="msg" />
      }
    </div>
  `,
})
export class MessageListComponent implements AfterViewChecked {
  messages = input.required<MessageViewModel[]>();
  isLoadingOlder = input(false);
  readonly loadOlder = output<void>();

  @ViewChild('scrollContainer') private scrollContainer!: ElementRef<HTMLDivElement>;

  private shouldScrollToBottom = true;
  private previousMessageCount = 0;

  ngAfterViewChecked(): void {
    const currentCount = this.messages().length;
    if (currentCount > this.previousMessageCount && this.shouldScrollToBottom) {
      this.scrollToBottom();
    }
    this.previousMessageCount = currentCount;
  }

  scrollToBottom(): void {
    const el = this.scrollContainer?.nativeElement;
    if (el) {
      el.scrollTop = el.scrollHeight;
    }
  }

  onScroll(): void {
    const el = this.scrollContainer?.nativeElement;
    if (!el) return;

    // Check if scrolled to top → load older messages
    if (el.scrollTop === 0 && this.messages().length > 0) {
      this.loadOlder.emit();
    }

    // Track if user is near the bottom (auto-scroll on new messages)
    const threshold = 100;
    this.shouldScrollToBottom = el.scrollHeight - el.scrollTop - el.clientHeight < threshold;
  }
}
