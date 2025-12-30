import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { signal } from '@angular/core';
import { CreateFamilyModalComponent } from './create-family-modal.component';
import { FamilyService } from '../../services/family.service';
import { ModalComponent } from '../../../../shared/components/molecules/modal/modal.component';
import { InputComponent } from '../../../../shared/components/atoms/input/input.component';
import { IconComponent } from '../../../../shared/components/atoms/icon/icon.component';

describe('CreateFamilyModalComponent', () => {
  let component: CreateFamilyModalComponent;
  let fixture: ComponentFixture<CreateFamilyModalComponent>;
  let familyService: any;
  let isLoadingSignal: any;
  let errorSignal: any;

  beforeEach(async () => {
    isLoadingSignal = signal(false);
    errorSignal = signal(null);

    const familyServiceSpy = jasmine.createSpyObj('FamilyService', ['createFamily'], {
      isLoading: isLoadingSignal,
      error: errorSignal
    });

    await TestBed.configureTestingModule({
      imports: [
        CreateFamilyModalComponent,
        ReactiveFormsModule,
        ModalComponent,
        InputComponent,
        IconComponent
      ],
      providers: [
        { provide: FamilyService, useValue: familyServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CreateFamilyModalComponent);
    component = fixture.componentInstance;
    familyService = TestBed.inject(FamilyService) as jasmine.SpyObj<FamilyService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Form Initialization', () => {
    it('should initialize with empty form', () => {
      expect(component.familyForm.value.name).toBe('');
    });

    it('should have invalid form when name is empty', () => {
      expect(component.familyForm.invalid).toBe(true);
    });

    it('should have valid form when name is provided', () => {
      component.familyForm.controls.name.setValue('Smith Family');
      expect(component.familyForm.valid).toBe(true);
    });
  });

  describe('Form Validation', () => {
    it('should validate required field', () => {
      const nameControl = component.familyForm.controls.name;
      nameControl.setValue('');
      nameControl.markAsTouched();

      expect(nameControl.hasError('required')).toBe(true);
      expect(component.getNameError()).toBe('Family name is required');
    });

    it('should validate max length (50 chars)', () => {
      const nameControl = component.familyForm.controls.name;
      nameControl.setValue('a'.repeat(51));
      nameControl.markAsTouched();

      expect(nameControl.hasError('maxlength')).toBe(true);
      expect(component.getNameError()).toBe('Family name must be 50 characters or less');
    });

    it('should accept valid names (1-50 chars)', () => {
      const nameControl = component.familyForm.controls.name;

      nameControl.setValue('A');
      expect(nameControl.valid).toBe(true);

      nameControl.setValue('a'.repeat(50));
      expect(nameControl.valid).toBe(true);
    });

    it('should only show error after field is touched', () => {
      const nameControl = component.familyForm.controls.name;
      nameControl.setValue('');

      expect(component.getNameError()).toBeUndefined();

      nameControl.markAsTouched();

      expect(component.getNameError()).toBe('Family name is required');
    });
  });

  describe('Submit Button State', () => {
    it('should disable submit button when form is invalid', () => {
      component.isOpen = true;
      component.familyForm.controls.name.setValue('');
      fixture.detectChanges();

      const submitButton = fixture.nativeElement.querySelector('button[type="submit"]');
      expect(submitButton.disabled).toBe(true);
    });

    it('should enable submit button when form is valid', () => {
      component.isOpen = true;
      component.familyForm.controls.name.setValue('Test Family');
      fixture.detectChanges();

      const submitButton = fixture.nativeElement.querySelector('button[type="submit"]');
      expect(submitButton.disabled).toBe(false);
    });

    it('should disable submit button when submitting', () => {
      component.isOpen = true;
      component.familyForm.controls.name.setValue('Test Family');
      isLoadingSignal.set(true);
      fixture.detectChanges();

      const submitButton = fixture.nativeElement.querySelector('button[type="submit"]');
      expect(submitButton.disabled).toBe(true);
    });
  });

  describe('Form Submission', () => {
    it('should call familyService.createFamily on submit with valid form', async () => {
      component.familyForm.controls.name.setValue('Smith Family');
      familyService.createFamily.and.returnValue(Promise.resolve());

      await component.onSubmit();

      expect(familyService.createFamily).toHaveBeenCalledWith('Smith Family');
    });

    it('should not call familyService.createFamily with invalid form', async () => {
      component.familyForm.controls.name.setValue('');

      await component.onSubmit();

      expect(familyService.createFamily).not.toHaveBeenCalled();
    });

    it('should emit onSuccess when family created successfully', async () => {
      spyOn(component.onSuccess, 'emit');
      component.familyForm.controls.name.setValue('Test Family');
      familyService.createFamily.and.returnValue(Promise.resolve());
      errorSignal.set(null);

      await component.onSubmit();

      expect(component.onSuccess.emit).toHaveBeenCalled();
    });

    it('should NOT emit onSuccess when creation fails', async () => {
      spyOn(component.onSuccess, 'emit');
      component.familyForm.controls.name.setValue('Test Family');
      familyService.createFamily.and.returnValue(Promise.resolve());
      errorSignal.set('User already has a family');

      await component.onSubmit();

      expect(component.onSuccess.emit).not.toHaveBeenCalled();
    });

    it('should reset form after successful creation', async () => {
      component.familyForm.controls.name.setValue('Test Family');
      familyService.createFamily.and.returnValue(Promise.resolve());
      errorSignal.set(null);

      await component.onSubmit();

      expect(component.familyForm.value.name).toBeNull();
      expect(component.familyForm.pristine).toBe(true);
    });

    it('should NOT reset form after failed creation', async () => {
      component.familyForm.controls.name.setValue('Test Family');
      familyService.createFamily.and.returnValue(Promise.resolve());
      errorSignal.set('Error occurred');

      await component.onSubmit();

      expect(component.familyForm.value.name).toBe('Test Family');
    });
  });

  describe('Loading State', () => {
    it('should compute isSubmitting from familyService.isLoading', () => {
      isLoadingSignal.set(false);
      expect(component.isSubmitting()).toBe(false);

      isLoadingSignal.set(true);
      expect(component.isSubmitting()).toBe(true);
    });

    it('should show loading text when submitting', () => {
      component.isOpen = true;
      component.familyForm.controls.name.setValue('Test');
      isLoadingSignal.set(true);
      fixture.detectChanges();

      const submitButton = fixture.nativeElement.querySelector('button[type="submit"]');
      expect(submitButton.textContent).toContain('Creating...');
    });

    it('should show normal text when not submitting', () => {
      component.isOpen = true;
      component.familyForm.controls.name.setValue('Test');
      isLoadingSignal.set(false);
      fixture.detectChanges();

      const submitButton = fixture.nativeElement.querySelector('button[type="submit"]');
      expect(submitButton.textContent).toContain('Create Family');
    });
  });

  describe('Error Display', () => {
    it('should display API error from familyService', () => {
      component.isOpen = true;
      errorSignal.set('User already has a family');
      fixture.detectChanges();

      const errorElement = fixture.nativeElement.querySelector('[role="alert"]');
      expect(errorElement).toBeTruthy();
      expect(errorElement.textContent).toContain('User already has a family');
    });

    it('should not display error when familyService.error is null', () => {
      component.isOpen = true;
      errorSignal.set(null);
      fixture.detectChanges();

      const errorElement = fixture.nativeElement.querySelector('[role="alert"]');
      expect(errorElement).toBeFalsy();
    });
  });

  describe('Modal Integration', () => {
    it('should pass isOpen prop to modal', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const modal = fixture.nativeElement.querySelector('app-modal');
      expect(modal).toBeTruthy();
    });

    it('should pass title to modal', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const title = fixture.nativeElement.querySelector('.modal-title');
      expect(title.textContent).toContain('Create Your Family');
    });

    it('should set closeable to false on modal', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const closeButton = fixture.nativeElement.querySelector('.modal-close-button');
      expect(closeButton).toBeFalsy();
    });
  });

  describe('Input Component Integration', () => {
    it('should render Input component for family name', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('app-input');
      expect(input).toBeTruthy();
    });

    it('should pass maxLength of 50 to input', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input');
      expect(input.getAttribute('maxlength')).toBe('50');
    });

    it('should pass error to input component', () => {
      component.isOpen = true;
      component.familyForm.controls.name.setValue('');
      component.familyForm.controls.name.markAsTouched();
      fixture.detectChanges();

      const errorMessage = fixture.nativeElement.querySelector('.error-message');
      expect(errorMessage).toBeTruthy();
      expect(errorMessage.textContent).toContain('Family name is required');
    });
  });
});
