import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FamilyService } from '../../services/family.service';
import { InvitationService } from '../../services/invitation.service';
import { UserRole } from '../../models/family.models';
import { FamilyMembersListComponent } from '../../components/family-members-list/family-members-list.component';
import { PendingInvitationsComponent } from '../../components/pending-invitations/pending-invitations.component';
import { InviteMemberModalComponent } from '../../components/invite-member-modal/invite-member-modal.component';
import { MainLayoutComponent } from '../../../../shared/layout/main-layout/main-layout.component';

/**
 * Family Management Page - Main interface for managing family members and invitations.
 *
 * Features:
 * - View all family members with roles
 * - Manage pending invitations
 * - Invite new members via email
 *
 * @example
 * Router configuration:
 * ```typescript
 * {
 *   path: 'family/manage',
 *   component: FamilyManagementComponent,
 *   canActivate: [AuthGuard]
 * }
 * ```
 */
@Component({
  selector: 'app-family-management',
  standalone: true,
  imports: [
    CommonModule,
    MainLayoutComponent,
    FamilyMembersListComponent,
    PendingInvitationsComponent,
    InviteMemberModalComponent,
  ],
  templateUrl: './family-management.component.html',
  styleUrl: './family-management.component.scss',
})
export class FamilyManagementComponent implements OnInit {
  private familyService = inject(FamilyService);
  private invitationService = inject(InvitationService);

  /**
   * Reactive state from services.
   */
  familyMembers = this.familyService.familyMembers;
  currentFamily = this.familyService.currentFamily;
  isLoading = this.familyService.isLoading;
  error = this.familyService.error;

  /**
   * Modal states.
   */
  showInviteModal = signal<boolean>(false);

  /**
   * Current user's role (for permission checks - future use).
   */
  currentUserRole = signal<UserRole | undefined>(undefined);

  async ngOnInit(): Promise<void> {
    await this.loadData();
  }

  /**
   * Loads all data for the page.
   */
  async loadData(): Promise<void> {
    try {
      // Load current family
      await this.familyService.loadCurrentFamily();

      const family = this.currentFamily();
      if (!family) {
        console.error('No current family found');
        return;
      }

      // Load family members
      await this.familyService.loadFamilyMembers(family.id);

      // Load pending invitations
      await this.invitationService.loadPendingInvitations(family.id);

      // TODO: Load current user's role from auth context
      // For now, assume OWNER for development
      this.currentUserRole.set('OWNER');
    } catch (err) {
      console.error('Failed to load family management data:', err);
    }
  }

  /**
   * Opens the invite member modal.
   */
  openInviteModal(): void {
    this.showInviteModal.set(true);
  }

  /**
   * Closes the invite member modal and refreshes data.
   */
  async closeInviteModal(): Promise<void> {
    this.showInviteModal.set(false);
    // Refresh data to show new invitations/members
    await this.loadData();
  }
}
