import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InlineEditTextComponent } from './inline-edit-text.component';

describe('InlineEditTextComponent', () => {
  let component: InlineEditTextComponent;
  let fixture: ComponentFixture<InlineEditTextComponent>;
  let nativeElement: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InlineEditTextComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(InlineEditTextComponent);
    component = fixture.componentInstance;
    nativeElement = fixture.nativeElement;
  });

  function render(): void {
    fixture.detectChanges();
  }

  function queryByTestId(testId: string): HTMLElement | null {
    return nativeElement.querySelector(`[data-testid="${testId}"]`);
  }

  it('should display value text', () => {
    component.value = 'Hello World';
    component.testId = 'title';
    render();

    expect(queryByTestId('title')?.textContent?.trim()).toBe('Hello World');
  });

  it('should display placeholder when value is empty', () => {
    component.value = '';
    component.placeholder = 'Enter title';
    component.testId = 'title';
    render();

    expect(queryByTestId('title')?.textContent?.trim()).toBe('Enter title');
  });

  it('should display placeholder when value is null', () => {
    component.value = null;
    component.placeholder = 'Enter title';
    component.testId = 'title';
    render();

    expect(queryByTestId('title')?.textContent?.trim()).toBe('Enter title');
  });

  it('should switch to input on click', async () => {
    component.value = 'Hello';
    component.testId = 'title';
    render();

    component.startEditing();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(queryByTestId('title-input')).toBeTruthy();
  });

  it('should render textarea when multiline is true', () => {
    component.value = 'Some text';
    component.multiline = true;
    component.testId = 'desc';
    render();

    component.startEditing();
    fixture.detectChanges();

    const input = queryByTestId('desc-input');
    expect(input?.tagName.toLowerCase()).toBe('textarea');
  });

  it('should render input when multiline is false', () => {
    component.value = 'Some text';
    component.multiline = false;
    component.testId = 'title';
    render();

    component.startEditing();
    fixture.detectChanges();

    const input = queryByTestId('title-input');
    expect(input?.tagName.toLowerCase()).toBe('input');
  });

  it('should emit saved on blur when value changed', () => {
    component.value = 'Original';
    component.testId = 'title';
    render();

    const spy = vi.fn();
    component.saved.subscribe(spy);

    component.startEditing();
    fixture.detectChanges();

    component.editValue.set('Updated');
    component.onBlur();
    fixture.detectChanges();

    expect(spy).toHaveBeenCalledWith('Updated');
  });

  it('should not emit saved when value is unchanged', () => {
    component.value = 'Same';
    component.testId = 'title';
    render();

    const spy = vi.fn();
    component.saved.subscribe(spy);

    component.startEditing();
    fixture.detectChanges();

    // Don't change value
    component.onBlur();
    fixture.detectChanges();

    expect(spy).not.toHaveBeenCalled();
  });

  it('should revert on Escape', () => {
    component.value = 'Original';
    component.testId = 'title';
    render();

    const spy = vi.fn();
    component.saved.subscribe(spy);

    component.startEditing();
    fixture.detectChanges();

    component.editValue.set('Changed');
    const event = new KeyboardEvent('keydown', { key: 'Escape' });
    component.onEscape(event);
    fixture.detectChanges();

    // Should be back to display mode with original value
    expect(component.isEditing()).toBe(false);
    expect(queryByTestId('title')?.textContent?.trim()).toBe('Original');
    expect(spy).not.toHaveBeenCalled();
  });

  it('should not enter edit mode when disabled', () => {
    component.value = 'Hello';
    component.disabled = true;
    component.testId = 'title';
    render();

    component.startEditing();
    fixture.detectChanges();

    expect(queryByTestId('title-input')).toBeNull();
    expect(component.isEditing()).toBe(false);
  });

  it('should not show pencil icon when disabled', () => {
    component.value = 'Hello';
    component.disabled = true;
    component.testId = 'title';
    render();

    expect(queryByTestId('title-pencil')).toBeNull();
  });

  it('should have correct data-testid attributes', () => {
    component.value = 'Hello';
    component.testId = 'my-field';
    render();

    expect(queryByTestId('my-field')).toBeTruthy();
    expect(queryByTestId('my-field-pencil')).toBeTruthy();
  });

  it('should trim value before emitting saved', () => {
    component.value = 'Original';
    component.testId = 'title';
    render();

    const spy = vi.fn();
    component.saved.subscribe(spy);

    component.startEditing();
    fixture.detectChanges();

    component.editValue.set('  Updated  ');
    component.onBlur();
    fixture.detectChanges();

    expect(spy).toHaveBeenCalledWith('Updated');
  });
});
