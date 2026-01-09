import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { InputComponent } from './input.component';

describe('InputComponent', () => {
  let component: InputComponent;
  let fixture: ComponentFixture<InputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InputComponent, ReactiveFormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(InputComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ControlValueAccessor Implementation', () => {
    it('should implement writeValue', () => {
      const testValue = 'test value';
      component.writeValue(testValue);
      expect(component.value).toBe(testValue);
    });

    it('should implement registerOnChange', () => {
      const onChange = jasmine.createSpy('onChange');
      component.registerOnChange(onChange);

      component.value = 'new value';
      component.onInputChange({ target: { value: 'new value' } } as unknown as Event);

      expect(onChange).toHaveBeenCalledWith('new value');
    });

    it('should implement registerOnTouched', () => {
      const onTouched = jasmine.createSpy('onTouched');
      component.registerOnTouched(onTouched);

      component.onBlur();

      expect(onTouched).toHaveBeenCalled();
    });

    it('should implement setDisabledState', () => {
      component.setDisabledState(true);
      expect(component.disabled).toBe(true);

      component.setDisabledState(false);
      expect(component.disabled).toBe(false);
    });

    it('should work with Angular FormControl', () => {
      const formControl = new FormControl('initial value');
      component.writeValue(formControl.value);

      expect(component.value).toBe('initial value');
    });
  });

  describe('Input Types', () => {
    it('should render text input by default', () => {
      component.type = 'text';
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.type).toBe('text');
    });

    it('should render email input', () => {
      component.type = 'email';
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.type).toBe('email');
    });

    it('should render password input', () => {
      component.type = 'password';
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.type).toBe('password');
    });
  });

  describe('Character Counter', () => {
    it('should show character counter when maxLength is set', () => {
      component.maxLength = 50;
      component.value = 'Test';
      fixture.detectChanges();

      const counter = fixture.nativeElement.querySelector('.character-counter');
      expect(counter).toBeTruthy();
      expect(counter.textContent).toContain('4/50');
    });

    it('should update character count on input', () => {
      component.maxLength = 50;
      fixture.detectChanges();

      component.value = 'Hello World';
      fixture.detectChanges();

      const counter = fixture.nativeElement.querySelector('.character-counter');
      expect(counter.textContent).toContain('11/50');
    });

    it('should not show counter when maxLength is not set', () => {
      fixture.detectChanges();

      const counter = fixture.nativeElement.querySelector('.character-counter');
      expect(counter).toBeFalsy();
    });

    it('should show 0 when value is empty', () => {
      component.maxLength = 50;
      component.value = '';
      fixture.detectChanges();

      const counter = fixture.nativeElement.querySelector('.character-counter');
      expect(counter.textContent).toContain('0/50');
    });

    it('should change color when near limit (> 90%)', () => {
      component.maxLength = 50;
      component.value = 'a'.repeat(46); // 92% of 50
      fixture.detectChanges();

      const counter = fixture.nativeElement.querySelector('.character-counter');
      expect(counter.classList.contains('text-amber-600')).toBe(true);
    });

    it('should keep default color when below 90%', () => {
      component.maxLength = 50;
      component.value = 'a'.repeat(40); // 80% of 50
      fixture.detectChanges();

      const counter = fixture.nativeElement.querySelector('.character-counter');
      expect(counter.classList.contains('text-gray-500')).toBe(true);
    });
  });

  describe('Error Display', () => {
    it('should show error message when error prop is set', () => {
      component.error = 'This field is required';
      fixture.detectChanges();

      const errorMessage = fixture.nativeElement.querySelector('.error-message');
      expect(errorMessage).toBeTruthy();
      expect(errorMessage.textContent).toContain('This field is required');
    });

    it('should not show error message when error is empty', () => {
      component.error = '';
      fixture.detectChanges();

      const errorMessage = fixture.nativeElement.querySelector('.error-message');
      expect(errorMessage).toBeFalsy();
    });

    it('should apply error styles to input when error exists', () => {
      component.error = 'Invalid input';
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.classList.contains('border-red-500')).toBe(true);
    });

    it('should apply normal styles when no error', () => {
      component.error = '';
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.classList.contains('border-gray-300')).toBe(true);
    });
  });

  describe('Disabled State', () => {
    it('should disable input when disabled prop is true', () => {
      component.disabled = true;
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.disabled).toBe(true);
    });

    it('should enable input when disabled prop is false', () => {
      component.disabled = false;
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.disabled).toBe(false);
    });

    it('should apply disabled styles when disabled', () => {
      component.disabled = true;
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.classList.contains('opacity-60')).toBe(true);
      expect(input.classList.contains('cursor-not-allowed')).toBe(true);
    });
  });

  describe('Placeholder', () => {
    it('should show placeholder text', () => {
      component.placeholder = 'Enter your name';
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.placeholder).toBe('Enter your name');
    });

    it('should work without placeholder', () => {
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.placeholder).toBe('');
    });
  });

  describe('Accessibility', () => {
    it('should have aria-invalid="true" when error exists', () => {
      component.error = 'Invalid';
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.getAttribute('aria-invalid')).toBe('true');
    });

    it('should have aria-invalid="false" when no error', () => {
      component.error = '';
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.getAttribute('aria-invalid')).toBe('false');
    });

    it('should have aria-describedby pointing to error message when error exists', () => {
      component.error = 'Error message';
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      const errorId = input.getAttribute('aria-describedby');
      expect(errorId).toBeTruthy();

      const errorMessage = fixture.nativeElement.querySelector(`#${errorId}`);
      expect(errorMessage).toBeTruthy();
      expect(errorMessage.textContent).toContain('Error message');
    });

    it('should have aria-required="true" when validators include required', () => {
      component.ariaRequired = true;
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.getAttribute('aria-required')).toBe('true');
    });

    it('should have aria-label when ariaLabel is set', () => {
      component.ariaLabel = 'Family name';
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.getAttribute('aria-label')).toBe('Family name');
    });

    it('error message should have role="alert" for screen readers', () => {
      component.error = 'Error';
      fixture.detectChanges();

      const errorMessage = fixture.nativeElement.querySelector('.error-message');
      expect(errorMessage.getAttribute('role')).toBe('alert');
    });

    it('error message should have aria-live="polite" for screen readers', () => {
      component.error = 'Error';
      fixture.detectChanges();

      const errorMessage = fixture.nativeElement.querySelector('.error-message');
      expect(errorMessage.getAttribute('aria-live')).toBe('polite');
    });
  });

  describe('Focus Management', () => {
    it('should call onTouched when input loses focus', () => {
      const onTouched = jasmine.createSpy('onTouched');
      component.registerOnTouched(onTouched);
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      input.dispatchEvent(new Event('blur'));

      expect(onTouched).toHaveBeenCalled();
    });

    it('should apply focus styles on focus', () => {
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.classList.contains('focus:border-blue-500')).toBe(true);
      expect(input.classList.contains('focus:ring-2')).toBe(true);
      expect(input.classList.contains('focus:ring-blue-200')).toBe(true);
    });
  });

  describe('Value Binding', () => {
    it('should update value when user types', () => {
      const onChange = jasmine.createSpy('onChange');
      component.registerOnChange(onChange);
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
      input.value = 'New text';
      input.dispatchEvent(new Event('input'));

      expect(onChange).toHaveBeenCalledWith('New text');
    });

    it('should reflect internal value in input element', () => {
      component.value = 'Test Value';
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
      expect(input.value).toBe('Test Value');
    });
  });
});
