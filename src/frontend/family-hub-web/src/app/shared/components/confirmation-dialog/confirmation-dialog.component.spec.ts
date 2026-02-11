import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmationDialogComponent } from './confirmation-dialog.component';

describe('ConfirmationDialogComponent', () => {
  let component: ConfirmationDialogComponent;
  let fixture: ComponentFixture<ConfirmationDialogComponent>;
  let nativeElement: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfirmationDialogComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmationDialogComponent);
    component = fixture.componentInstance;
    nativeElement = fixture.nativeElement;
  });

  function render(): void {
    fixture.detectChanges();
  }

  it('should create', () => {
    render();
    expect(component).toBeTruthy();
  });

  it('should render with default props', () => {
    render();
    expect(queryByTestId('confirmation-dialog-title')?.textContent?.trim()).toBe('Confirm');
    expect(queryByTestId('confirmation-dialog-message')?.textContent?.trim()).toBe('Are you sure?');
    expect(queryByTestId('confirmation-dialog-confirm')?.textContent?.trim()).toBe('Confirm');
    expect(queryByTestId('confirmation-dialog-cancel')?.textContent?.trim()).toBe('Cancel');
  });

  it('should render with custom props', () => {
    component.title = 'Delete Item';
    component.message = 'This will permanently delete the item.';
    component.confirmLabel = 'Delete';
    component.cancelLabel = 'Keep';
    render();

    expect(queryByTestId('confirmation-dialog-title')?.textContent?.trim()).toBe('Delete Item');
    expect(queryByTestId('confirmation-dialog-message')?.textContent?.trim()).toBe(
      'This will permanently delete the item.',
    );
    expect(queryByTestId('confirmation-dialog-confirm')?.textContent?.trim()).toBe('Delete');
    expect(queryByTestId('confirmation-dialog-cancel')?.textContent?.trim()).toBe('Keep');
  });

  it('should emit confirmed on confirm button click', () => {
    render();
    const spy = vi.fn();
    component.confirmed.subscribe(spy);

    queryByTestId('confirmation-dialog-confirm')?.click();

    expect(spy).toHaveBeenCalledTimes(1);
  });

  it('should emit cancelled on cancel button click', () => {
    render();
    const spy = vi.fn();
    component.cancelled.subscribe(spy);

    queryByTestId('confirmation-dialog-cancel')?.click();

    expect(spy).toHaveBeenCalledTimes(1);
  });

  it('should emit cancelled on overlay click', () => {
    render();
    const spy = vi.fn();
    component.cancelled.subscribe(spy);

    queryByTestId('confirmation-dialog-overlay')?.click();

    expect(spy).toHaveBeenCalledTimes(1);
  });

  it('should not emit cancelled on dialog body click (stopPropagation)', () => {
    render();
    const spy = vi.fn();
    component.cancelled.subscribe(spy);

    queryByTestId('confirmation-dialog')?.click();

    expect(spy).not.toHaveBeenCalled();
  });

  it('should emit cancelled on Escape key', () => {
    render();
    const spy = vi.fn();
    component.cancelled.subscribe(spy);

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));

    expect(spy).toHaveBeenCalledTimes(1);
  });

  it('should not emit cancelled on Escape key when loading', () => {
    component.isLoading = true;
    render();

    const spy = vi.fn();
    component.cancelled.subscribe(spy);

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));

    expect(spy).not.toHaveBeenCalled();
  });

  it('should show "Processing..." when loading', () => {
    component.isLoading = true;
    render();

    expect(queryByTestId('confirmation-dialog-confirm')?.textContent?.trim()).toBe('Processing...');
  });

  it('should disable buttons when loading', () => {
    component.isLoading = true;
    render();

    expect(queryByTestId('confirmation-dialog-confirm')?.getAttribute('disabled')).not.toBeNull();
    expect(queryByTestId('confirmation-dialog-cancel')?.getAttribute('disabled')).not.toBeNull();
  });

  it('should apply danger variant styles to confirm button', () => {
    component.variant = 'danger';
    render();

    const button = queryByTestId('confirmation-dialog-confirm');
    expect(button?.classList.contains('bg-red-500')).toBe(true);
  });

  it('should apply warning variant styles to confirm button', () => {
    component.variant = 'warning';
    render();

    const button = queryByTestId('confirmation-dialog-confirm');
    expect(button?.classList.contains('bg-amber-500')).toBe(true);
  });

  it('should apply info variant styles to confirm button', () => {
    render();

    const button = queryByTestId('confirmation-dialog-confirm');
    expect(button?.classList.contains('bg-blue-500')).toBe(true);
  });

  it('should have correct accessibility attributes', () => {
    render();
    const overlay = queryByTestId('confirmation-dialog-overlay');
    expect(overlay?.getAttribute('role')).toBe('dialog');
    expect(overlay?.getAttribute('aria-modal')).toBe('true');
    expect(overlay?.getAttribute('aria-labelledby')).toBe('confirmation-dialog-title');
  });

  it('should have all required data-testid attributes', () => {
    render();
    expect(queryByTestId('confirmation-dialog-overlay')).toBeTruthy();
    expect(queryByTestId('confirmation-dialog')).toBeTruthy();
    expect(queryByTestId('confirmation-dialog-title')).toBeTruthy();
    expect(queryByTestId('confirmation-dialog-message')).toBeTruthy();
    expect(queryByTestId('confirmation-dialog-confirm')).toBeTruthy();
    expect(queryByTestId('confirmation-dialog-cancel')).toBeTruthy();
    expect(queryByTestId('confirmation-dialog-icon')).toBeTruthy();
  });

  it('should render icon with correct variant styling', () => {
    component.variant = 'danger';
    render();

    const iconContainer = queryByTestId('confirmation-dialog-icon');
    expect(iconContainer?.classList.contains('bg-red-50')).toBe(true);
    expect(iconContainer?.classList.contains('text-red-500')).toBe(true);
  });

  it('should render warning icon with correct variant styling', () => {
    component.variant = 'warning';
    render();

    const iconContainer = queryByTestId('confirmation-dialog-icon');
    expect(iconContainer?.classList.contains('bg-amber-50')).toBe(true);
    expect(iconContainer?.classList.contains('text-amber-500')).toBe(true);
  });

  it('should render info icon with correct variant styling', () => {
    render();

    const iconContainer = queryByTestId('confirmation-dialog-icon');
    expect(iconContainer?.classList.contains('bg-blue-50')).toBe(true);
    expect(iconContainer?.classList.contains('text-blue-500')).toBe(true);
  });

  it('should render trash icon when icon input is trash', () => {
    component.icon = 'trash';
    render();

    const iconContainer = queryByTestId('confirmation-dialog-icon');
    const svg = iconContainer?.querySelector('svg');
    expect(svg).toBeTruthy();
    // Trash icon has a distinctive path with "M14.74"
    const path = svg?.querySelector('path');
    expect(path?.getAttribute('d')).toContain('M14.74');
  });

  function queryByTestId(testId: string): HTMLElement | null {
    return nativeElement.querySelector(`[data-testid="${testId}"]`);
  }
});
