import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { WizardComponent } from '../../../../shared/components/organisms/wizard/wizard.component';
import { WizardStepConfig } from '../../../../shared/services/wizard.service';
import {
  FamilyNameStepComponent,
  FamilyNameStepData,
} from '../../components/family-name-step/family-name-step.component';
import { InviteMembersStepComponent } from '../../components/invite-members-step/invite-members-step.component';
import { InviteMembersStepData } from '../../models/invite-members-step.models';
import { FamilyService } from '../../services/family.service';
import { InvitationService } from '../../services/invitation.service';
import { ToastService } from '../../../../core/services/toast.service';
import { ConfirmInvitesDialogComponent } from '../../../../shared/components/organisms/confirm-invites-dialog/confirm-invites-dialog.component';

/**
 * Event emitted when wizard completes.
 * Contains all collected step data keyed by step ID.
 */
export interface WizardCompleteEvent {
  stepData: ReadonlyMap<string, unknown>;
}

/**
 * Page component that orchestrates the family creation wizard.
 *
 * **Purpose:** Provides a guided multi-step flow for creating a new family.
 * Currently implements single step (family name), but architected for future expansion.
 *
 * **Architecture:**
 * - Thin wrapper around WizardComponent
 * - Configures family-specific wizard steps
 * - Integrates with FamilyService for API calls
 * - Handles success/error states and navigation
 *
 * **Wizard Flow:**
 * 1. Family Name (FamilyNameStepComponent) - CURRENT
 * 2. [Future] Family Members - Add initial members
 * 3. [Future] Preferences - Set family preferences
 *
 * **Navigation Guards:**
 * - noFamilyGuard: Prevents access if user already has family
 * - Redirects to dashboard after successful creation
 *
 * **Error Handling:**
 * - GraphQL errors displayed in wizard UI via FamilyService.error signal
 * - Network errors trigger retry prompt
 * - Validation errors prevent wizard progression
 *
 * @example
 * ```typescript
 * // In routes configuration:
 * {
 *   path: 'family/create',
 *   component: FamilyWizardPageComponent,
 *   canActivate: [authGuard, noFamilyGuard]
 * }
 * ```
 */
@Component({
  selector: 'app-family-wizard-page',
  standalone: true,
  imports: [CommonModule, WizardComponent, ConfirmInvitesDialogComponent],
  template: `
    <!-- Error Alert -->
    @if (familyService.error()) {
      <div
        role="alert"
        class="fixed top-4 left-1/2 -translate-x-1/2 z-50 max-w-md w-full mx-4 bg-red-50 border-l-4 border-red-500 p-4 shadow-lg"
      >
        <div class="flex items-start">
          <div class="flex-shrink-0">
            <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
              <path
                fill-rule="evenodd"
                d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                clip-rule="evenodd"
              />
            </svg>
          </div>
          <div class="ml-3 flex-1">
            <p class="text-sm text-red-700">{{ familyService.error() }}</p>
          </div>
          <button
            type="button"
            class="ml-3 flex-shrink-0 inline-flex text-red-400 hover:text-red-600 focus:outline-none"
            (click)="familyService.error.set(null)"
            aria-label="Close error message"
          >
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path
                fill-rule="evenodd"
                d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                clip-rule="evenodd"
              />
            </svg>
          </button>
        </div>
      </div>
    }

    <app-wizard
      title="Create Your Family"
      [steps]="wizardSteps"
      submitButtonText="Create Family"
      [isSubmitting]="familyService.isLoading()"
      (complete)="onWizardComplete($event)"
    ></app-wizard>

    <!-- Confirmation Dialog -->
    <app-confirm-invites-dialog
      [isOpen]="showConfirmDialog()"
      [invitations]="pendingInviteData()?.invitations ?? []"
      [familyName]="familyService.currentFamily()?.name ?? ''"
      [personalMessage]="pendingInviteData()?.message"
      (confirm)="onConfirmSendInvitations()"
      (cancelled)="onCancelSendInvitations()"
    />
  `,
  styles: [],
})
export class FamilyWizardPageComponent implements OnInit {
  /**
   * Injected FamilyService for state management and API calls.
   * Public to allow template access for error/loading state display.
   */
  familyService = inject(FamilyService);

  /**
   * Injected InvitationService for sending email invitations.
   */
  private invitationService = inject(InvitationService);

  /**
   * Injected ToastService for user feedback notifications.
   */
  private toastService = inject(ToastService);

  /**
   * Injected Router for navigation after wizard completion.
   */
  private router = inject(Router);

  /**
   * Signal for showing confirmation dialog.
   */
  showConfirmDialog = signal(false);

  /**
   * Signal for pending invitation data before confirmation.
   */
  pendingInviteData = signal<InviteMembersStepData | null>(null);

  /**
   * Wizard step configurations.
   * Defines the sequence of steps, their components, and validation logic.
   *
   * **Current Steps:**
   * 1. family-name: Collect family name (required, max 50 chars)
   *
   * **Future Steps:**
   * 2. family-members: Add initial family members (optional)
   * 3. family-preferences: Configure family settings (optional)
   */
  wizardSteps: WizardStepConfig[] = [
    {
      id: 'family-name',
      componentType: FamilyNameStepComponent,
      title: 'Family Name',
      validateOnNext: (stepData) => {
        const data = stepData.get('family-name') as FamilyNameStepData | undefined;

        // Validation: Family name is required
        if (!data?.name) {
          return ['Family name is required.'];
        }

        // Validation: Trim whitespace and check if empty
        const trimmedName = data.name.trim();
        if (trimmedName.length === 0) {
          return ['Family name cannot be only whitespace.'];
        }

        // Validation: Max length 50 characters
        if (trimmedName.length > 50) {
          return ['Family name must be 50 characters or less.'];
        }

        // All validations passed
        return null;
      },
    },
    {
      id: 'invite-members',
      componentType: InviteMembersStepComponent,
      title: 'Invite Family Members',
      canSkip: true,
      validateOnNext: () => {
        // No validation - invitations are completely optional
        return null;
      },
    },
    // Future steps:
    // {
    //   id: 'family-preferences',
    //   componentType: FamilyPreferencesStepComponent,
    //   title: 'Family Preferences',
    //   canSkip: true
    // }
  ];

