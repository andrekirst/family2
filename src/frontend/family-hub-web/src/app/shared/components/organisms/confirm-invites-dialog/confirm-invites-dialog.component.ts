import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalComponent } from '../../molecules/modal/modal.component';
import { EmailPreviewComponent } from '../../molecules/email-preview/email-preview.component';
import { ButtonComponent } from '../../atoms/button/button.component';

/**
 * User role type for invitations
 */
export type UserRole = 'ADMIN' | 'MEMBER' | 'CHILD';

/**
 * Invitation item interface
 */
export interface InvitationItem {
  email: string;
  role: UserRole;
}

/**
 * Confirm Invites Dialog Component
 *
 * **Purpose:** Shows confirmation dialog before sending family member invitations
 *
 * **Features:**
 * - Shows invitation summary (emails, roles)
 * - Integrates EmailPreviewComponent (collapsible)
 * - Confirm/Cancel actions
 * - Warning message about immediate sending
 * - WCAG 2.1 AA compliant (ARIA, keyboard support)
 *
 * **Usage:**
 * ```html
 * <app-confirm-invites-dialog
 *   [isOpen]="showConfirmDialog()"
 *   [invitations]="pendingInviteData()?.invitations ?? []"
 *   [familyName]="familyService.currentFamily()?.name ?? ''"
 *   [personalMessage]="pendingInviteData()?.message"
 *   (confirm)="onConfirmSendInvitations()"
 *   (cancel)="onCancelSendInvitations()"
 * />
 * ```
 *
 * **Workflow:**
 * 1. User completes invite members step in wizard
 * 2. Dialog shows summary of invitations to be sent
 * 3. User can preview email content (collapsible)
 * 4. User confirms → invitations sent via API
 * 5. User cancels → returns to wizard without sending
 *
 * @example
 * ```typescript
 * // In family-wizard-page.component.ts
 * showConfirmDialog = signal(false);
 * pendingInviteData = signal<InviteMembersStepData | null>(null);
 *
 * async onWizardComplete(event: Map<string, unknown>): Promise<void> {
 *   const inviteData = event.get('invite-members') as InviteMembersStepData;
 *
 *   if (inviteData?.invitations && inviteData.invitations.length > 0) {
 *     this.pendingInviteData.set(inviteData);
 *     this.showConfirmDialog.set(true);
 *     return; // Wait for confirmation
 *   }
 *
 *   this.router.navigate(['/dashboard']);
 * }
 * ```
 */
@Component({
  selector: 'app-confirm-invites-dialog',
  standalone: true,
  imports: [CommonModule, ModalComponent, EmailPreviewComponent, ButtonComponent],
  templateUrl: './confirm-invites-dialog.component.html',
  styleUrls: ['./confirm-invites-dialog.component.scss'],
})
export class ConfirmInvitesDialogComponent {
  /**
   * Controls modal visibility
   */
  @Input() isOpen = false;

  /**
   * Array of invitations to send
   */
  @Input() invitations: InvitationItem[] = [];

  /**
   * Family name for email preview
   */
  @Input() familyName = '';

  /**
   * Personal message for email preview
   */
  @Input() personalMessage?: string;

  /**
   * Event emitted when user confirms sending invitations
   */
  @Output() confirm = new EventEmitter<void>();

  /**
   * Event emitted when user cancels
   */
  @Output() cancel = new EventEmitter<void>();

  /**
   * Gets Tailwind CSS classes for role badge
   *
   * @param role - User role (ADMIN/MEMBER/CHILD)
   * @returns CSS class string
   */
  getRoleBadgeClasses(role: UserRole): string {
    const baseClasses = 'inline-block px-2 py-1 text-xs font-semibold rounded';

    const roleClasses = {
      ADMIN: 'bg-purple-100 text-purple-800',
      MEMBER: 'bg-blue-100 text-blue-800',
      CHILD: 'bg-green-100 text-green-800',
    };

    return `${baseClasses} ${roleClasses[role]}`;
  }

  /**
   * Handles confirm button click
   */
  onConfirm(): void {
    this.confirm.emit();
  }

  /**
   * Handles cancel button click
   */
  onCancel(): void {
    this.cancel.emit();
  }
}
