import { Component, Input, Output, EventEmitter, OnInit, computed, signal } from '@angular/core';
import {
  ReactiveFormsModule,
  FormGroup,
  FormControl,
  FormArray,
  Validators,
  ValidatorFn,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';
import { InviteMembersStepData, UserRole } from '../../models/invite-members-step.models';

/**
 * Wizard step component for inviting family members via email.
 * Implements the WizardStepComponent contract for integration with WizardService.
 *
 * **Purpose:** Second step of family creation wizard where users can optionally invite members.
 *
 * **Features:**
 * - FormArray for dynamic email inputs (max 20)
 * - Per-email role selection (Admin/Member/Child)
 * - Single shared invitation message (max 500 chars)
 * - Client-side duplicate email detection (case-insensitive)
 * - Real-time validation with error display
 * - Data persistence across wizard navigation
 * - Skip functionality (step is optional)
 *
 * **Validation Rules:**
 * - Email format: RFC 5322 pattern validation on blur
 * - Duplicate detection: Case-insensitive email comparison
 * - Message length: Max 500 characters
 * - At least 1 email row (even if empty)
 * - Max 20 email rows
 *
 * **UI/UX:**
 * - Color-coded email counter (gray → amber → red)
 * - Disabled "Add" button at limit
 * - Keep minimum 1 row (clear instead of remove)
 * - Helper text for users
 *
 * @example
 * ```typescript
 * // Used within WizardComponent:
 * {
 *   id: 'invite-members',
 *   componentType: InviteMembersStepComponent,
 *   title: 'Invite Family Members',
 *   canSkip: true,
 *   validateOnNext: () => null  // Always valid (invitations optional)
 * }
 * ```
 */
@Component({
  selector: 'app-invite-members-step',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, InputComponent, IconComponent, ButtonComponent],
  templateUrl: './invite-members-step.component.html',
  styleUrls: ['./invite-members-step.component.scss'],
})
export class InviteMembersStepComponent implements OnInit {
  /**
   * Input data from WizardService.
   * Used to restore state when navigating back to this step.
   */
  @Input() data?: InviteMembersStepData;

  /**
   * Output emitter for data changes.
   * WizardService subscribes to this to persist step data.
   */
  @Output() dataChange = new EventEmitter<InviteMembersStepData>();

  /**
   * Maximum number of email invitations allowed.
   */
  private readonly MAX_INVITATIONS = 20;

  /**
   * RFC 5322 Email validation regex.
   * Validates email format on blur.
   */
  private readonly EMAIL_REGEX = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

  /**
   * Available roles for selection.
   */
  readonly availableRoles: { value: UserRole; label: string; description: string }[] = [
    { value: 'ADMIN', label: 'Admin', description: 'Full access' },
    { value: 'MEMBER', label: 'Member', description: 'Standard access' },
    { value: 'CHILD', label: 'Child', description: 'Limited access' },
  ];

