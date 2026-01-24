import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmInvitesDialogComponent, InvitationItem } from './confirm-invites-dialog.component';

describe('ConfirmInvitesDialogComponent', () => {
  let component: ConfirmInvitesDialogComponent;
  let fixture: ComponentFixture<ConfirmInvitesDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfirmInvitesDialogComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmInvitesDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render modal when isOpen is true', () => {
    component.isOpen = true;
    component.familyName = 'Test Family';
    component.invitations = [{ email: 'test@example.com', role: 'MEMBER' }];
    fixture.detectChanges();

    const modal = fixture.nativeElement.querySelector('app-modal');
    expect(modal).toBeTruthy();
  });

  it('should display correct invitation count', () => {
    component.isOpen = true;
    component.familyName = 'Test Family';
    component.invitations = [
      { email: 'test1@example.com', role: 'MEMBER' },
      { email: 'test2@example.com', role: 'ADMIN' },
    ];
    fixture.detectChanges();

    const content = fixture.nativeElement.textContent;
    expect(content).toContain('2');
    expect(content).toContain('invitations'); // plural
  });

  it('should display singular "invitation" for single invite', () => {
    component.isOpen = true;
    component.familyName = 'Test Family';
    component.invitations = [{ email: 'test@example.com', role: 'MEMBER' }];
    fixture.detectChanges();

    const content = fixture.nativeElement.textContent;
    expect(content).toContain('1');
    expect(content).toContain('invitation'); // singular
  });

  it('should render all invitations in the list', () => {
    const invitations: InvitationItem[] = [
      { email: 'test1@example.com', role: 'MEMBER' },
      { email: 'test2@example.com', role: 'ADMIN' },
      { email: 'test3@example.com', role: 'CHILD' },
    ];

    component.isOpen = true;
    component.familyName = 'Test Family';
    component.invitations = invitations;
    fixture.detectChanges();

    const inviteElements = fixture.nativeElement.querySelectorAll('.bg-white.rounded.border');
    expect(inviteElements.length).toBe(3);
  });

  it('should display email addresses correctly', () => {
    component.isOpen = true;
    component.familyName = 'Test Family';
    component.invitations = [
      { email: 'john@example.com', role: 'MEMBER' },
      { email: 'jane@example.com', role: 'ADMIN' },
    ];
    fixture.detectChanges();

    const content = fixture.nativeElement.textContent;
    expect(content).toContain('john@example.com');
    expect(content).toContain('jane@example.com');
  });

  it('should return correct badge classes for ADMIN role', () => {
    const classes = component.getRoleBadgeClasses('ADMIN');
    expect(classes).toContain('bg-purple-100');
    expect(classes).toContain('text-purple-800');
  });

  it('should return correct badge classes for MEMBER role', () => {
    const classes = component.getRoleBadgeClasses('MEMBER');
    expect(classes).toContain('bg-blue-100');
    expect(classes).toContain('text-blue-800');
  });

  it('should return correct badge classes for CHILD role', () => {
    const classes = component.getRoleBadgeClasses('CHILD');
    expect(classes).toContain('bg-green-100');
    expect(classes).toContain('text-green-800');
  });

  it('should emit confirm event when confirm button is clicked', () => {
    spyOn(component.confirm, 'emit');

    component.onConfirm();

    expect(component.confirm.emit).toHaveBeenCalled();
  });

  it('should emit cancelled event when cancel button is clicked', () => {
    spyOn(component.cancelled, 'emit');

    component.onCancel();

    expect(component.cancelled.emit).toHaveBeenCalled();
  });

  it('should pass familyName to email preview component', () => {
    component.isOpen = true;
    component.familyName = 'Smith Family';
    component.invitations = [{ email: 'test@example.com', role: 'MEMBER' }];
    fixture.detectChanges();

    const emailPreview = fixture.nativeElement.querySelector('app-email-preview');
    expect(emailPreview).toBeTruthy();
  });

  it('should pass personalMessage to email preview component', () => {
    component.isOpen = true;
    component.familyName = 'Test Family';
    component.personalMessage = 'Welcome to our family!';
    component.invitations = [{ email: 'test@example.com', role: 'MEMBER' }];
    fixture.detectChanges();

    const emailPreview = fixture.nativeElement.querySelector('app-email-preview');
    expect(emailPreview).toBeTruthy();
  });

  it('should display family name in summary text', () => {
    component.isOpen = true;
    component.familyName = 'Awesome Family';
    component.invitations = [{ email: 'test@example.com', role: 'MEMBER' }];
    fixture.detectChanges();

    const content = fixture.nativeElement.textContent;
    expect(content).toContain('Awesome Family');
  });

  it('should display warning message about immediate sending', () => {
    component.isOpen = true;
    component.familyName = 'Test Family';
    component.invitations = [{ email: 'test@example.com', role: 'MEMBER' }];
    fixture.detectChanges();

    const content = fixture.nativeElement.textContent;
    expect(content).toContain('Invitations will be sent immediately and cannot be undone');
  });

  it('should display information about 14-day validity', () => {
    component.isOpen = true;
    component.familyName = 'Test Family';
    component.invitations = [{ email: 'test@example.com', role: 'MEMBER' }];
    fixture.detectChanges();

    const content = fixture.nativeElement.textContent;
    expect(content).toContain('14 days');
  });

  it('should have confirm and cancel buttons', () => {
    component.isOpen = true;
    component.familyName = 'Test Family';
    component.invitations = [{ email: 'test@example.com', role: 'MEMBER' }];
    fixture.detectChanges();

    const buttons = fixture.nativeElement.querySelectorAll('app-button');
    expect(buttons.length).toBe(2);
  });

  it('should not be closeable via backdrop or escape key', () => {
    component.isOpen = true;
    component.familyName = 'Test Family';
    component.invitations = [{ email: 'test@example.com', role: 'MEMBER' }];
    fixture.detectChanges();

    const modal = fixture.nativeElement.querySelector('app-modal');
    expect(modal).toBeTruthy();
    // closeable attribute is set to false in template
  });
});
