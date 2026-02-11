import { Component, inject, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MembersListComponent } from '../members-list/members-list.component';
import { PendingInvitationsComponent } from '../pending-invitations/pending-invitations.component';
import { InviteMemberComponent } from '../invite-member/invite-member.component';
import { FamilyPermissionService } from '../../../../core/permissions/family-permission.service';

@Component({
  selector: 'app-family-settings',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MembersListComponent,
    PendingInvitationsComponent,
    InviteMemberComponent,
  ],
  template: `
    <div class="min-h-screen bg-gray-50">
      <header class="bg-white shadow">
        <div class="max-w-7xl mx-auto px-4 py-4 sm:px-6 lg:px-8 flex justify-between items-center">
          <div class="flex items-center gap-4">
            <a routerLink="/dashboard" class="text-gray-500 hover:text-gray-700">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M15 19l-7-7 7-7"
                />
              </svg>
            </a>
            <h1 class="text-2xl font-bold text-gray-900">Family Settings</h1>
          </div>
          @if (permissions.canInvite()) {
            <button
              (click)="openInviteDialog()"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700"
            >
              Invite Member
            </button>
          }
        </div>
      </header>

      <main class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
        <!-- Tab Navigation -->
        <div class="border-b border-gray-200 mb-6">
          <nav class="flex space-x-8">
            <button
              (click)="activeTab.set('members')"
              class="pb-3 px-1 text-sm font-medium border-b-2"
              [class.border-blue-500]="activeTab() === 'members'"
              [class.text-blue-600]="activeTab() === 'members'"
              [class.border-transparent]="activeTab() !== 'members'"
              [class.text-gray-500]="activeTab() !== 'members'"
            >
              Members
            </button>
            @if (permissions.canInvite()) {
              <button
                (click)="activeTab.set('invitations')"
                class="pb-3 px-1 text-sm font-medium border-b-2"
                [class.border-blue-500]="activeTab() === 'invitations'"
                [class.text-blue-600]="activeTab() === 'invitations'"
                [class.border-transparent]="activeTab() !== 'invitations'"
                [class.text-gray-500]="activeTab() !== 'invitations'"
              >
                Pending Invitations
              </button>
            }
          </nav>
        </div>

        <!-- Tab Content -->
        @if (activeTab() === 'members') {
          <app-members-list #membersList />
        }

        @if (activeTab() === 'invitations') {
          <app-pending-invitations #pendingInvitations />
        }
      </main>

      <!-- Invite Member Dialog -->
      @if (showInviteDialog()) {
        <app-invite-member
          (invitationSent)="onInvitationSent()"
          (dialogClosed)="onInviteDialogClosed()"
        />
      }
    </div>
  `,
})
export class FamilySettingsComponent {
  permissions = inject(FamilyPermissionService);

  @ViewChild('membersList') membersList?: MembersListComponent;
  @ViewChild('pendingInvitations') pendingInvitations?: PendingInvitationsComponent;

  activeTab = signal<'members' | 'invitations'>('members');
  showInviteDialog = signal(false);

  openInviteDialog(): void {
    this.showInviteDialog.set(true);
  }

  onInvitationSent(): void {
    this.showInviteDialog.set(false);
    // Refresh the pending invitations list
    this.activeTab.set('invitations');
    this.pendingInvitations?.loadInvitations();
  }

  onInviteDialogClosed(): void {
    this.showInviteDialog.set(false);
  }
}
