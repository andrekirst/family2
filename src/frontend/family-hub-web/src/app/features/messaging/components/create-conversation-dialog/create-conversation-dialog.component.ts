import { Component, inject, output, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CreateConversationInput, MessagingService } from '../../services/messaging.service';
import { InvitationService } from '../../../family/services/invitation.service';

@Component({
  selector: 'app-create-conversation-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule],
  template: `
    @if (isOpen()) {
      <div
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
        (click)="close()"
      >
        <div
          class="bg-white rounded-lg shadow-xl w-full max-w-md mx-4 p-5"
          (click)="$event.stopPropagation()"
        >
          <h2
            class="text-lg font-semibold text-gray-900 mb-4"
            i18n="@@messaging.createConversation"
          >
            New Conversation
          </h2>

          <!-- Type selector -->
          <div class="mb-4">
            <label
              class="block text-sm font-medium text-gray-700 mb-1"
              i18n="@@messaging.conversationType"
            >
              Type
            </label>
            <select
              class="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              [(ngModel)]="selectedType"
            >
              <option value="Direct" i18n="@@messaging.typeDirect">Direct Message</option>
              <option value="Group" i18n="@@messaging.typeGroup">Group</option>
            </select>
          </div>

          <!-- Name (only for Group) -->
          @if (selectedType === 'Group') {
            <div class="mb-4">
              <label
                class="block text-sm font-medium text-gray-700 mb-1"
                i18n="@@messaging.conversationName"
              >
                Name
              </label>
              <input
                type="text"
                class="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                [(ngModel)]="name"
                maxlength="255"
                placeholder="e.g. Trip Planning"
                i18n-placeholder="@@messaging.conversationNamePlaceholder"
              />
            </div>
          }

          <!-- Member picker -->
          <div class="mb-4">
            <label
              class="block text-sm font-medium text-gray-700 mb-1"
              i18n="@@messaging.selectMembers"
            >
              Members
            </label>
            <div class="max-h-40 overflow-y-auto border border-gray-200 rounded-md">
              @for (member of familyMembers(); track member.id) {
                <label
                  class="flex items-center gap-2 px-3 py-2 hover:bg-gray-50 cursor-pointer text-sm"
                >
                  <input
                    type="checkbox"
                    [checked]="selectedMemberIds.has(member.id)"
                    (change)="toggleMember(member.id)"
                    class="rounded border-gray-300 text-blue-600"
                  />
                  <span class="text-gray-700">{{ member.displayName }}</span>
                </label>
              } @empty {
                <div class="px-3 py-2 text-sm text-gray-400" i18n="@@messaging.noFamilyMembers">
                  No family members found
                </div>
              }
            </div>
          </div>

          <!-- Actions -->
          <div class="flex justify-end gap-2">
            <button
              class="px-4 py-2 text-sm text-gray-600 hover:text-gray-800"
              (click)="close()"
              i18n="@@common.cancel"
            >
              Cancel
            </button>
            <button
              class="px-4 py-2 text-sm bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
              [disabled]="!canCreate()"
              (click)="create()"
              i18n="@@messaging.create"
            >
              Create
            </button>
          </div>
        </div>
      </div>
    }
  `,
})
export class CreateConversationDialogComponent {
  private messagingService = inject(MessagingService);
  private invitationService = inject(InvitationService);

  readonly isOpen = signal(false);
  readonly familyMembers = signal<{ id: string; displayName: string }[]>([]);
  readonly created = output<void>();

  selectedType = 'Direct';
  name = '';
  selectedMemberIds = new Set<string>();

  open(): void {
    this.isOpen.set(true);
    this.selectedType = 'Direct';
    this.name = '';
    this.selectedMemberIds.clear();
    this.loadFamilyMembers();
  }

  close(): void {
    this.isOpen.set(false);
  }

  toggleMember(id: string): void {
    if (this.selectedMemberIds.has(id)) {
      this.selectedMemberIds.delete(id);
    } else {
      this.selectedMemberIds.add(id);
    }
  }

  canCreate(): boolean {
    const memberCount = this.selectedMemberIds.size;
    if (this.selectedType === 'Direct') {
      return memberCount === 1; // +1 for current user = 2 total
    }
    return memberCount >= 1 && this.name.trim().length > 0;
  }

  create(): void {
    if (!this.canCreate()) return;

    const autoName =
      this.selectedType === 'Direct' ? this.getDirectConversationName() : this.name.trim();

    const input: CreateConversationInput = {
      name: autoName,
      type: this.selectedType,
      memberIds: Array.from(this.selectedMemberIds),
    };

    this.messagingService.createConversation(input).subscribe({
      next: (result) => {
        if (result) {
          this.close();
          this.created.emit();
        }
      },
    });
  }

  private getDirectConversationName(): string {
    const selectedMembers = this.familyMembers().filter((m) => this.selectedMemberIds.has(m.id));
    return selectedMembers.map((m) => m.displayName).join(' & ') || 'Direct Message';
  }

  private loadFamilyMembers(): void {
    this.invitationService.getFamilyMembers().subscribe({
      next: (members) => {
        this.familyMembers.set(
          members.map((m) => ({
            id: m.userId,
            displayName: m.userName || m.userEmail || 'Unknown',
          })),
        );
      },
    });
  }
}
