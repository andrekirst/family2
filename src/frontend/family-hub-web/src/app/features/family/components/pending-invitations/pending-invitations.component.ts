import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InvitationService } from '../../services/invitation.service';
import { FamilyService } from '../../services/family.service';
import { RoleService } from '../../../../core/services/role.service';
import { PendingInvitation, UserRole } from '../../models/family.models';

/**
 * Displays pending email invitations with management controls.
 * Allows role updates and cancellation of invitations.
 *
 * Features:
 * - Expiry countdown display
 * - Role dropdown for updates
 * - Cancel button
 * - Empty state when no invitations
 *
 * @example
 * ```html
 * <app-pending-invitations />
 * ```
 */
@Component({
  selector: 'app-pending-invitations',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pending-invitations.component.html',
  styleUrl: './pending-invitations.component.scss',
})
export class PendingInvitationsComponent implements OnInit {
  private invitationService = inject(InvitationService);
  private familyService = inject(FamilyService);
  private roleService = inject(RoleService);

  /**
   * Reactive state from service.
   */
  pendingInvitations = this.invitationService.pendingInvitations;
  isLoading = this.invitationService.isLoading;
  error = this.invitationService.error;

  /**
   * Track which invitations are being updated/cancelled.
   */
  updatingInvitations = signal<Set<string>>(new Set());

  /**
   * Available roles for invitation updates (from RoleService).
   * Excludes OWNER.
   */
  availableRoles = this.roleService.invitableRoles;

  /**
   * Loading state for roles.
   */
  rolesLoading = this.roleService.isLoading;

  /**
   * Error state for roles.
   */
  rolesError = this.roleService.error;

  async ngOnInit(): Promise<void> {
    // Load both roles and invitations in parallel
    await Promise.all([this.roleService.loadRoles(), this.loadInvitations()]);
  }

  /**
   * Loads pending invitations from the service.
   */
  async loadInvitations(): Promise<void> {
    const family = this.familyService.currentFamily();
    if (!family) {
      console.error('No family found');
      return;
    }

    try {
      await this.invitationService.loadPendingInvitations();
    } catch (err) {
      console.error('Failed to load invitations:', err);
    }
  }

  /**
   * Updates the role of an invitation.
   */
  async updateRole(invitation: PendingInvitation, newRole: UserRole): Promise<void> {
    if (newRole === invitation.role) return; // No change

    this.addUpdating(invitation.id);
    try {
      await this.invitationService.updateInvitationRole(invitation.id, newRole);
      await this.loadInvitations(); // Refresh list
    } catch (err) {
      console.error('Failed to update role:', err);
    } finally {
      this.removeUpdating(invitation.id);
    }
  }

  /**
   * Cancels a pending invitation.
   */
  async cancelInvitation(invitation: PendingInvitation): Promise<void> {
    if (!confirm(`Cancel invitation for ${invitation.email}?`)) return;

    this.addUpdating(invitation.id);
    try {
      await this.invitationService.cancelInvitation(invitation.id);
      await this.loadInvitations(); // Refresh list
    } catch (err) {
      console.error('Failed to cancel invitation:', err);
    } finally {
      this.removeUpdating(invitation.id);
    }
  }

  /**
   * Checks if an invitation is currently being updated.
   */
  isUpdating(invitationId: string): boolean {
    return this.updatingInvitations().has(invitationId);
  }

  /**
   * Calculates time until expiry in human-readable format.
   */
  getExpiryText(expiresAt: string): string {
    const now = new Date();
    const expiry = new Date(expiresAt);
    const diffMs = expiry.getTime() - now.getTime();

    if (diffMs <= 0) return 'Expired';

    const days = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    const hours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

    if (days > 0) return `${days}d ${hours}h`;
    if (hours > 0) return `${hours}h`;

    const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
    return `${minutes}m`;
  }

  /**
   * Checks if an invitation is expired.
   */
  isExpired(expiresAt: string): boolean {
    return new Date(expiresAt) <= new Date();
  }

  /**
   * Formats date for display.
   */
  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  private addUpdating(id: string): void {
    const current = new Set(this.updatingInvitations());
    current.add(id);
    this.updatingInvitations.set(current);
  }

  private removeUpdating(id: string): void {
    const current = new Set(this.updatingInvitations());
    current.delete(id);
    this.updatingInvitations.set(current);
  }
}
