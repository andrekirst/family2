import { Component, inject, signal, effect, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MembersListComponent } from '../members-list/members-list.component';
import { PendingInvitationsComponent } from '../pending-invitations/pending-invitations.component';
import { InviteMemberComponent } from '../invite-member/invite-member.component';
import { FamilyPermissionService } from '../../../../core/permissions/family-permission.service';
import { TopBarService } from '../../../../shared/services/top-bar.service';

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
          <span i18n="@@family.settings.members">Members</span>
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
            <span i18n="@@family.settings.pendingInvitations">Pending Invitations</span>
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

    <!-- Invite Member Dialog -->
    @if (showInviteDialog()) {
      <app-invite-member
        (invitationSent)="onInvitationSent()"
        (dialogClosed)="onInviteDialogClosed()"
      />
    }
  `,
})
export class FamilySettingsComponent implements OnDestroy {
  readonly permissions = inject(FamilyPermissionService);
  private readonly topBarService = inject(TopBarService);

  @ViewChild('membersList') membersList?: MembersListComponent;
  @ViewChild('pendingInvitations') pendingInvitations?: PendingInvitationsComponent;

  activeTab = signal<'members' | 'invitations'>('members');
  showInviteDialog = signal(false);

  private readonly topBarEffect = effect(() => {
    const canInvite = this.permissions.canInvite();
    this.topBarService.setConfig({
      title: $localize`:@@family.settings.title:Family Settings`,
      actions: canInvite
        ? [
            {
              id: 'invite-member',
              label: $localize`:@@family.settings.inviteMember:Invite Member`,
              onClick: () => this.openInviteDialog(),
              variant: 'primary',
            },
          ]
        : [],
    });
  });

  ngOnDestroy(): void {
    this.topBarService.clear();
  }

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
