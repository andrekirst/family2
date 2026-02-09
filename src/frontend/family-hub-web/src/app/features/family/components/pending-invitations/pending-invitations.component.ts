import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvitationService } from '../../services/invitation.service';
import { InvitationDto } from '../../models/invitation.models';
import { FamilyPermissionService } from '../../../../core/permissions/family-permission.service';

@Component({
  selector: 'app-pending-invitations',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-3">
      @if (isLoading()) {
        <div class="animate-pulse space-y-3">
          <div class="h-16 bg-gray-200 rounded"></div>
          <div class="h-16 bg-gray-200 rounded"></div>
        </div>
      } @else if (invitations().length === 0) {
        <p class="text-gray-500 text-sm">No pending invitations.</p>
      } @else {
        @for (invitation of invitations(); track invitation.id) {
          <div class="flex items-center justify-between p-4 bg-white border rounded-lg">
            <div>
              <p class="font-medium text-gray-900">{{ invitation.inviteeEmail }}</p>
              <p class="text-sm text-gray-500">
                Role: {{ invitation.role }} &middot; Expires
                {{ invitation.expiresAt | date: 'mediumDate' }}
              </p>
            </div>
            @if (permissions.canRevokeInvitation()) {
              <button
                (click)="revokeInvitation(invitation.id)"
                [disabled]="revoking() === invitation.id"
                class="px-3 py-1.5 text-xs font-medium text-red-700 bg-red-50 rounded-md hover:bg-red-100 disabled:opacity-50"
              >
                {{ revoking() === invitation.id ? 'Revoking...' : 'Revoke' }}
              </button>
            }
          </div>
        }
      }
    </div>
  `,
})
export class PendingInvitationsComponent implements OnInit {
  private invitationService = inject(InvitationService);
  permissions = inject(FamilyPermissionService);

  invitations = signal<InvitationDto[]>([]);
  isLoading = signal(true);
  revoking = signal<string | null>(null);

  ngOnInit(): void {
    this.loadInvitations();
  }

  loadInvitations(): void {
    this.isLoading.set(true);
    this.invitationService.getPendingInvitations().subscribe({
      next: (invitations) => {
        this.invitations.set(invitations);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  revokeInvitation(invitationId: string): void {
    this.revoking.set(invitationId);
    this.invitationService.revokeInvitation(invitationId).subscribe({
      next: () => {
        this.invitations.update((list) => list.filter((i) => i.id !== invitationId));
        this.revoking.set(null);
      },
      error: () => {
        this.revoking.set(null);
      },
    });
  }
}
