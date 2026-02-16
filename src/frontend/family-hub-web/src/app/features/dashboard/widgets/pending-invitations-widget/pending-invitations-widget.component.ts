import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardWidgetComponent } from '../../../../core/dashboard/dashboard-widget.interface';
import { InvitationService } from '../../../family/services/invitation.service';
import { UserService } from '../../../../core/user/user.service';

interface PendingInvitation {
  id: string;
  familyName: string;
  invitedByName: string;
  role: string;
}

@Component({
  selector: 'app-pending-invitations-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (isLoading()) {
      <div class="animate-pulse space-y-2">
        <div class="h-4 bg-gray-200 rounded w-2/3"></div>
        <div class="h-4 bg-gray-200 rounded w-1/2"></div>
      </div>
    } @else if (invitations().length > 0) {
      <div class="space-y-2">
        @for (inv of invitations(); track inv.id) {
          <div class="p-2 bg-blue-50 rounded-md">
            <p class="text-sm font-medium text-gray-900">{{ inv.familyName }}</p>
            <p class="text-xs text-gray-500">Invited by {{ inv.invitedByName }}</p>
            <div class="mt-1 flex gap-1">
              <button
                (click)="accept(inv.id)"
                [disabled]="processing() === inv.id"
                class="px-2 py-0.5 text-xs font-medium text-white bg-blue-600 rounded hover:bg-blue-700 disabled:opacity-50"
              >
                Accept
              </button>
              <button
                (click)="decline(inv.id)"
                [disabled]="processing() === inv.id"
                class="px-2 py-0.5 text-xs font-medium text-gray-700 bg-white border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50"
              >
                Decline
              </button>
            </div>
          </div>
        }
      </div>
    } @else {
      <div class="text-center py-4">
        <p class="text-sm text-gray-500">No pending invitations.</p>
      </div>
    }
  `,
})
export class PendingInvitationsWidgetComponent implements DashboardWidgetComponent, OnInit {
  widgetConfig = signal<Record<string, unknown> | null>(null);
  invitations = signal<PendingInvitation[]>([]);
  isLoading = signal(true);
  processing = signal<string | null>(null);

  private invitationService = inject(InvitationService);
  private userService = inject(UserService);

  ngOnInit(): void {
    if (!this.userService.currentUser()?.familyId) {
      this.invitationService.getMyPendingInvitations().subscribe({
        next: (invs) => {
          this.invitations.set(invs);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false),
      });
    } else {
      this.isLoading.set(false);
    }
  }

  accept(id: string): void {
    this.processing.set(id);
    this.invitationService.acceptInvitationById(id).subscribe({
      next: async () => {
        await this.userService.fetchCurrentUser();
        this.processing.set(null);
      },
      error: () => this.processing.set(null),
    });
  }

  decline(id: string): void {
    this.processing.set(id);
    this.invitationService.declineInvitationById(id).subscribe({
      next: () => {
        this.invitations.update((list) => list.filter((inv) => inv.id !== id));
        this.processing.set(null);
      },
      error: () => this.processing.set(null),
    });
  }
}
