import { Component, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../../atoms/icon/icon.component';

/**
 * Email Preview Component - Collapsible email content preview
 *
 * **Purpose:** Shows preview of invitation email with subject, body, and personal message
 *
 * **Features:**
 * - Collapsible (default: collapsed to save space)
 * - Shows email subject and body preview
 * - Personal message highlighted with blue accent
 * - Read-only display
 * - ARIA attributes for accessibility
 *
 * **Usage:**
 * ```html
 * <app-email-preview
 *   [familyName]="'Smith Family'"
 *   [personalMessage]="'Join our family!'"
 * />
 * ```
 *
 * **Accessibility:**
 * - aria-expanded indicates collapsed/expanded state
 * - aria-controls links button to content region
 * - Keyboard accessible toggle button
 *
 * @example
 * ```html
 * <!-- In confirm-invites-dialog.component.html -->
 * <app-email-preview
 *   [familyName]="familyService.currentFamily()?.name ?? ''"
 *   [personalMessage]="pendingInviteData()?.message"
 * />
 * ```
 */
@Component({
  selector: 'app-email-preview',
  standalone: true,
  imports: [CommonModule, IconComponent],
  templateUrl: './email-preview.component.html',
  styleUrls: ['./email-preview.component.scss'],
})
export class EmailPreviewComponent {
  /**
   * Family name for email subject line
   */
  @Input() familyName?: string;

  /**
   * Personal message from inviter (optional)
   */
  @Input() personalMessage?: string;

  /**
   * Signal for expanded/collapsed state
   * Default: collapsed to save space
   */
  isExpanded = signal(false);

  /**
   * Toggles expanded/collapsed state
   */
  toggleExpanded(): void {
    this.isExpanded.update((value) => !value);
  }

  /**
   * Gets the email subject line
   * @returns Email subject string
   */
  getEmailSubject(): string {
    return `You've been invited to join ${this.familyName || 'a family'}`;
  }

  /**
   * Gets the email body preview (static template)
   * @returns Email body string
   */
  getEmailBody(): string {
    return `You've been invited to join ${this.familyName || 'a family'} on Family Hub. Click the button in the email to accept your invitation.`;
  }
}
