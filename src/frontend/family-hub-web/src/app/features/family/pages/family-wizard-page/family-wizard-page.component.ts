import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { WizardComponent } from '../../../../shared/components/organisms/wizard/wizard.component';
import { WizardStepConfig } from '../../../../shared/services/wizard.service';
import { FamilyNameStepComponent, FamilyNameStepData } from '../../components/family-name-step/family-name-step.component';
import { FamilyService } from '../../services/family.service';

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
  imports: [
    CommonModule,
    WizardComponent
  ],
  template: `
    <app-wizard
      title="Create Your Family"
      [steps]="wizardSteps"
      submitButtonText="Create Family"
      (complete)="onWizardComplete($event)"
    ></app-wizard>
  `,
  styles: []
})
export class FamilyWizardPageComponent implements OnInit {
  /**
   * Injected FamilyService for state management and API calls.
   * Public to allow template access for error/loading state display.
   */
  familyService = inject(FamilyService);

  /**
   * Injected Router for navigation after wizard completion.
   */
  private router = inject(Router);

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
      }
    }
    // Future steps will be added here:
    // {
    //   id: 'family-members',
    //   componentType: FamilyMembersStepComponent,
    //   title: 'Add Family Members',
    //   canSkip: true
    // },
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

    // Success: Navigate to dashboard
    // FamilyService.currentFamily signal is now populated
    console.log('Family created successfully. Navigating to dashboard.');
    this.router.navigate(['/dashboard']);
  }
}
