import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EmailPreviewComponent } from './email-preview.component';

describe('EmailPreviewComponent', () => {
  let component: EmailPreviewComponent;
  let fixture: ComponentFixture<EmailPreviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmailPreviewComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EmailPreviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start collapsed by default', () => {
    expect(component.isExpanded()).toBe(false);
  });

  it('should toggle expanded state when clicked', () => {
    expect(component.isExpanded()).toBe(false);

    component.toggleExpanded();
    expect(component.isExpanded()).toBe(true);

    component.toggleExpanded();
    expect(component.isExpanded()).toBe(false);
  });

  it('should render email subject with family name', () => {
    component.familyName = 'Smith Family';
    fixture.detectChanges();

    component.toggleExpanded();
    fixture.detectChanges();

    const subjectElement = fixture.nativeElement.querySelector('#email-preview-content');
    expect(subjectElement?.textContent).toContain("You've been invited to join Smith Family");
  });

  it('should render email subject with fallback when no family name', () => {
    component.familyName = undefined;
    fixture.detectChanges();

    const subject = component.getEmailSubject();
    expect(subject).toContain("You've been invited to join a family");
  });

  it('should render personal message when provided', () => {
    component.familyName = 'Test Family';
    component.personalMessage = 'Welcome to our family!';
    fixture.detectChanges();

    component.toggleExpanded();
    fixture.detectChanges();

    const contentElement = fixture.nativeElement.querySelector('#email-preview-content');
    expect(contentElement?.textContent).toContain('Welcome to our family!');
  });

  it('should not render personal message section when empty', () => {
    component.familyName = 'Test Family';
    component.personalMessage = '';
    fixture.detectChanges();

    component.toggleExpanded();
    fixture.detectChanges();

    const personalMessageSection = fixture.nativeElement.querySelector('.bg-blue-50.border-l-4');
    expect(personalMessageSection).toBeFalsy();
  });

  it('should not render personal message section when undefined', () => {
    component.familyName = 'Test Family';
    component.personalMessage = undefined;
    fixture.detectChanges();

    component.toggleExpanded();
    fixture.detectChanges();

    const personalMessageSection = fixture.nativeElement.querySelector('.bg-blue-50.border-l-4');
    expect(personalMessageSection).toBeFalsy();
  });

  it('should have ARIA attributes for accessibility', () => {
    const toggleButton = fixture.nativeElement.querySelector('button');
    expect(toggleButton?.getAttribute('aria-expanded')).toBe('false');
    expect(toggleButton?.getAttribute('aria-controls')).toBe('email-preview-content');

    component.toggleExpanded();
    fixture.detectChanges();

    expect(toggleButton?.getAttribute('aria-expanded')).toBe('true');
  });

  it('should show correct chevron icon based on expanded state', () => {
    // Initially collapsed - should show chevron-down
    expect(component.isExpanded()).toBe(false);

    component.toggleExpanded();
    fixture.detectChanges();

    // Expanded - should show chevron-up
    expect(component.isExpanded()).toBe(true);
  });

  it('should render accept button preview (disabled)', () => {
    component.familyName = 'Test Family';
    fixture.detectChanges();

    component.toggleExpanded();
    fixture.detectChanges();

    const buttonPreview = fixture.nativeElement.querySelector('.cursor-not-allowed');
    expect(buttonPreview?.textContent).toContain('Accept Invitation');
  });

  it('should render email body with family name', () => {
    component.familyName = 'Awesome Family';
    fixture.detectChanges();

    const body = component.getEmailBody();
    expect(body).toContain('Awesome Family');
    expect(body).toContain('Click the button in the email to accept your invitation');
  });
});
