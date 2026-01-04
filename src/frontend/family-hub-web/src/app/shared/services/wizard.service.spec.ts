import { TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { WizardService, WizardStepConfig } from './wizard.service';

// Mock step components for testing
@Component({
  selector: 'app-test-step-1',
  template: '<div>Step 1</div>',
  standalone: true
})
class TestStep1Component {}

@Component({
  selector: 'app-test-step-2',
  template: '<div>Step 2</div>',
  standalone: true
})
class TestStep2Component {}

@Component({
  selector: 'app-test-step-3',
  template: '<div>Step 3</div>',
  standalone: true
})
class TestStep3Component {}

describe('WizardService', () => {
  let service: WizardService;

  const mockSteps: WizardStepConfig[] = [
    {
      id: 'step1',
      componentType: TestStep1Component,
      title: 'First Step',
      validateOnNext: (data) => {
        const stepData = data.get('step1') as { name?: string } | undefined;
        return stepData?.name ? null : ['Name is required'];
      }
    },
    {
      id: 'step2',
      componentType: TestStep2Component,
      title: 'Second Step',
      canSkip: true
    },
    {
      id: 'step3',
      componentType: TestStep3Component,
      title: 'Third Step'
    }
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [WizardService]
    });

    service = TestBed.inject(WizardService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Initial State', () => {
    it('should initialize with currentStepIndex at 0', () => {
      expect(service.currentStepIndex()).toBe(0);
    });

    it('should initialize with empty stepsConfig array', () => {
      expect(service.stepsConfig()).toEqual([]);
    });

    it('should initialize with empty stepData Map', () => {
      expect(service.stepData().size).toBe(0);
    });

    it('should initialize with empty stepErrors Map', () => {
      expect(service.stepErrors().size).toBe(0);
    });

    it('should initialize totalSteps computed signal to 0', () => {
      expect(service.totalSteps()).toBe(0);
    });

    it('should initialize currentStep to 0', () => {
      expect(service.currentStep()).toBe(0);
    });

    it('should initialize currentStepConfig to undefined', () => {
      expect(service.currentStepConfig()).toBeUndefined();
    });

    it('should initialize isFirstStep to true', () => {
      expect(service.isFirstStep()).toBe(true);
    });

    it('should initialize isLastStep to false (no steps)', () => {
      expect(service.isLastStep()).toBe(false);
    });

    it('should initialize canGoNext to false (no steps)', () => {
      expect(service.canGoNext()).toBe(false);
    });
  });

  describe('initialize()', () => {
    it('should set stepsConfig with provided steps', () => {
      service.initialize(mockSteps);

      expect(service.stepsConfig()).toEqual(mockSteps);
    });

    it('should reset currentStepIndex to 0', () => {
      service.initialize(mockSteps);
      service.setStepData('step1', { name: 'Test' }); // Add valid data to allow navigation
      service.nextStep();
      expect(service.currentStepIndex()).toBe(1);

      service.initialize(mockSteps);
      expect(service.currentStepIndex()).toBe(0);
    });

    it('should clear stepData when initializing', () => {
      service.initialize(mockSteps);
      service.setStepData('step1', { name: 'Test' });
      expect(service.stepData().size).toBe(1);

      service.initialize(mockSteps);
      expect(service.stepData().size).toBe(0);
    });

    it('should clear stepErrors when initializing', () => {
      service.initialize(mockSteps);
      service.setStepErrors('step1', ['Error']);
      expect(service.stepErrors().size).toBe(1);

      service.initialize(mockSteps);
      expect(service.stepErrors().size).toBe(0);
    });

    it('should throw error if steps array is empty', () => {
      expect(() => service.initialize([])).toThrowError('Wizard must have at least one step');
    });

    it('should throw error if steps array is null', () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect(() => service.initialize(null as any)).toThrowError('Wizard must have at least one step');
    });

    it('should throw error if step IDs are not unique', () => {
      const duplicateSteps: WizardStepConfig[] = [
        { id: 'step1', componentType: TestStep1Component, title: 'Step 1' },
        { id: 'step1', componentType: TestStep2Component, title: 'Step 2' }
      ];

      expect(() => service.initialize(duplicateSteps)).toThrowError('Wizard step IDs must be unique');
    });
  });

  describe('Computed Signals', () => {
    beforeEach(() => {
      service.initialize(mockSteps);
    });

    it('should compute totalSteps correctly', () => {
      expect(service.totalSteps()).toBe(3);
    });

    it('should compute currentStepConfig for first step', () => {
      const config = service.currentStepConfig();
      expect(config).toBeDefined();
      expect(config?.id).toBe('step1');
      expect(config?.title).toBe('First Step');
    });

    it('should compute currentStepConfig for middle step', () => {
      service.nextStep();
      service.setStepData('step1', { name: 'Test' }); // Ensure validation passes
      service.nextStep();

      const config = service.currentStepConfig();
      expect(config?.id).toBe('step2');
    });

    it('should compute isFirstStep correctly', () => {
      expect(service.isFirstStep()).toBe(true);

      service.setStepData('step1', { name: 'Test' });
      service.nextStep();
      expect(service.isFirstStep()).toBe(false);
    });

    it('should compute isLastStep correctly', () => {
      expect(service.isLastStep()).toBe(false);

      service.goToStep(2);
      expect(service.isLastStep()).toBe(true);
    });

    it('should compute canGoNext based on validation and position', () => {
      // First step without data (validation fails)
      expect(service.canGoNext()).toBe(false);

      // First step with valid data
      service.setStepData('step1', { name: 'Test' });
      expect(service.canGoNext()).toBe(true);

      // Last step
      service.goToStep(2);
      expect(service.canGoNext()).toBe(false);
    });
  });

  describe('Navigation', () => {
    beforeEach(() => {
      service.initialize(mockSteps);
    });

    describe('nextStep()', () => {
      it('should navigate to next step when validation passes', () => {
        service.setStepData('step1', { name: 'Test' });
        service.nextStep();

        expect(service.currentStepIndex()).toBe(1);
      });

      it('should not navigate when validation fails', () => {
        // No data set, validation will fail
        service.nextStep();

        expect(service.currentStepIndex()).toBe(0);
      });

      it('should not navigate when already on last step', () => {
        service.goToStep(2);
        service.nextStep();

        expect(service.currentStepIndex()).toBe(2);
      });

      it('should validate current step before navigating', () => {
        spyOn(service, 'validateStep').and.returnValue(true);

        service.nextStep();

        expect(service.validateStep).toHaveBeenCalledWith('step1');
      });
    });

    describe('previousStep()', () => {
      it('should navigate to previous step', () => {
        service.goToStep(1);
        service.previousStep();

        expect(service.currentStepIndex()).toBe(0);
      });

      it('should not navigate when already on first step', () => {
        service.previousStep();

        expect(service.currentStepIndex()).toBe(0);
      });

      it('should not validate when going backwards', () => {
        service.goToStep(1);
        spyOn(service, 'validateStep');

        service.previousStep();

        expect(service.validateStep).not.toHaveBeenCalled();
      });
    });

    describe('goToStep()', () => {
      it('should jump to specific step index', () => {
        service.goToStep(2);

        expect(service.currentStepIndex()).toBe(2);
      });

      it('should not navigate to negative index', () => {
        service.goToStep(-1);

        expect(service.currentStepIndex()).toBe(0);
      });

      it('should not navigate to index beyond totalSteps', () => {
        service.goToStep(10);

        expect(service.currentStepIndex()).toBe(0);
      });

      it('should allow jumping to same step', () => {
        service.goToStep(0);

        expect(service.currentStepIndex()).toBe(0);
      });
    });
  });

  describe('Step Data Management', () => {
    beforeEach(() => {
      service.initialize(mockSteps);
    });

    describe('setStepData()', () => {
      it('should store step data for given step ID', () => {
        const testData = { name: 'John Doe', email: 'john@example.com' };
        service.setStepData('step1', testData);

        expect(service.stepData().get('step1')).toEqual(testData);
      });

      it('should create new Map instance for immutable update', () => {
        const originalMap = service.stepData();
        service.setStepData('step1', { name: 'Test' });
        const newMap = service.stepData();

        expect(newMap).not.toBe(originalMap);
      });

      it('should allow storing different data types', () => {
        service.setStepData('step1', { name: 'String data' });
        service.setStepData('step2', [1, 2, 3]);
        service.setStepData('step3', 42);

        expect(service.stepData().get('step1')).toEqual({ name: 'String data' });
        expect(service.stepData().get('step2')).toEqual([1, 2, 3]);
        expect(service.stepData().get('step3')).toBe(42);
      });

      it('should update existing step data', () => {
        service.setStepData('step1', { name: 'Original' });
        service.setStepData('step1', { name: 'Updated' });

        expect(service.stepData().get('step1')).toEqual({ name: 'Updated' });
      });
    });

    describe('getStepData()', () => {
      it('should retrieve stored step data', () => {
        const testData = { name: 'Jane Doe' };
        service.setStepData('step1', testData);

        const retrieved = service.getStepData<{ name: string }>('step1');
        expect(retrieved).toEqual(testData);
      });

      it('should return undefined for non-existent step', () => {
        const retrieved = service.getStepData('nonexistent');
        expect(retrieved).toBeUndefined();
      });

      it('should support type-safe retrieval', () => {
        interface Step1Data {
          name: string;
          age: number;
        }

        const testData: Step1Data = { name: 'Test', age: 30 };
        service.setStepData('step1', testData);

        const retrieved = service.getStepData<Step1Data>('step1');
        expect(retrieved?.name).toBe('Test');
        expect(retrieved?.age).toBe(30);
      });
    });
  });

  describe('Step Validation', () => {
    beforeEach(() => {
      service.initialize(mockSteps);
    });

    describe('validateStep()', () => {
      it('should return true when validation function passes', () => {
        service.setStepData('step1', { name: 'Valid Name' });

        const isValid = service.validateStep('step1');

        expect(isValid).toBe(true);
      });

      it('should return false when validation function fails', () => {
        // No data set, validation will fail
        const isValid = service.validateStep('step1');

        expect(isValid).toBe(false);
      });

      it('should set errors when validation fails', () => {
        service.validateStep('step1');

        expect(service.hasStepErrors('step1')).toBe(true);
        expect(service.getStepErrors('step1')).toEqual(['Name is required']);
      });

      it('should clear errors when validation passes', () => {
        service.setStepErrors('step1', ['Old error']);
        service.setStepData('step1', { name: 'Valid Name' });

        service.validateStep('step1');

        expect(service.hasStepErrors('step1')).toBe(false);
      });

      it('should return true for step without validation function', () => {
        const isValid = service.validateStep('step2');

        expect(isValid).toBe(true);
      });

      it('should return true for non-existent step', () => {
        const isValid = service.validateStep('nonexistent');

        expect(isValid).toBe(true);
      });

      it('should call validation function with current step data', () => {
        const validationSpy = jasmine.createSpy('validation').and.returnValue(null);
        const customSteps: WizardStepConfig[] = [
          {
            id: 'custom',
            componentType: TestStep1Component,
            title: 'Custom',
            validateOnNext: validationSpy
          }
        ];

        service.initialize(customSteps);
        service.setStepData('custom', { test: 'data' });
        service.validateStep('custom');

        expect(validationSpy).toHaveBeenCalledWith(jasmine.any(Map));
        const passedMap = validationSpy.calls.mostRecent().args[0];
        expect(passedMap.get('custom')).toEqual({ test: 'data' });
      });
    });

    describe('setStepErrors()', () => {
      it('should set errors for given step', () => {
        service.setStepErrors('step1', ['Error 1', 'Error 2']);

        expect(service.stepErrors().get('step1')).toEqual(['Error 1', 'Error 2']);
      });

      it('should create new Map instance for immutable update', () => {
        const originalMap = service.stepErrors();
        service.setStepErrors('step1', ['Error']);
        const newMap = service.stepErrors();

        expect(newMap).not.toBe(originalMap);
      });

      it('should replace existing errors for same step', () => {
        service.setStepErrors('step1', ['Old error']);
        service.setStepErrors('step1', ['New error']);

        expect(service.stepErrors().get('step1')).toEqual(['New error']);
      });
    });

    describe('clearStepErrors()', () => {
      it('should remove errors for given step', () => {
        service.setStepErrors('step1', ['Error']);
        service.clearStepErrors('step1');

        expect(service.stepErrors().has('step1')).toBe(false);
      });

      it('should create new Map instance for immutable update', () => {
        service.setStepErrors('step1', ['Error']);
        const originalMap = service.stepErrors();

        service.clearStepErrors('step1');
        const newMap = service.stepErrors();

        expect(newMap).not.toBe(originalMap);
      });

      it('should not affect other step errors', () => {
        service.setStepErrors('step1', ['Error 1']);
        service.setStepErrors('step2', ['Error 2']);

        service.clearStepErrors('step1');

        expect(service.stepErrors().has('step2')).toBe(true);
        expect(service.stepErrors().get('step2')).toEqual(['Error 2']);
      });
    });

    describe('hasStepErrors()', () => {
      it('should return true when step has errors', () => {
        service.setStepErrors('step1', ['Error']);

        expect(service.hasStepErrors('step1')).toBe(true);
      });

      it('should return false when step has no errors', () => {
        expect(service.hasStepErrors('step1')).toBe(false);
      });

      it('should return false when step has empty error array', () => {
        service.setStepErrors('step1', []);

        expect(service.hasStepErrors('step1')).toBe(false);
      });

      it('should return false for non-existent step', () => {
        expect(service.hasStepErrors('nonexistent')).toBe(false);
      });
    });

    describe('getStepErrors()', () => {
      it('should return error array for given step', () => {
        service.setStepErrors('step1', ['Error 1', 'Error 2']);

        const errors = service.getStepErrors('step1');

        expect(errors).toEqual(['Error 1', 'Error 2']);
      });

      it('should return empty array when step has no errors', () => {
        const errors = service.getStepErrors('step1');

        expect(errors).toEqual([]);
      });

      it('should return empty array for non-existent step', () => {
        const errors = service.getStepErrors('nonexistent');

        expect(errors).toEqual([]);
      });
    });
  });

  describe('reset()', () => {
    beforeEach(() => {
      service.initialize(mockSteps);
    });

    it('should reset currentStepIndex to 0', () => {
      service.goToStep(2);
      service.reset();

      expect(service.currentStepIndex()).toBe(0);
    });

    it('should clear all step data', () => {
      service.setStepData('step1', { name: 'Test' });
      service.setStepData('step2', { value: 123 });

      service.reset();

      expect(service.stepData().size).toBe(0);
    });

    it('should clear all step errors', () => {
      service.setStepErrors('step1', ['Error 1']);
      service.setStepErrors('step2', ['Error 2']);

      service.reset();

      expect(service.stepErrors().size).toBe(0);
    });

    it('should not clear stepsConfig', () => {
      service.reset();

      expect(service.stepsConfig()).toEqual(mockSteps);
    });
  });

  describe('Signal Reactivity', () => {
    beforeEach(() => {
      service.initialize(mockSteps);
    });

    it('should trigger computed signal updates when currentStepIndex changes', () => {
      const originalConfig = service.currentStepConfig();

      // Access computed to establish dependency
      service.currentStepConfig();

      service.nextStep();
      service.setStepData('step1', { name: 'Test' });
      service.nextStep();

      // Computed should have new value
      expect(service.currentStepConfig()).not.toEqual(originalConfig);
    });

    it('should trigger isFirstStep when navigation occurs', () => {
      expect(service.isFirstStep()).toBe(true);

      service.setStepData('step1', { name: 'Test' });
      service.nextStep();

      expect(service.isFirstStep()).toBe(false);
    });

    it('should trigger isLastStep when navigation occurs', () => {
      expect(service.isLastStep()).toBe(false);

      service.goToStep(2);

      expect(service.isLastStep()).toBe(true);
    });

    it('should trigger canGoNext when validation state changes', () => {
      expect(service.canGoNext()).toBe(false);

      service.setStepData('step1', { name: 'Test' });

      expect(service.canGoNext()).toBe(true);
    });
  });

  describe('Edge Cases', () => {
    it('should handle single-step wizard', () => {
      const singleStep: WizardStepConfig[] = [
        { id: 'only', componentType: TestStep1Component, title: 'Only Step' }
      ];

      service.initialize(singleStep);

      expect(service.totalSteps()).toBe(1);
      expect(service.isFirstStep()).toBe(true);
      expect(service.isLastStep()).toBe(true);
      expect(service.canGoNext()).toBe(false);
    });

    it('should handle step data of null or undefined', () => {
      service.initialize(mockSteps);

      service.setStepData('step1', null);
      expect(service.getStepData('step1')).toBeNull();

      service.setStepData('step2', undefined);
      expect(service.getStepData('step2')).toBeUndefined();
    });

    it('should handle complex nested step data', () => {
      service.initialize(mockSteps);

      const complexData = {
        user: {
          name: 'John',
          address: {
            street: '123 Main St',
            city: 'Springfield'
          },
          hobbies: ['reading', 'coding']
        }
      };

      service.setStepData('step1', complexData);
      const retrieved = service.getStepData<typeof complexData>('step1');

      expect(retrieved).toEqual(complexData);
      expect(retrieved?.user.address.city).toBe('Springfield');
    });

    it('should maintain isolation between multiple service instances', () => {
      const service1 = new WizardService();
      const service2 = new WizardService();

      service1.initialize(mockSteps);
      service2.initialize(mockSteps);

      service1.setStepData('step1', { name: 'Service 1' });
      service2.setStepData('step1', { name: 'Service 2' });

      expect(service1.getStepData('step1')).toEqual({ name: 'Service 1' });
      expect(service2.getStepData('step1')).toEqual({ name: 'Service 2' });
    });
  });

  describe('Integration Scenarios', () => {
    beforeEach(() => {
      service.initialize(mockSteps);
    });

    it('should complete full wizard flow with validation', () => {
      // Step 1: Fill data and validate
      service.setStepData('step1', { name: 'Test User' });
      expect(service.validateStep('step1')).toBe(true);
      service.nextStep();

      expect(service.currentStepIndex()).toBe(1);

      // Step 2: Skip (no validation)
      service.nextStep();
      expect(service.currentStepIndex()).toBe(2);

      // Step 3: Complete
      expect(service.isLastStep()).toBe(true);
    });

    it('should prevent navigation with invalid data', () => {
      // Try to advance without setting required data
      service.nextStep();

      expect(service.currentStepIndex()).toBe(0);
      expect(service.hasStepErrors('step1')).toBe(true);
    });

    it('should allow backward navigation without validation', () => {
      // Go forward
      service.setStepData('step1', { name: 'Test' });
      service.nextStep();
      service.nextStep();

      expect(service.currentStepIndex()).toBe(2);

      // Go backward
      service.previousStep();
      expect(service.currentStepIndex()).toBe(1);

      service.previousStep();
      expect(service.currentStepIndex()).toBe(0);
    });

    it('should maintain data integrity through navigation', () => {
      service.setStepData('step1', { name: 'Step 1 Data' });
      service.setStepData('step2', { value: 'Step 2 Data' });
      service.setStepData('step3', { info: 'Step 3 Data' });

      service.goToStep(2);
      expect(service.getStepData('step1')).toEqual({ name: 'Step 1 Data' });
      expect(service.getStepData('step2')).toEqual({ value: 'Step 2 Data' });
      expect(service.getStepData('step3')).toEqual({ info: 'Step 3 Data' });
    });
  });
});