  /**
   * Reactive form for email invitations and message.
   */
  emailForm = new FormGroup({
    invitations: new FormArray<FormGroup>([]),
    message: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(500)],
    }),
  });

  /**
   * Checks whether more invitations can be added.
   * Returns true if current count is below MAX_INVITATIONS (20).
   *
   * Note: Using getter function instead of computed signal because
   * FormArray.length is not a signal - computed signals only track signal dependencies.
   */
  canAddMoreInvitations(): boolean {
    return this.emailInvitationControls.length < this.MAX_INVITATIONS;
  }

  /**
   * Gets current invitation count.
   *
   * Note: Using getter function instead of computed signal because
   * FormArray.length is not a signal - computed signals only track signal dependencies.
   */
  invitationCount(): number {
    return this.emailInvitationControls.length;
  }

  /**
   * Gets counter color class based on current count.
   * Gray: 0-17, Amber: 18-19, Red: 20
   *
   * Note: Using getter function instead of computed signal because
   * it depends on invitationCount() which reads FormArray.length (not a signal).
   */
  counterColorClass(): string {
    const count = this.invitationCount();
    if (count >= 20) return 'text-red-600';
    if (count >= 18) return 'text-amber-600';
    return 'text-gray-500';
  }

  /**
   * Gets FormArray for email invitations.
   */
  get emailInvitationControls(): FormArray<FormGroup> {
    return this.emailForm.get('invitations') as FormArray<FormGroup>;
  }

  /**
   * Gets message FormControl.
   */
  get messageControl(): FormControl {
    return this.emailForm.get('message') as FormControl;
  }

  /**
   * Initializes form with data from WizardService if available.
   * Sets up reactive effect to emit data changes.
   */
  ngOnInit(): void {
    // Restore data if navigating back
    if (this.data?.invitations && this.data.invitations.length > 0) {
      // Clear any default rows
      this.emailInvitationControls.clear();

      // Recreate rows from saved data
      this.data.invitations.forEach((inv) => {
        const group = this.createEmailInvitationGroup();
        group.patchValue({ email: inv.email, role: inv.role });
        this.emailInvitationControls.push(group);
      });

      // Restore message
      if (this.data.message) {
        this.emailForm.patchValue({ message: this.data.message });
      }
    } else {
      // Initialize with one empty row
      this.addEmailInvitation();
    }

    // Emit initial data
    this.emitData();

    // Subscribe to changes and emit data
    this.emailForm.valueChanges.subscribe(() => {
      this.emitData();
    });
  }

  /**
   * Creates a new FormGroup for an email invitation.
   * Includes email validation and duplicate detection.
   *
   * @returns FormGroup with email and role controls
   */
  private createEmailInvitationGroup(): FormGroup {
    return new FormGroup({
      email: new FormControl('', {
        nonNullable: true,
        updateOn: 'blur', // Validate on blur (not on every keystroke)
        validators: [Validators.pattern(this.EMAIL_REGEX), this.duplicateEmailValidator()],
      }),
      role: new FormControl<UserRole>('MEMBER', {
        nonNullable: true,
      }),
    });
  }

  /**
   * Custom validator for duplicate email detection.
   * Case-insensitive comparison across all email inputs.
   *
   * @returns Validator function
   */
  private duplicateEmailValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;

      const email = control.value.toLowerCase().trim();

      // Get all other email controls (exclude current control by reference)
      const otherEmails = this.emailInvitationControls.controls
        .filter((group) => group.get('email') !== control) // Exclude by control reference, not value
        .map((group) => group.get('email')?.value?.toLowerCase().trim())
        .filter((e) => e); // Filter out empty values

      if (otherEmails.includes(email)) {
        return { duplicate: true };
      }

      return null;
    };
  }

  /**
   * Adds a new email invitation row.
   * Disabled when max limit (20) is reached.
   */
  addEmailInvitation(): void {
    if (this.emailInvitationControls.length >= this.MAX_INVITATIONS) {
      return; // Prevent adding beyond limit
    }

    this.emailInvitationControls.push(this.createEmailInvitationGroup());
  }

  /**
   * Removes an email invitation row at the specified index.
   * Keeps at least one row (clears instead of removing if last row).
   *
   * @param index - Index of row to remove
   */
  removeEmailInvitation(index: number): void {
    if (this.emailInvitationControls.length <= 1) {
      // Keep at least one row - clear values instead
      const group = this.emailInvitationControls.at(0);
      group.get('email')?.setValue('');
      group.get('role')?.setValue('MEMBER');
      group.markAsUntouched();
      return;
    }

    this.emailInvitationControls.removeAt(index);

    // Revalidate all emails (duplicate detection)
    this.emailInvitationControls.controls.forEach((group) => {
      group.get('email')?.updateValueAndValidity();
    });
  }

  /**
   * Gets validation error message for email field at index.
   * Returns undefined if field is untouched or valid.
   *
   * @param index - Index of email field
   * @returns Error message or undefined
   */
  getEmailError(index: number): string | undefined {
    const group = this.emailInvitationControls.at(index);
    const emailControl = group.get('email');

    // Don't show errors until field is touched
    if (!emailControl?.touched) {
      return undefined;
    }

    if (emailControl.hasError('pattern')) {
      return 'Invalid email format';
    }

    if (emailControl.hasError('duplicate')) {
      return 'This email is already in the list';
    }

    return undefined;
  }

  /**
   * Gets validation error message for message field.
   * Returns undefined if field is valid.
   *
   * @returns Error message or undefined
   */
  getMessageError(): string | undefined {
    const control = this.messageControl;

    if (control.hasError('maxlength')) {
      return 'Message must be 500 characters or less';
    }

    return undefined;
  }

  /**
   * Gets the current message character count.
   *
   * @returns Number of characters in message
   */
  getMessageCharCount(): number {
    return this.messageControl.value?.length || 0;
  }

  /**
   * Emits current form data to WizardService.
   * Filters out empty emails and trims values.
   */
  private emitData(): void {
    const formValue = this.emailForm.getRawValue();

    // Filter out empty emails and trim
    const validInvitations = (formValue.invitations as any[])
      .filter((inv: any) => inv.email.trim() !== '')
      .map((inv: any) => ({
        email: inv.email.trim(),
        role: inv.role as UserRole,
      }));

    this.dataChange.emit({
      invitations: validInvitations,
      message: formValue.message.trim(),
    });
  }

  /**
   * Handles form submission (Enter key press).
   * Triggers the wizard's Next/Submit button programmatically.
   */
  onSubmit(): void {
    // Mark all fields as touched to trigger validation
    this.emailInvitationControls.controls.forEach((group) => {
      group.get('email')?.markAsTouched();
    });
    this.messageControl.markAsTouched();

    // Find and click the wizard's Next/Submit button in the footer
    const buttons = document.querySelectorAll<HTMLButtonElement>(
      'app-wizard footer app-button button'
    );

    if (buttons.length >= 2) {
      // Click the second button (Next/Submit, first is Back)
      buttons[1].click();
    }
  }
}
