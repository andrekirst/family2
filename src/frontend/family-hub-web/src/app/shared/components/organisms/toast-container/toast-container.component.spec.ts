import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastContainerComponent } from './toast-container.component';
import { ToastService } from '../../../../core/services/toast.service';

describe('ToastContainerComponent', () => {
  let component: ToastContainerComponent;
  let fixture: ComponentFixture<ToastContainerComponent>;
  let toastService: ToastService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToastContainerComponent, BrowserAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(ToastContainerComponent);
    component = fixture.componentInstance;
    toastService = TestBed.inject(ToastService);
    fixture.detectChanges();
  });

  afterEach(() => {
    // Clean up toasts after each test
    toastService.dismissAll();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display toasts from ToastService', () => {
    toastService.success('Test success message');
    fixture.detectChanges();

    expect(component.toastService.toasts().length).toBe(1);
    expect(component.toastService.toasts()[0].message).toBe('Test success message');
  });

  it('should render multiple toasts', () => {
    toastService.success('Message 1');
    toastService.error('Message 2');
    toastService.warning('Message 3');
    fixture.detectChanges();

    expect(component.toastService.toasts().length).toBe(3);
  });

  it('should dismiss toast when dismiss() is called', () => {
    toastService.info('Test message');
    fixture.detectChanges();

    const toastId = component.toastService.toasts()[0].id;
    component.dismiss(toastId);
    fixture.detectChanges();

    expect(component.toastService.toasts().length).toBe(0);
  });

  it('should return correct CSS classes for success toast', () => {
    const classes = component.getToastClasses('success');
    expect(classes).toContain('bg-green-50');
    expect(classes).toContain('border-green-500');
    expect(classes).toContain('text-green-900');
  });

  it('should return correct CSS classes for error toast', () => {
    const classes = component.getToastClasses('error');
    expect(classes).toContain('bg-red-50');
    expect(classes).toContain('border-red-500');
    expect(classes).toContain('text-red-900');
  });

  it('should return correct CSS classes for warning toast', () => {
    const classes = component.getToastClasses('warning');
    expect(classes).toContain('bg-amber-50');
    expect(classes).toContain('border-amber-500');
    expect(classes).toContain('text-amber-900');
  });

  it('should return correct CSS classes for info toast', () => {
    const classes = component.getToastClasses('info');
    expect(classes).toContain('bg-blue-50');
    expect(classes).toContain('border-blue-500');
    expect(classes).toContain('text-blue-900');
  });

  it('should return correct icon name for success toast', () => {
    expect(component.getIconName('success')).toBe('check-circle');
  });

  it('should return correct icon name for error toast', () => {
    expect(component.getIconName('error')).toBe('x-circle');
  });

  it('should return correct icon name for warning toast', () => {
    expect(component.getIconName('warning')).toBe('exclamation-triangle');
  });

  it('should return correct icon name for info toast', () => {
    expect(component.getIconName('info')).toBe('information-circle');
  });

  it('should return correct icon color class for success toast', () => {
    expect(component.getIconColorClass('success')).toBe('text-green-600');
  });

  it('should return correct icon color class for error toast', () => {
    expect(component.getIconColorClass('error')).toBe('text-red-600');
  });

  it('should return correct icon color class for warning toast', () => {
    expect(component.getIconColorClass('warning')).toBe('text-amber-600');
  });

  it('should return correct icon color class for info toast', () => {
    expect(component.getIconColorClass('info')).toBe('text-blue-600');
  });

  it('should have ARIA attributes for accessibility', () => {
    toastService.success('Accessible message');
    fixture.detectChanges();

    const toastElement = fixture.nativeElement.querySelector('[role="alert"]');
    expect(toastElement).toBeTruthy();
    expect(toastElement?.getAttribute('aria-live')).toBe('assertive');
  });

  it('should have accessible close button', () => {
    toastService.success('Test message');
    fixture.detectChanges();

    const closeButton = fixture.nativeElement.querySelector(
      'button[aria-label="Dismiss notification"]'
    );
    expect(closeButton).toBeTruthy();
  });
});