  /**
   * Lifecycle hook: Component initialization.
   * Performs triple-safety guard check to redirect if user already has a family.
   *
   * **Why this check still exists:**
   * This is a defensive programming check that should never actually trigger because:
   * 1. APP_INITIALIZER loads family data before routing begins
   * 2. noFamilyGuard blocks access if user has family
   * 3. This component-level check provides final safety net
   *
   * **When it might trigger:**
   * - If family is created in another tab after guard passes
   * - If app state becomes inconsistent (edge case)
   * - During development/testing scenarios
   */
  ngOnInit(): void {
    // Triple-safety guard: Redirect if user already has family
    if (this.familyService.hasFamily()) {
      console.warn('User already has a family. Redirecting to dashboard.');
      this.router.navigate(['/dashboard']);
    }
  }

  /**
   * Retry loading family data after an error.
   * Clears error state and triggers loadCurrentFamily() again.
   */
  retry(): void {
    this.familyService.loadCurrentFamily();
  }

  /**
   * Handles wizard completion event.
   * Extracts family data, calls FamilyService to create family,
   * and navigates to dashboard on success.
   *
   * **Flow:**
   * 1. Extract family name from step data
   * 2. Validate data presence (defensive check)
   * 3. Call FamilyService.createFamily() with trimmed name
   * 4. Check for errors from API
   * 5. Navigate to dashboard on success
   *
   * **Error Handling:**
   * - Missing data: Log error, stay on wizard
   * - API errors: Displayed via FamilyService.error signal, stay on wizard
   * - Network errors: Handled by GraphQLService, displayed in wizard
   *
   * @param event - Wizard complete event containing all step data
   */
  async onWizardComplete(event: Map<string, unknown>): Promise<void> {
    // Extract family name data from step
    const familyNameData = event.get('family-name') as FamilyNameStepData | undefined;

    // Defensive check: Ensure data exists
    // Validation should have caught this, but safety first
    if (!familyNameData?.name) {
      console.error('Missing family name data in wizard completion');
      return;
    }

    // Trim whitespace from family name
    const trimmedName = familyNameData.name.trim();

    // Additional defensive check for empty trimmed name
    if (trimmedName.length === 0) {
      console.error('Family name is empty after trimming');
      return;
    }

    // Call FamilyService to create family
    // Service handles loading state, error state, and API call
    await this.familyService.createFamily(trimmedName);

    // Check for errors from family creation
    // Errors are displayed in wizard UI via FamilyService.error signal
    if (this.familyService.error()) {
      // Error is already displayed in UI
      // Could also show global error toast here if desired
      console.error('Family creation failed:', this.familyService.error());
      return;
    }

    // Success: Family created
    console.log('Family created successfully.');
    this.toastService.success('Family created successfully!');

    // Check if there are invitations to send
    const inviteData = event.get('invite-members') as InviteMembersStepData | undefined;

    if (inviteData?.invitations && inviteData.invitations.length > 0) {
      // Show confirmation dialog instead of sending immediately
      this.pendingInviteData.set(inviteData);
      this.showConfirmDialog.set(true);
      return; // Wait for confirmation
    }

    // No invitations - navigate directly to dashboard
    this.router.navigate(['/dashboard']);
  }

  /**
   * Handles confirmation to send invitations.
   * Called when user clicks "Send Invitations" in confirmation dialog.
   */
  async onConfirmSendInvitations(): Promise<void> {
    this.showConfirmDialog.set(false);

    const inviteData = this.pendingInviteData();
    const familyId = this.familyService.currentFamily()?.id;

    if (!familyId || !inviteData?.invitations) {
      console.error('Missing family ID or invitation data');
      this.router.navigate(['/dashboard']);
      return;
    }

    console.log(`Sending ${inviteData.invitations.length} invitations...`);

    try {
      // Send invitations via InvitationService
      const result = await this.invitationService.inviteFamilyMembersByEmail(
        familyId,
        inviteData.invitations.map((inv) => ({
          email: inv.email,
          role: inv.role as 'ADMIN' | 'MEMBER', // Backend doesn't support CHILD yet
        }))
      );

      // Show success/failure toasts
      if (result.successCount > 0) {
        this.toastService.success(`${result.successCount} invitation(s) sent!`);
      }

      if (result.errors.length > 0) {
        this.toastService.warning(`${result.errors.length} invitation(s) failed`);
        console.warn('Some invitations failed:', result.errors);
      }
    } catch (err) {
      // Network or unexpected error
      console.error('Failed to send invitations:', err);
      this.toastService.error('Failed to send invitations');
    } finally {
      // Clear pending data and navigate to dashboard
      this.pendingInviteData.set(null);
      this.router.navigate(['/dashboard']);
    }
  }

  /**
   * Handles cancellation of sending invitations.
   * Called when user clicks "Cancel" in confirmation dialog.
   */
  onCancelSendInvitations(): void {
    this.showConfirmDialog.set(false);
    this.pendingInviteData.set(null);
    // Navigate to dashboard without sending invitations
    this.router.navigate(['/dashboard']);
  }
}
