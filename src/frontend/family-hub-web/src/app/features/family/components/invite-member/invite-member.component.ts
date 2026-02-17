import { Component, EventEmitter, Output, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InvitationService } from '../../services/invitation.service';

@Component({
  selector: 'app-invite-member',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="dialog-overlay" (click)="onDismiss()">
      <div class="dialog-content" (click)="$event.stopPropagation()">
        <div class="px-6 py-4 border-b border-gray-200 flex justify-between items-center">
          <h3 class="text-lg font-semibold text-gray-900" i18n="@@family.invite.title">
            Invite Family Member
          </h3>
          <button
            (click)="onDismiss()"
            class="text-gray-400 hover:text-gray-600"
            aria-label="Close"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </div>

        <div class="p-6">
          <form (ngSubmit)="onSubmit()">
            <div class="space-y-4">
              <div>
                <label
                  for="invite-email"
                  class="block text-sm font-medium text-gray-700 mb-1"
                  i18n="@@family.invite.email"
                  >Email Address</label
                >
                <input
                  id="invite-email"
                  type="email"
                  data-testid="invite-email-input"
                  [(ngModel)]="email"
                  [disabled]="isLoading()"
                  name="email"
                  i18n-placeholder="@@family.invite.emailPlaceholder"
                  placeholder="member@example.com"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              <div>
                <label
                  for="invite-role"
                  class="block text-sm font-medium text-gray-700 mb-1"
                  i18n="@@family.invite.role"
                  >Role</label
                >
                <select
                  id="invite-role"
                  [(ngModel)]="role"
                  [disabled]="isLoading()"
                  name="role"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
                >
                  <option value="Member" i18n="@@family.invite.roleMember">Member</option>
                  <option value="Admin" i18n="@@family.invite.roleAdmin">Admin</option>
                </select>
              </div>
            </div>

            @if (errorMessage()) {
              <div class="mt-3 text-sm text-red-600" role="alert">
                {{ errorMessage() }}
              </div>
            }

            <div class="mt-6 flex justify-end gap-3">
              <button
                type="button"
                (click)="onDismiss()"
                [disabled]="isLoading()"
                class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
                i18n="@@family.invite.cancel"
              >
                Cancel
              </button>
              <button
                type="submit"
                data-testid="send-invitation-button"
                [disabled]="isLoading()"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
              >
                {{ isLoading() ? sendingLabel : sendInvitationLabel }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .dialog-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 50;
      }
      .dialog-content {
        background: white;
        border-radius: 0.5rem;
        max-width: 28rem;
        width: 90%;
        box-shadow:
          0 20px 25px -5px rgba(0, 0, 0, 0.1),
          0 10px 10px -5px rgba(0, 0, 0, 0.04);
      }
    `,
  ],
})
export class InviteMemberComponent {
  private invitationService = inject(InvitationService);

  @Output() invitationSent = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  readonly sendingLabel = $localize`:@@family.invite.sending:Sending...`;
  readonly sendInvitationLabel = $localize`:@@family.invite.send:Send Invitation`;

  email = signal('');
  role = signal('Member');
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  onSubmit() {
    if (!this.email().trim()) {
      this.errorMessage.set($localize`:@@family.invite.emailRequired:Email address is required`);
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.invitationService
      .sendInvitation({
        email: this.email().trim(),
        role: this.role(),
      })
      .subscribe({
        next: (invitation) => {
          if (invitation) {
            this.invitationSent.emit();
          } else {
            this.errorMessage.set($localize`:@@family.invite.sendFailed:Failed to send invitation`);
          }
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set(
            $localize`:@@family.invite.sendError:An error occurred while sending the invitation`,
          );
          this.isLoading.set(false);
        },
      });
  }

  onDismiss() {
    this.dialogClosed.emit();
  }
}
