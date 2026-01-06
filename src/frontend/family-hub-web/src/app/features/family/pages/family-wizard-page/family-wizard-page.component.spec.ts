import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { FamilyWizardPageComponent } from './family-wizard-page.component';
import { FamilyService } from '../../services/family.service';
import { WizardComponent } from '../../../../shared/components/organisms/wizard/wizard.component';
import { FamilyNameStepData } from '../../components/family-name-step/family-name-step.component';
import { signal, WritableSignal } from '@angular/core';
import { By } from '@angular/platform-browser';

describe('FamilyWizardPageComponent', () => {
  let component: FamilyWizardPageComponent;
  let fixture: ComponentFixture<FamilyWizardPageComponent>;
  let mockFamilyService: jasmine.SpyObj<FamilyService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let hasFamilySignal: WritableSignal<boolean>;
  let errorSignal: WritableSignal<string | null>;
  let isLoadingSignal: WritableSignal<boolean>;

  beforeEach(async () => {
    // Create writable signals
    hasFamilySignal = signal(false);
    errorSignal = signal(null);
    isLoadingSignal = signal(false);

    // Create mock FamilyService with signals
    mockFamilyService = jasmine.createSpyObj('FamilyService', ['createFamily'], {
      hasFamily: hasFamilySignal,
      error: errorSignal,
      isLoading: isLoadingSignal,
      currentFamily: signal(null)
    });

    // Create mock Router
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [FamilyWizardPageComponent, WizardComponent],
      providers: [
        { provide: FamilyService, useValue: mockFamilyService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FamilyWizardPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Wizard Configuration', () => {
    it('should configure wizard with family name step', () => {
      expect(component.wizardSteps.length).toBe(1);
      expect(component.wizardSteps[0].id).toBe('family-name');
      expect(component.wizardSteps[0].title).toBe('Family Name');
    });

    it('should have validation function for family name step', () => {
      const step = component.wizardSteps[0];
      expect(step.validateOnNext).toBeDefined();
    });

    it('should validate required family name', () => {
      const step = component.wizardSteps[0];
      const stepData = new Map<string, unknown>();

      // Test missing data
      const errors1 = step.validateOnNext!(stepData);
      expect(errors1).toEqual(['Family name is required.']);

      // Test empty name
      stepData.set('family-name', { name: '' } as FamilyNameStepData);
      const errors2 = step.validateOnNext!(stepData);
      expect(errors2).toEqual(['Family name is required.']);
    });

    it('should validate whitespace-only family name', () => {
      const step = component.wizardSteps[0];
      const stepData = new Map<string, unknown>();
      stepData.set('family-name', { name: '   ' } as FamilyNameStepData);

      const errors = step.validateOnNext!(stepData);
      expect(errors).toEqual(['Family name cannot be only whitespace.']);
    });

    it('should validate max length of 50 characters', () => {
      const step = component.wizardSteps[0];
      const stepData = new Map<string, unknown>();
      const longName = 'a'.repeat(51);
      stepData.set('family-name', { name: longName } as FamilyNameStepData);

      const errors = step.validateOnNext!(stepData);
      expect(errors).toEqual(['Family name must be 50 characters or less.']);
    });

    it('should accept valid family name', () => {
      const step = component.wizardSteps[0];
      const stepData = new Map<string, unknown>();
      stepData.set('family-name', { name: 'Smith Family' } as FamilyNameStepData);

      const errors = step.validateOnNext!(stepData);
      expect(errors).toBeNull();
    });

    it('should accept family name with 50 characters', () => {
      const step = component.wizardSteps[0];
      const stepData = new Map<string, unknown>();
      const fiftyCharName = 'a'.repeat(50);
      stepData.set('family-name', { name: fiftyCharName } as FamilyNameStepData);

      const errors = step.validateOnNext!(stepData);
      expect(errors).toBeNull();
    });
  });

  describe('ngOnInit', () => {
    it('should redirect to dashboard if user already has family', () => {
      // Set hasFamily to return true
      hasFamilySignal.set(true);

      component.ngOnInit();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should not redirect if user has no family', () => {
      // hasFamily is false by default
      component.ngOnInit();

      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  describe('onWizardComplete', () => {
    it('should create family with trimmed name on completion', async () => {
      const stepData = new Map<string, unknown>();
      stepData.set('family-name', { name: '  Smith Family  ' } as FamilyNameStepData);

      mockFamilyService.createFamily.and.returnValue(Promise.resolve());
      errorSignal.set(null);

      await component.onWizardComplete(stepData);

      expect(mockFamilyService.createFamily).toHaveBeenCalledWith('Smith Family');
    });

    it('should navigate to dashboard on successful creation', async () => {
      const stepData = new Map<string, unknown>();
      stepData.set('family-name', { name: 'Johnson Family' } as FamilyNameStepData);

      mockFamilyService.createFamily.and.returnValue(Promise.resolve());
      errorSignal.set(null);

      await component.onWizardComplete(stepData);

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should not navigate if family creation fails', async () => {
      const stepData = new Map<string, unknown>();
      stepData.set('family-name', { name: 'Test Family' } as FamilyNameStepData);

      mockFamilyService.createFamily.and.returnValue(Promise.resolve());
      errorSignal.set('User already has a family');

      await component.onWizardComplete(stepData);

      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should handle missing family name data', async () => {
      const stepData = new Map<string, unknown>();
      // No family-name data

      await component.onWizardComplete(stepData);

      expect(mockFamilyService.createFamily).not.toHaveBeenCalled();
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should handle empty family name after trimming', async () => {
      const stepData = new Map<string, unknown>();
      stepData.set('family-name', { name: '   ' } as FamilyNameStepData);

      await component.onWizardComplete(stepData);

      expect(mockFamilyService.createFamily).not.toHaveBeenCalled();
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should handle undefined family name', async () => {
      const stepData = new Map<string, unknown>();
      stepData.set('family-name', { name: undefined } as { name: undefined });

      await component.onWizardComplete(stepData);

      expect(mockFamilyService.createFamily).not.toHaveBeenCalled();
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  describe('Template Rendering', () => {
    it('should render WizardComponent', () => {
      const wizard = fixture.debugElement.query(By.directive(WizardComponent));
      expect(wizard).toBeTruthy();
    });

    it('should pass correct title to wizard', () => {
      const wizard = fixture.debugElement.query(By.directive(WizardComponent));
      expect(wizard.componentInstance.title).toBe('Create Your Family');
    });

    it('should pass correct submit button text', () => {
      const wizard = fixture.debugElement.query(By.directive(WizardComponent));
      expect(wizard.componentInstance.submitButtonText).toBe('Create Family');
    });

    it('should pass wizard steps configuration', () => {
      const wizard = fixture.debugElement.query(By.directive(WizardComponent));
      expect(wizard.componentInstance.steps).toEqual(component.wizardSteps);
    });
  });

  describe('Integration', () => {
    it('should complete full wizard flow', async () => {
      const stepData = new Map<string, unknown>();
      stepData.set('family-name', { name: 'Complete Family' } as FamilyNameStepData);

      mockFamilyService.createFamily.and.returnValue(Promise.resolve());
      errorSignal.set(null);

      await component.onWizardComplete(stepData);

      expect(mockFamilyService.createFamily).toHaveBeenCalledWith('Complete Family');
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });
  });
});
