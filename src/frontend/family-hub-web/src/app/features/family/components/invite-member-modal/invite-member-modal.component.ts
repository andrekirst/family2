import { Component, Input, Output, EventEmitter, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormGroup, FormArray, FormControl, Validators } from '@angular/forms';
import { InvitationService } from '../../services/invitation.service';
import { FamilyService } from '../../services/family.service';
import { RoleService } from '../../../../core/services/role.service';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';

/**
 * Modal for inviting family members via email.
 *
 * Features:
 * - Email invitations (single or batch)
 * - Role selection (ADMIN or MEMBER)
 * - Validation and error handling
 * - Results display after submission
 *
 * @example
 * ```html
 * <app-invite-member-modal
 *   [isOpen]="showModal()"
 *   (close)="showModal.set(false)" />
 * ```
 */
@Component({
  selector: 'app-invite-member-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, InputComponent],
  templateUrl: './invite-member-modal.component.html',
  styleUrl: './invite-member-modal.component.scss',
})
export class InviteMemberModalComponent implements OnInit {
  private invitationService = inject(InvitationService);
  private familyService = inject(FamilyService);
  private roleService = inject(RoleService);

  /**
   * Controls modal visibility.
   */
  @Input() isOpen = false;

  /**
   * Emits when modal should close.
   */
  @Output() closed = new EventEmitter<void>();

  /**
   * Reactive form for email invitations.
   * Each invitation has email and role fields.
   */
  emailForm = new FormGroup({
    emails: new FormArray<FormGroup>([])
  });

  /**
   * Tracks the last number of invitations to detect when new fields are added.
   * Used for aria-live announcements.
   */
  previousEmailCount = 0;

  /**
   * Email validation regex.
   */
  private readonly EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  /**
   * Role selection.
   */
  selectedRole = signal<'ADMIN' | 'MEMBER'>('MEMBER');

  /**
   * Available roles for invitations (from RoleService).
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

  /**
   * Loading state during submission.
   */
  isSubmitting = signal<boolean>(false);

  /**
   * Results after submission.
   */
  successCount = signal<number>(0);
  errors = signal<string[]>([]);
  showResults = signal<boolean>(false);

  /**
   * Gets the FormArray of email invitation groups.
   * Each group contains email and role controls.
   */
  get emailInvitationControls(): FormArray<FormGroup> {
    return this.emailForm.get('emails') as FormArray<FormGroup>;
  }

  /**
   * Creates a new FormGroup for an email invitation.
   * Contains email field (with validation) and role field.
   */
  private createEmailInvitationGroup(): FormGroup {
    return new FormGroup({
      email: new FormControl('', {
        nonNullable: true,
        updateOn: 'blur',
        validators: [Validators.pattern(this.EMAIL_REGEX)]
      }),
      role: new FormControl<'ADMIN' | 'MEMBER'>('MEMBER', { nonNullable: true })
    });
  }

  /**
   * Adds a new email invitation row to the FormArray.
   */
  addEmailInvitation(): void {
    this.emailInvitationControls.push(this.createEmailInvitationGroup());
    this.previousEmailCount = this.emailInvitationControls.length - 1;
  }

  /**
   * Removes an email invitation row at the specified index.
   * Always keeps at least one field.
   */
  removeEmailInvitation(index: number): void {
    if (this.emailInvitationControls.length <= 1) {
      // Clear the group instead of removing
      const group = this.emailInvitationControls.at(0);
      group.get('email')?.setValue('');
      group.get('role')?.setValue('MEMBER');
      group.markAsUntouched();
      return;
    }

    this.emailInvitationControls.removeAt(index);

    // Focus next field after removal
    setTimeout(() => {
      const nextIndex = Math.min(index, this.emailInvitationControls.length - 1);
      const nextInput = document.getElementById(`email-input-${nextIndex}`);
      nextInput?.focus();
    }, 0);
  }

