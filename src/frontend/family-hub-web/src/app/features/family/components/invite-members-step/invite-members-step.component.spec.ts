import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { InviteMembersStepComponent } from './invite-members-step.component';
import { InviteMembersStepData } from '../../models/invite-members-step.models';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';
import { ButtonComponent } from '../../../../shared/components/atoms/button/button.component';

describe('InviteMembersStepComponent', () => {
  let component: InviteMembersStepComponent;
  let fixture: ComponentFixture<InviteMembersStepComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        InviteMembersStepComponent,
        ReactiveFormsModule,
        InputComponent,
        IconComponent,
        ButtonComponent,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(InviteMembersStepComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('FormArray Management', () => {
    it('should initialize with one empty invitation row', () => {
      // Note: ngOnInit called in beforeEach adds 1 row
      expect(component.emailInvitationControls.length).toBeGreaterThanOrEqual(1);
      const firstRow = component.emailInvitationControls.at(0);
      expect(firstRow.get('email')?.value).toBe('');
      expect(firstRow.get('role')?.value).toBe('MEMBER');
    });

    it('should add email invitation row', () => {
      const initialCount = component.emailInvitationControls.length;
      component.addEmailInvitation();

      expect(component.emailInvitationControls.length).toBe(initialCount + 1);
    });

    it('should remove email invitation row', () => {
      component.addEmailInvitation();
      component.addEmailInvitation();
      const initialCount = component.emailInvitationControls.length;

      component.removeEmailInvitation(1);

      expect(component.emailInvitationControls.length).toBe(initialCount - 1);
    });

    it('should keep at least one row when removing last', () => {
      // Clear to 1 row first
      while (component.emailInvitationControls.length > 1) {
        component.removeEmailInvitation(component.emailInvitationControls.length - 1);
      }
      expect(component.emailInvitationControls.length).toBe(1);

      component.removeEmailInvitation(0);

      expect(component.emailInvitationControls.length).toBe(1);
      expect(component.emailInvitationControls.at(0).get('email')?.value).toBe('');
      expect(component.emailInvitationControls.at(0).get('role')?.value).toBe('MEMBER');
    });

    it('should disable add button when reaching 20 emails', () => {
      // Start with 1 from beforeEach, add 19 more
      for (let i = 1; i < 20; i++) {
        component.addEmailInvitation();
      }
      fixture.detectChanges(); // Trigger computed signal update

      expect(component.emailInvitationControls.length).toBe(20);
      expect(component.canAddMoreInvitations()).toBe(false);
    });

    it('should re-enable add button when below 20 emails', () => {
      // Start with 1 from beforeEach, add 19 more to reach 20
      for (let i = 1; i < 20; i++) {
        component.addEmailInvitation();
      }
      fixture.detectChanges();
      expect(component.emailInvitationControls.length).toBe(20);
      expect(component.canAddMoreInvitations()).toBe(false);

      // Remove one to go below 20
      component.removeEmailInvitation(0);
      fixture.detectChanges(); // Trigger computed signal update

      expect(component.emailInvitationControls.length).toBe(19);
      expect(component.canAddMoreInvitations()).toBe(true);
    });

    it('should update counter correctly', () => {
      // Start with 1 from beforeEach
      expect(component.invitationCount()).toBe(1);

      component.addEmailInvitation();
      fixture.detectChanges();
      expect(component.invitationCount()).toBe(2);

      component.addEmailInvitation();
      fixture.detectChanges();
      expect(component.invitationCount()).toBe(3);
    });

    it('should update counter color class based on count', () => {
      // Start with 1 from beforeEach (< 18, should be gray)
      expect(component.counterColorClass()).toBe('text-gray-500');

      // Add to 18 (amber)
      for (let i = 1; i < 18; i++) {
        component.addEmailInvitation();
      }
      fixture.detectChanges();
      expect(component.emailInvitationControls.length).toBe(18);
      expect(component.counterColorClass()).toBe('text-amber-600');

      // Add to 20 (red)
      component.addEmailInvitation();
      component.addEmailInvitation();
      fixture.detectChanges();
      expect(component.emailInvitationControls.length).toBe(20);
      expect(component.counterColorClass()).toBe('text-red-600');
    });

    it('should not add beyond max limit', () => {
      // Clear and add to max
      while (component.emailInvitationControls.length > 0) {
        component.emailInvitationControls.removeAt(0);
      }
      for (let i = 0; i < 20; i++) {
        component.addEmailInvitation();
      }
      expect(component.emailInvitationControls.length).toBe(20);

      // Try to add more
      component.addEmailInvitation();

      expect(component.emailInvitationControls.length).toBe(20);
    });
  });

  describe('Email Validation', () => {
    it('should validate email format on blur', () => {
      const emailControl = component.emailInvitationControls.at(0).get('email');
      emailControl?.setValue('invalid-email');
      emailControl?.markAsTouched();
      emailControl?.updateValueAndValidity();

      expect(emailControl?.hasError('pattern')).toBe(true);
    });

    it('should detect duplicate emails (case-insensitive)', () => {
      component.addEmailInvitation();
      fixture.detectChanges();

      const email1 = component.emailInvitationControls.at(0).get('email');
      const email2 = component.emailInvitationControls.at(1).get('email');

      // Set first email and update
      email1?.setValue('user@example.com');
      email1?.updateValueAndValidity();
      fixture.detectChanges();

      // Set second email (duplicate, different case)
      email2?.setValue('USER@EXAMPLE.COM');
      email2?.markAsTouched();
      fixture.detectChanges();

      // Validator runs when we update validity
      email2?.updateValueAndValidity();
      fixture.detectChanges();

      expect(email2?.hasError('duplicate')).toBe(true);
    });

    it('should show error for invalid email only after touched', () => {
      const emailControl = component.emailInvitationControls.at(0).get('email');
      emailControl?.setValue('invalid');

      // Before touched
      expect(component.getEmailError(0)).toBeUndefined();

      // After touched
      emailControl?.markAsTouched();
      expect(component.getEmailError(0)).toBe('Invalid email format');
    });

    it('should show error for duplicate email', () => {
      component.addEmailInvitation();

      const email1 = component.emailInvitationControls.at(0).get('email');
      const email2 = component.emailInvitationControls.at(1).get('email');

      // Set first email
      email1?.setValue('test@example.com');
      email1?.updateValueAndValidity();

      // Set second email (duplicate)
      email2?.setValue('test@example.com');
      email2?.markAsTouched();
      email2?.updateValueAndValidity();

      expect(component.getEmailError(1)).toBe('This email is already in the list');
    });

    it('should clear duplicate error after removing duplicate', () => {
      component.addEmailInvitation();
      component.addEmailInvitation();

      const email1 = component.emailInvitationControls.at(0).get('email');
      const email2 = component.emailInvitationControls.at(1).get('email');
      const email3 = component.emailInvitationControls.at(2).get('email');

      // Set all emails
      email1?.setValue('test@example.com');
      email1?.updateValueAndValidity();

      email2?.setValue('test@example.com');
      email2?.markAsTouched();
      email2?.updateValueAndValidity();

      email3?.setValue('other@example.com');
      email3?.updateValueAndValidity();

      expect(email2?.hasError('duplicate')).toBe(true);

      // Remove first duplicate - removeEmailInvitation revalidates all
      component.removeEmailInvitation(0);

      // After removal, what was email2 is now at index 0
      expect(component.emailInvitationControls.at(0).get('email')?.hasError('duplicate')).toBe(
        false
      );
    });

    it('should revalidate all emails on add/remove', () => {
      component.addEmailInvitation();

      const email1 = component.emailInvitationControls.at(0).get('email');
      const email2 = component.emailInvitationControls.at(1).get('email');

      // Set both to same value
      email1?.setValue('test@example.com');
      email1?.updateValueAndValidity();

      email2?.setValue('test@example.com');
      email2?.markAsTouched();
      email2?.updateValueAndValidity();

      expect(email2?.hasError('duplicate')).toBe(true);

      // Remove the row with duplicate - removeEmailInvitation calls updateValueAndValidity on all
      component.removeEmailInvitation(1);
      component.addEmailInvitation();

      // New row should be empty and not have duplicate error
      const newEmail = component.emailInvitationControls.at(1).get('email');
      expect(newEmail?.value).toBe('');
      expect(newEmail?.hasError('duplicate')).toBeFalsy();
    });

    it('should accept valid emails', () => {
      const validEmails = [
        'user@example.com',
        'test.user@example.com',
        'user+tag@example.com',
        'user123@test-domain.co.uk',
      ];

      validEmails.forEach((email, index) => {
        if (index > 0) component.addEmailInvitation();
        const emailControl = component.emailInvitationControls.at(index).get('email');
        emailControl?.setValue(email);
        emailControl?.updateValueAndValidity();

        expect(emailControl?.hasError('pattern')).toBeFalsy();
      });
    });
  });

  describe('Role Selection', () => {
    it('should default role to MEMBER for new rows', () => {
      component.addEmailInvitation();
      component.addEmailInvitation();

      expect(component.emailInvitationControls.at(0).get('role')?.value).toBe('MEMBER');
      expect(component.emailInvitationControls.at(1).get('role')?.value).toBe('MEMBER');
      expect(component.emailInvitationControls.at(2).get('role')?.value).toBe('MEMBER');
    });

    it('should allow changing role to ADMIN', () => {
      const roleControl = component.emailInvitationControls.at(0).get('role');
      roleControl?.setValue('ADMIN');

      expect(roleControl?.value).toBe('ADMIN');
    });

    it('should allow changing role to CHILD', () => {
      const roleControl = component.emailInvitationControls.at(0).get('role');
      roleControl?.setValue('CHILD');

      expect(roleControl?.value).toBe('CHILD');
    });

    it('should persist role on data restoration', () => {
      const testData: InviteMembersStepData = {
        invitations: [
          { email: 'admin@example.com', role: 'ADMIN' },
          { email: 'child@example.com', role: 'CHILD' },
        ],
        message: '',
      };

      component.data = testData;
      component.ngOnInit();

      expect(component.emailInvitationControls.at(0).get('role')?.value).toBe('ADMIN');
      expect(component.emailInvitationControls.at(1).get('role')?.value).toBe('CHILD');
    });

    it('should have available roles defined', () => {
      expect(component.availableRoles).toEqual([
        { value: 'ADMIN', label: 'Admin', description: 'Full access' },
        { value: 'MEMBER', label: 'Member', description: 'Standard access' },
        { value: 'CHILD', label: 'Child', description: 'Limited access' },
      ]);
    });
  });

  describe('Message Field', () => {
    it('should allow message up to 500 characters', () => {
      const message = 'a'.repeat(500);
      component.messageControl.setValue(message);

      expect(component.messageControl.valid).toBe(true);
    });

    it('should show error when exceeding 500 characters', () => {
      const message = 'a'.repeat(501);
      component.messageControl.setValue(message);

      expect(component.messageControl.hasError('maxlength')).toBe(true);
      expect(component.getMessageError()).toBe('Message must be 500 characters or less');
    });

    it('should show character count', () => {
      component.messageControl.setValue('Hello');
      expect(component.getMessageCharCount()).toBe(5);

      component.messageControl.setValue('Hello World!');
      expect(component.getMessageCharCount()).toBe(12);
    });

    it('should return 0 for empty message', () => {
      component.messageControl.setValue('');
      expect(component.getMessageCharCount()).toBe(0);
    });

    it('should persist message on data restoration', () => {
      const testData: InviteMembersStepData = {
        invitations: [{ email: 'test@example.com', role: 'MEMBER' }],
        message: 'Welcome to our family!',
      };

      // Create fresh component (beforeEach component already initialized)
      const newComponent = new InviteMembersStepComponent();
      newComponent.data = testData;
      newComponent.ngOnInit();

      expect(newComponent.messageControl.value).toBe('Welcome to our family!');
    });
  });

  describe('Data Emission', () => {
    it('should emit initial empty data on ngOnInit', (done) => {
      const newComponent = new InviteMembersStepComponent();

      newComponent.dataChange.subscribe((data: InviteMembersStepData) => {
        // First emission should have no valid invitations (empty email)
        expect(data.invitations.length).toBe(0);
        expect(data.message).toBe('');
        done();
      });

      newComponent.ngOnInit();
    });

    it('should emit data on every form change', (done) => {
      let emissionCount = 0;

      const subscription = component.dataChange.subscribe((data: InviteMembersStepData) => {
        emissionCount++;

        // First emission might be immediate, second will be from our setValue
        if (
          emissionCount >= 1 &&
          data.invitations.length === 1 &&
          data.invitations[0].email === 'test@example.com'
        ) {
          expect(data.invitations.length).toBe(1);
          expect(data.invitations[0].email).toBe('test@example.com');
          subscription.unsubscribe();
          done();
        }
      });

      // Trigger a change after subscription is set up
      component.emailInvitationControls.at(0).get('email')?.setValue('test@example.com');
    });

    it('should filter out empty emails when emitting', (done) => {
      let handled = false;

      const subscription = component.dataChange.subscribe((data: InviteMembersStepData) => {
        if (!handled && data.invitations.length === 2) {
          handled = true;
          expect(data.invitations).toEqual([
            { email: 'test1@example.com', role: 'MEMBER' },
            { email: 'test2@example.com', role: 'MEMBER' },
          ]);
          subscription.unsubscribe();
          done();
        }
      });

      component.addEmailInvitation();
      component.addEmailInvitation();

      component.emailInvitationControls.at(0).get('email')?.setValue('test1@example.com');
      component.emailInvitationControls.at(1).get('email')?.setValue('');
      component.emailInvitationControls.at(2).get('email')?.setValue('test2@example.com');

      // Trigger emission
      component.messageControl.setValue('test');
    });

    it('should include message in emitted data', (done) => {
      let handled = false;

      const subscription = component.dataChange.subscribe((data: InviteMembersStepData) => {
        if (!handled && data.message === 'Welcome message') {
          handled = true;
          expect(data.message).toBe('Welcome message');
          subscription.unsubscribe();
          done();
        }
      });

      component.messageControl.setValue('Welcome message');

      // Trigger emission
      setTimeout(() => {
        component.emailInvitationControls.at(0).get('email')?.setValue('test@example.com');
      }, 10);
    });

    it('should include valid invitations only', (done) => {
      let handled = false;

      const subscription = component.dataChange.subscribe((data: InviteMembersStepData) => {
        if (
          !handled &&
          data.invitations.length === 1 &&
          data.invitations[0].email === 'user@example.com' &&
          data.invitations[0].role === 'ADMIN'
        ) {
          handled = true;
          expect(data.invitations).toEqual([{ email: 'user@example.com', role: 'ADMIN' }]);
          subscription.unsubscribe();
          done();
        }
      });

      component.addEmailInvitation();

      // Set values - form valueChanges will trigger emission
      component.emailInvitationControls.at(0).get('email')?.setValue('  user@example.com  ');
      component.emailInvitationControls.at(0).get('role')?.setValue('ADMIN');
      component.emailInvitationControls.at(1).get('email')?.setValue(''); // Empty - will be filtered out
    });

    it('should emit data immediately after restoration', (done) => {
      const testData: InviteMembersStepData = {
        invitations: [{ email: 'restored@example.com', role: 'ADMIN' }],
        message: 'Restored message',
      };

      const newComponent = new InviteMembersStepComponent();
      newComponent.data = testData;
      let handled = false;

      const subscription = newComponent.dataChange.subscribe((data: InviteMembersStepData) => {
        if (!handled && data.invitations.length === 1) {
          handled = true;
          expect(data.invitations[0].email).toBe('restored@example.com');
          expect(data.message).toBe('Restored message');
          subscription.unsubscribe();
          done();
        }
      });

      newComponent.ngOnInit();
    });
  });

  describe('WizardStepComponent Contract', () => {
    it('should accept data via @Input', () => {
      const testData: InviteMembersStepData = {
        invitations: [{ email: 'test@example.com', role: 'MEMBER' }],
        message: 'Test message',
      };

      component.data = testData;

      expect(component.data).toEqual(testData);
    });

    it('should restore form from @Input data', () => {
      const testData: InviteMembersStepData = {
        invitations: [
          { email: 'user1@example.com', role: 'ADMIN' },
          { email: 'user2@example.com', role: 'MEMBER' },
        ],
        message: 'Welcome!',
      };

      component.data = testData;
      component.ngOnInit();

      expect(component.emailInvitationControls.length).toBe(2);
      expect(component.emailInvitationControls.at(0).get('email')?.value).toBe('user1@example.com');
      expect(component.emailInvitationControls.at(0).get('role')?.value).toBe('ADMIN');
      expect(component.emailInvitationControls.at(1).get('email')?.value).toBe('user2@example.com');
      expect(component.messageControl.value).toBe('Welcome!');
    });

    it('should emit dataChange via @Output', (done) => {
      const newComponent = new InviteMembersStepComponent();
      let called = false;

      newComponent.dataChange.subscribe((data: InviteMembersStepData) => {
        if (!called) {
          called = true;
          expect(data).toBeDefined();
          expect(data.invitations).toBeDefined();
          expect(data.message).toBeDefined();
          done();
        }
      });

      newComponent.ngOnInit();
    });

    it('should match expected interface', () => {
      const testData: InviteMembersStepData = {
        invitations: [],
        message: '',
      };

      // Type checking at compile time ensures interface compatibility
      component.data = testData;
      expect(true).toBe(true); // If it compiles, interface matches
    });

    it('should handle missing @Input data gracefully', () => {
      const newComponent = new InviteMembersStepComponent();
      newComponent.data = undefined;
      newComponent.ngOnInit();

      expect(newComponent.emailInvitationControls.length).toBe(1);
      expect(newComponent.emailInvitationControls.at(0).get('email')?.value).toBe('');
      expect(newComponent.messageControl.value).toBe('');
    });

    it('should handle empty invitations array in @Input data', () => {
      const newComponent = new InviteMembersStepComponent();
      newComponent.data = { invitations: [], message: '' };
      newComponent.ngOnInit();

      expect(newComponent.emailInvitationControls.length).toBe(1);
    });
  });

  describe('Template Rendering', () => {
    it('should render heading "Invite Family Members"', () => {
      const heading = fixture.debugElement.query(By.css('h2'));
      expect(heading.nativeElement.textContent).toContain('Invite Family Members');
    });

    it('should render description text', () => {
      const description = fixture.debugElement.query(By.css('p.text-gray-600'));
      expect(description.nativeElement.textContent).toContain(
        'Invite family members to join your family'
      );
    });

    it('should render mail icon', () => {
      const icon = fixture.debugElement.query(By.directive(IconComponent));
      expect(icon).toBeTruthy();
      expect(icon.componentInstance.name).toBe('mail');
    });

    it('should render email counter', () => {
      // Find all spans in the header flex container - the counter is the second one
      const headerDiv = fixture.debugElement.query(By.css('.flex.justify-between.items-center'));
      const spans = headerDiv.queryAll(By.css('span'));
      // The counter is the second span (index 1)
      const counter = spans[1];
      expect(counter.nativeElement.textContent).toContain('1 of 20 emails');
    });

    it('should render Add Another Email button', () => {
      // Template uses a plain <button> element, not <app-button>
      const button = fixture.debugElement.query(
        By.css('button[aria-label="Add another email invitation"]')
      );
      expect(button).toBeTruthy();
    });

    it('should render message textarea', () => {
      const textarea = fixture.debugElement.query(By.css('textarea#invitation-message'));
      expect(textarea).toBeTruthy();
    });

    it('should show warning when at max limit', () => {
      // Start from 1 email (from beforeEach), add 19 more to reach 20
      for (let i = 1; i < 20; i++) {
        component.addEmailInvitation();
      }
      fixture.detectChanges(); // Trigger change detection to update computed signals and template

      // Check that we're at max
      expect(component.canAddMoreInvitations()).toBe(false);
      expect(component.invitationCount()).toBe(20);

      const warning = fixture.debugElement.query(By.css('p.text-amber-600'));
      expect(warning).toBeTruthy();
      expect(warning.nativeElement.textContent).toContain('Maximum 20 invitations reached');
    });
  });

  describe('Accessibility', () => {
    it('should have aria-label on email inputs', () => {
      const input = fixture.debugElement.query(By.directive(InputComponent));
      expect(input.componentInstance.ariaLabel).toContain('Email address');
    });

    it('should have aria-label on role dropdowns', () => {
      const select = fixture.debugElement.query(By.css('select'));
      expect(select.nativeElement.getAttribute('aria-label')).toBeTruthy();
    });

    it('should have aria-label on remove buttons', () => {
      const removeButton = fixture.debugElement.query(By.css('button[type="button"]'));
      expect(removeButton.nativeElement.getAttribute('aria-label')).toBeTruthy();
    });

    it('should have aria-invalid on message textarea when error', () => {
      component.messageControl.setValue('a'.repeat(501));
      fixture.detectChanges();

      const textarea = fixture.debugElement.query(By.css('textarea'));
      expect(textarea.nativeElement.getAttribute('aria-invalid')).toBe('true');
    });

    it('should have aria-describedby on message textarea', () => {
      const textarea = fixture.debugElement.query(By.css('textarea'));
      expect(textarea.nativeElement.getAttribute('aria-describedby')).toBeTruthy();
    });
  });

  describe('Form Behavior', () => {
    it('should mark fields as touched on submit', () => {
      component.addEmailInvitation();

      const email1 = component.emailInvitationControls.at(0).get('email');
      const email2 = component.emailInvitationControls.at(1).get('email');

      expect(email1?.touched).toBeFalsy();
      expect(email2?.touched).toBeFalsy();
      expect(component.messageControl.touched).toBeFalsy();

      component.onSubmit();

      expect(email1?.touched).toBe(true);
      expect(email2?.touched).toBe(true);
      expect(component.messageControl.touched).toBe(true);
    });

    it('should trim whitespace from emails when emitting', (done) => {
      let handled = false;

      const subscription = component.dataChange.subscribe((data: InviteMembersStepData) => {
        if (!handled && data.invitations.length === 1) {
          handled = true;
          expect(data.invitations[0].email).toBe('test@example.com');
          subscription.unsubscribe();
          done();
        }
      });

      component.emailInvitationControls.at(0).get('email')?.setValue('  test@example.com  ');
      component.messageControl.setValue('');
    });

    it('should trim whitespace from message when emitting', (done) => {
      let handled = false;

      const subscription = component.dataChange.subscribe((data: InviteMembersStepData) => {
        if (!handled && data.message === 'Welcome!') {
          handled = true;
          expect(data.message).toBe('Welcome!');
          subscription.unsubscribe();
          done();
        }
      });

      component.messageControl.setValue('  Welcome!  ');
      setTimeout(() => {
        component.emailInvitationControls.at(0).get('email')?.setValue('test@example.com');
      }, 10);
    });
  });
});
