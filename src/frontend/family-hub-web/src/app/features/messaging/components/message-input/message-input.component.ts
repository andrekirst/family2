import { Component, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-message-input',
  standalone: true,
  imports: [FormsModule],
  styles: [
    `
      :host {
        display: block;
        flex-shrink: 0;
      }
    `,
  ],
  template: `
    <div
      class="flex-shrink-0 border-t border-gray-200 bg-white px-3 py-[13px]"
      data-testid="message-input"
    >
      <div class="flex items-end gap-2">
        <textarea
          class="flex-1 resize-none rounded-lg border border-gray-300 px-3 py-2.5 text-sm text-gray-900 placeholder-gray-400 focus:border-blue-500 focus:ring-1 focus:ring-blue-500 outline-none transition-colors"
          [rows]="1"
          [maxLength]="4000"
          placeholder="Type a message..."
          i18n-placeholder="@@messaging.inputPlaceholder"
          [(ngModel)]="content"
          (input)="updateCanSend()"
          (keydown)="onKeyDown($event)"
          data-testid="message-textarea"
        ></textarea>
        <button
          (click)="send()"
          [disabled]="!canSend()"
          class="flex-shrink-0 rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          data-testid="send-button"
        >
          <span i18n="@@messaging.send">Send</span>
        </button>
      </div>
    </div>
  `,
})
export class MessageInputComponent {
  readonly messageSend = output<string>();

  content = '';

  canSend = signal(false);

  updateCanSend(): void {
    this.canSend.set(this.content.trim().length > 0);
  }

  onKeyDown(event: KeyboardEvent): void {
    this.updateCanSend();

    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  send(): void {
    const trimmed = this.content.trim();
    if (!trimmed) return;

    this.messageSend.emit(trimmed);
    this.content = '';
    this.canSend.set(false);
  }
}