  /**
   * Checks if email in invitation group at index is valid (for checkmark display).
   */
  isEmailGroupValid(index: number): boolean {
    const group = this.emailInvitationControls.at(index);
    const emailControl = group.get('email');
    return emailControl ? emailControl.valid && emailControl.value.trim() !== '' : false;
  }

  /**
   * Lifecycle hook - initializes form with one empty invitation row.
   * Loads available roles from API.
   */
  async ngOnInit(): Promise<void> {
    this.addEmailInvitation();
    await this.roleService.loadRoles();
  }

  /**
   * Validates and submits the invitation form.
   */
  async submit(): Promise<void> {
    // Clear previous results
    this.errors.set([]);
    this.successCount.set(0);
    this.showResults.set(false);

    // Validate
    const validationErrors = this.validate();
    if (validationErrors.length > 0) {
      this.errors.set(validationErrors);
      this.showResults.set(true);
      return;
    }

    this.isSubmitting.set(true);

    try {
      await this.submitEmailInvitations();
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : 'An unexpected error occurred';
      this.errors.set([errorMessage]);
      this.showResults.set(true);
    } finally {
      this.isSubmitting.set(false);
    }
  }

  /**
   * Validates form inputs.
   */
  private validate(): string[] {
    const errors: string[] = [];

    // Extract emails from FormGroups
    const emails = this.emailInvitationControls.controls
      .map(group => group.get('email')?.value?.trim() || '')
      .filter(email => email !== '');

    if (emails.length === 0) {
      errors.push('Please enter at least one email address');
    }

    // Check for invalid emails
    const hasInvalidEmails = this.emailInvitationControls.controls.some(group => {
      const emailControl = group.get('email');
      const emailValue = emailControl?.value?.trim() || '';
      return emailControl?.invalid && emailValue !== '';
    });

    if (hasInvalidEmails) {
      errors.push('Please fix invalid email addresses');
    }

    return errors;
  }

  /**
   * Extracts valid email-role pairs from FormArray.
   * Returns array of objects with email and role properties.
   */
  private parseEmailInvitations(): { email: string; role: 'ADMIN' | 'MEMBER' }[] {
    return this.emailInvitationControls.controls
      .map(group => ({
        email: group.get('email')?.value?.trim() || '',
        role: (group.get('role')?.value as 'ADMIN' | 'MEMBER') || 'MEMBER'
      }))
      .filter(invitation => invitation.email !== '' && this.EMAIL_REGEX.test(invitation.email));
  }

  /**
   * Submits email invitations.
   */
  private async submitEmailInvitations(): Promise<void> {
    const family = this.familyService.currentFamily();
    if (!family) {
      this.errors.set(['No family found. Please create a family first.']);
      this.showResults.set(true);
      return;
    }

    // Get email-role pairs from FormArray
    const invitations = this.parseEmailInvitations();

    const result = await this.invitationService.inviteFamilyMembersByEmail(
      family.id,
      invitations
    );

    // Process results
    this.successCount.set(result.successCount || 0);
    if (result.errors && result.errors.length > 0) {
      this.errors.set(result.errors.map((e) => e.message));
    }
    this.showResults.set(true);

    // If no errors, close modal after 2 seconds
    if ((!result.errors || result.errors.length === 0) && result.successCount > 0) {
      setTimeout(() => this.closeModal(), 2000);
    }
  }

  /**
   * Closes the modal and resets state.
   */
  closeModal(): void {
    this.closed.emit();
    this.clearForm();
    this.clearResults();
  }

  /**
   * Clears form fields.
   */
  private clearForm(): void {
    this.emailInvitationControls.clear();
    this.addEmailInvitation();
    this.previousEmailCount = 0;
    this.selectedRole.set('MEMBER');
  }

  /**
   * Clears results state.
   */
  private clearResults(): void {
    this.successCount.set(0);
    this.errors.set([]);
    this.showResults.set(false);
  }
}
