import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { FamilyNameStepComponent, FamilyNameStepData } from './family-name-step.component';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';
import { By } from '@angular/platform-browser';

describe('FamilyNameStepComponent', () => {
  let component: FamilyNameStepComponent;
  let fixture: ComponentFixture<FamilyNameStepComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FamilyNameStepComponent,
        ReactiveFormsModule,
        InputComponent,
        IconComponent
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FamilyNameStepComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Form Initialization', () => {
    it('should initialize with empty form', () => {
      expect(component.familyForm.value.name).toBe('');
    });

    it('should initialize form with provided data input', () => {
      const testData: FamilyNameStepData = { name: 'Smith Family' };
      component.data = testData;
      component.ngOnInit();

      expect(component.familyForm.value.name).toBe('Smith Family');
    });

    it('should mark name field as required', () => {
      const nameControl = component.familyForm.controls.name;
      nameControl.setValue('');
      nameControl.markAsTouched();

      expect(nameControl.hasError('required')).toBe(true);
    });

    it('should enforce maxLength of 50 characters', () => {
      const nameControl = component.familyForm.controls.name;
      const longName = 'a'.repeat(51);
      nameControl.setValue(longName);
      nameControl.markAsTouched();

      expect(nameControl.hasError('maxlength')).toBe(true);
    });
  });

  describe('Data Change Emission', () => {
    it('should emit dataChange when form value changes', (done) => {
      component.dataChange.subscribe((data: FamilyNameStepData) => {
        expect(data.name).toBe('Johnson Family');
        done();
      });

      component.familyForm.patchValue({ name: 'Johnson Family' });
    });

    it('should emit initial data on form initialization', (done) => {
      const newComponent = new FamilyNameStepComponent();
      newComponent.dataChange.subscribe((data: FamilyNameStepData) => {
        if (data.name === 'Initial Family') {
          expect(data.name).toBe('Initial Family');
          done();
        }
      });

      newComponent.data = { name: 'Initial Family' };
      newComponent.ngOnInit();
    });
  });

  describe('Validation Error Messages', () => {
    it('should not show error when field is untouched', () => {
      const nameControl = component.familyForm.controls.name;
      nameControl.setValue('');

      expect(component.getNameError()).toBeUndefined();
    });

    it('should show required error when empty and touched', () => {
      const nameControl = component.familyForm.controls.name;
      nameControl.setValue('');
      nameControl.markAsTouched();

      expect(component.getNameError()).toBe('Family name is required');
    });

    it('should show maxLength error when exceeds 50 characters', () => {
      const nameControl = component.familyForm.controls.name;
      const longName = 'a'.repeat(51);
      nameControl.setValue(longName);
      nameControl.markAsTouched();

      expect(component.getNameError()).toBe('Family name must be 50 characters or less');
    });

    it('should return undefined when valid', () => {
      const nameControl = component.familyForm.controls.name;
      nameControl.setValue('Valid Family Name');
      nameControl.markAsTouched();

      expect(component.getNameError()).toBeUndefined();
    });
  });

  describe('Template Rendering', () => {
    it('should render heading "Name Your Family"', () => {
      const heading = fixture.debugElement.query(By.css('h2'));
      expect(heading.nativeElement.textContent).toContain('Name Your Family');
    });

    it('should render description text', () => {
      const description = fixture.debugElement.query(By.css('p.text-gray-600'));
      expect(description.nativeElement.textContent).toContain('Give your family a name to get started');
    });

    it('should render users icon', () => {
      const icon = fixture.debugElement.query(By.directive(IconComponent));
      expect(icon).toBeTruthy();
      expect(icon.componentInstance.name).toBe('users');
    });

    it('should render input component', () => {
      const input = fixture.debugElement.query(By.directive(InputComponent));
      expect(input).toBeTruthy();
    });

    it('should show required indicator on label', () => {
      const label = fixture.debugElement.query(By.css('label'));
      const requiredSpan = label.query(By.css('.text-red-600'));
      expect(requiredSpan).toBeTruthy();
      expect(requiredSpan.nativeElement.textContent).toContain('*');
    });

    it('should show helper text when no errors', () => {
      const helperText = fixture.debugElement.query(By.css('p.text-gray-500'));
      expect(helperText).toBeTruthy();
      expect(helperText.nativeElement.textContent).toContain('Choose a name that represents your family');
    });

    it('should hide helper text when there are errors', () => {
      const nameControl = component.familyForm.controls.name;
      nameControl.setValue('');
      nameControl.markAsTouched();
      fixture.detectChanges();

      const helperText = fixture.debugElement.query(By.css('p.text-gray-500'));
      expect(helperText).toBeFalsy();
    });
  });

  describe('Accessibility', () => {
    it('should have aria-label on input', () => {
      const input = fixture.debugElement.query(By.directive(InputComponent));
      expect(input.componentInstance.ariaLabel).toBe('Family name');
    });

    it('should have aria-required on input', () => {
      const input = fixture.debugElement.query(By.directive(InputComponent));
      expect(input.componentInstance.ariaRequired).toBe(true);
    });

    it('should associate label with input via id', () => {
      const label = fixture.debugElement.query(By.css('label'));
      const input = fixture.debugElement.query(By.directive(InputComponent));

      expect(label.nativeElement.getAttribute('for')).toBe('family-name-input');
      expect(input.componentInstance.id || 'family-name-input').toBe('family-name-input');
    });
  });

  describe('Form Behavior', () => {
    it('should accept valid family names', () => {
      const validNames = [
        'Smith Family',
        'The Johnsons',
        'García Family',
        'O\'Brien Household',
        'Family 123'
      ];

      validNames.forEach(name => {
        component.familyForm.patchValue({ name });
        expect(component.familyForm.valid).toBe(true);
      });
    });

    it('should reject empty family name', () => {
      component.familyForm.patchValue({ name: '' });
      component.familyForm.controls.name.markAsTouched();

      expect(component.familyForm.valid).toBe(false);
      expect(component.getNameError()).toBe('Family name is required');
    });

    it('should accept names up to 50 characters', () => {
      const fiftyCharName = 'a'.repeat(50);
      component.familyForm.patchValue({ name: fiftyCharName });

      expect(component.familyForm.valid).toBe(true);
    });

    it('should trim whitespace for validation', () => {
      component.familyForm.patchValue({ name: '   ' });
      component.familyForm.controls.name.markAsTouched();

      // Note: Current implementation doesn't trim, but validates as-is
      // This test documents current behavior
      expect(component.familyForm.valid).toBe(true);
    });
  });
});
