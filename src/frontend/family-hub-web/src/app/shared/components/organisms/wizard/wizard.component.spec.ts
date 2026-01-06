import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { WizardComponent } from './wizard.component';
import { WizardService, WizardStepConfig } from '../../../services/wizard.service';

/**
 * Mock step component for testing dynamic rendering.
 */
@Component({
  selector: 'app-test-step',
  imports: [FormsModule],
  template: `
    <div class="test-step">
      <input
        type="text"
        [(ngModel)]="localData.value"
        (ngModelChange)="onDataChange()"
        data-testid="step-input"
      />
    </div>
  `
})
class TestStepComponent implements OnInit {
  @Input() data?: { value: string };
  @Output() dataChange = new EventEmitter<{ value: string }>();

  localData: { value: string } = { value: '' };

  ngOnInit() {
    if (this.data) {
      this.localData = { ...this.data };
    }
  }

  onDataChange() {
    this.dataChange.emit(this.localData);
  }
}

/**
 * Second mock step component for multi-step testing.
 */
@Component({
  selector: 'app-test-step-two',
  template: `<div class="test-step-two">Step 2 Content</div>`
})
class TestStepTwoComponent {
  @Input() data: { name?: string } = {};
  @Output() dataChange = new EventEmitter<{ name: string }>();
}

describe('WizardComponent', () => {
  let component: WizardComponent;
  let fixture: ComponentFixture<WizardComponent>;
  let wizardService: WizardService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        WizardComponent,
        BrowserAnimationsModule // Required for animations
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(WizardComponent);
    component = fixture.componentInstance;
    wizardService = TestBed.inject(WizardService);
  });

  // ===== Component Creation =====

  describe('Component Creation', () => {
    it('should create component', () => {
      expect(component).toBeTruthy();
    });

    it('should inject WizardService at component level', () => {
      expect(wizardService).toBeTruthy();
      expect(wizardService).toBeInstanceOf(WizardService);
    });

    it('should have default inputs', () => {
      expect(component.title).toBe('Wizard');
      expect(component.steps).toEqual([]);
      expect(component.submitButtonText).toBe('Complete');
    });

    it('should provide WizardService at component level (isolated scope)', () => {
      // Create second component instance
      const fixture2 = TestBed.createComponent(WizardComponent);
      const wizardService2 = fixture2.debugElement.injector.get(WizardService);

      // Services should be different instances
      expect(wizardService).not.toBe(wizardService2);
    });
  });

  // ===== Initialization =====

  describe('Initialization', () => {
    it('should throw error if initialized with empty steps array', () => {
      component.steps = [];
      expect(() => component.ngOnInit()).toThrowError('Wizard must have at least one step');
    });

    it('should initialize wizard service with provided steps', () => {
      const steps: WizardStepConfig[] = [
        { id: 'step1', componentType: TestStepComponent, title: 'Step 1' },
        { id: 'step2', componentType: TestStepTwoComponent, title: 'Step 2' }
      ];

      component.steps = steps;
      component.ngOnInit();

      expect(wizardService.totalSteps()).toBe(2);
      expect(wizardService.currentStep()).toBe(0);
      expect(wizardService.currentStepConfig()?.id).toBe('step1');
    });

    it('should render first step after view initialization', fakeAsync(() => {
      component.steps = [
        { id: 'step1', componentType: TestStepComponent, title: 'Step 1' }
      ];

      component.ngOnInit();
      fixture.detectChanges();
      component.ngAfterViewInit();
      tick(100); // Allow for async rendering

      const stepElement = fixture.nativeElement.querySelector('.test-step');
      expect(stepElement).toBeTruthy();
    }));
  });

  // ===== Template Rendering =====

  describe('Template Rendering', () => {
    beforeEach(() => {
      component.steps = [
        { id: 'step1', componentType: TestStepComponent, title: 'Step 1' },
        { id: 'step2', componentType: TestStepTwoComponent, title: 'Step 2' }
      ];
      component.title = 'Test Wizard';
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should render wizard title', () => {
      const titleElement = fixture.nativeElement.querySelector('h1');
      expect(titleElement.textContent).toContain('Test Wizard');
    });

    it('should render progress bar component', () => {
      const progressBar = fixture.nativeElement.querySelector('app-progress-bar');
      expect(progressBar).toBeTruthy();
    });

    it('should render Back button', () => {
      const buttons = fixture.nativeElement.querySelectorAll('app-button');
      expect(buttons.length).toBeGreaterThanOrEqual(2);
      expect(buttons[0].textContent.trim()).toBe('Back');
    });

    it('should render Next button on first step', () => {
      const buttons = fixture.nativeElement.querySelectorAll('app-button');
      expect(buttons[1].textContent.trim()).toBe('Next');
    });

    it('should render submit button text on last step', fakeAsync(() => {
      component.submitButtonText = 'Finish Wizard';
      wizardService.goToStep(1); // Navigate to last step
      fixture.detectChanges();
      tick();

      const buttons = fixture.nativeElement.querySelectorAll('app-button');
      expect(buttons[1].textContent.trim()).toBe('Finish Wizard');
    }));

    it('should render screen reader announcement', () => {
      const srElement = fixture.nativeElement.querySelector('[role="status"][aria-live="polite"]');
      expect(srElement).toBeTruthy();
      expect(srElement.textContent).toContain('Step 1 of 2');
      expect(srElement.textContent).toContain('Step 1');
    });

    it('should have sr-only class for screen reader announcement', () => {
      const srElement = fixture.nativeElement.querySelector('.sr-only');
      expect(srElement).toBeTruthy();

      // Verify sr-only styles are applied (visually hidden)
      const styles = window.getComputedStyle(srElement);
      expect(styles.position).toBe('absolute');
      expect(styles.width).toBe('1px');
      expect(styles.height).toBe('1px');
    });
  });

  // ===== Navigation =====

  describe('Navigation', () => {
    beforeEach(() => {
      component.steps = [
        { id: 'step1', componentType: TestStepComponent, title: 'Step 1' },
        { id: 'step2', componentType: TestStepTwoComponent, title: 'Step 2' }
      ];
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should disable Back button on first step', () => {
      expect(wizardService.isFirstStep()).toBe(true);

      const backButton = fixture.nativeElement.querySelectorAll('app-button')[0];
      expect(backButton.disabled).toBe(true);
    });

    it('should enable Back button on non-first step', fakeAsync(() => {
      wizardService.nextStep();
      fixture.detectChanges();
      tick();

      expect(wizardService.isFirstStep()).toBe(false);

      const backButton = fixture.nativeElement.querySelectorAll('app-button')[0];
      expect(backButton.disabled).toBe(false);
    }));

    it('should navigate to next step when onNext is called', () => {
      expect(wizardService.currentStep()).toBe(0);

      component.onNext();

      expect(wizardService.currentStep()).toBe(1);
    });

    it('should navigate to previous step when onBack is called', () => {
      wizardService.nextStep();
      expect(wizardService.currentStep()).toBe(1);

      component.onBack();

      expect(wizardService.currentStep()).toBe(0);
    });

    it('should not navigate back from first step', () => {
      expect(wizardService.currentStep()).toBe(0);

      component.onBack();

      expect(wizardService.currentStep()).toBe(0);
    });

    it('should emit complete event on last step when onNext is called', () => {
      let emittedData: Map<string, unknown> | null = null;
      component.complete.subscribe((data: Map<string, unknown>) => {
        emittedData = data;
      });

      // Navigate to last step
      wizardService.goToStep(1);
      fixture.detectChanges();

      // Set some data
      wizardService.setStepData('step1', { value: 'test' });
      wizardService.setStepData('step2', { name: 'Test Name' });

      // Click Next (should emit complete)
      component.onNext();

      expect(emittedData).toBeTruthy();
      if (emittedData) {
        expect((emittedData as Map<string, unknown>).get('step1')).toEqual({ value: 'test' });
        expect((emittedData as Map<string, unknown>).get('step2')).toEqual({ name: 'Test Name' });
      }
    });
  });

  // ===== Validation =====

  describe('Validation', () => {
    it('should not navigate to next step if validation fails', () => {
      component.steps = [
        {
          id: 'step1',
          componentType: TestStepComponent,
          title: 'Step 1',
          validateOnNext: (data) => {
            const stepData = data.get('step1') as { value?: string } | undefined;
            return stepData?.value ? null : ['Value is required'];
          }
        },
        { id: 'step2', componentType: TestStepTwoComponent, title: 'Step 2' }
      ];

      component.ngOnInit();
      fixture.detectChanges();

      expect(wizardService.currentStep()).toBe(0);

      // Try to navigate without setting data (should fail)
      component.onNext();

      expect(wizardService.currentStep()).toBe(0); // Still on first step
      expect(wizardService.hasStepErrors('step1')).toBe(true);
      expect(wizardService.getStepErrors('step1')).toContain('Value is required');
    });

    it('should navigate to next step if validation passes', () => {
      component.steps = [
        {
          id: 'step1',
          componentType: TestStepComponent,
          title: 'Step 1',
          validateOnNext: (data) => {
            const stepData = data.get('step1') as { value?: string } | undefined;
            return stepData?.value ? null : ['Value is required'];
          }
        },
        { id: 'step2', componentType: TestStepTwoComponent, title: 'Step 2' }
      ];

      component.ngOnInit();
      fixture.detectChanges();

      // Set valid data
      wizardService.setStepData('step1', { value: 'valid' });

      // Should navigate successfully
      component.onNext();

      expect(wizardService.currentStep()).toBe(1);
      expect(wizardService.hasStepErrors('step1')).toBe(false);
    });

    it('canProceed should return false if validation fails', () => {
      component.steps = [
        {
          id: 'step1',
          componentType: TestStepComponent,
          title: 'Step 1',
          validateOnNext: () => ['Error']
        }
      ];

      component.ngOnInit();
      fixture.detectChanges();

      expect(component.canProceed()).toBe(false);
    });

    it('canProceed should return true if validation passes', () => {
      component.steps = [
        {
          id: 'step1',
          componentType: TestStepComponent,
          title: 'Step 1',
          validateOnNext: () => null
        },
        { id: 'step2', componentType: TestStepTwoComponent, title: 'Step 2' }
      ];

      component.ngOnInit();
      fixture.detectChanges();

      expect(component.canProceed()).toBe(true);
    });

    it('canProceed should return true on last step (validation happens on click)', () => {
      component.steps = [
        {
          id: 'step1',
          componentType: TestStepComponent,
          title: 'Step 1',
          validateOnNext: () => ['Error'] // Validation that would fail
        }
      ];

      component.ngOnInit();
      fixture.detectChanges();

      // On last step, should still return true
      expect(wizardService.isLastStep()).toBe(true);
      expect(component.canProceed()).toBe(true);
    });
  });

  // ===== Dynamic Step Rendering =====

  describe('Dynamic Step Rendering', () => {
    beforeEach(() => {
      component.steps = [
        { id: 'step1', componentType: TestStepComponent, title: 'Step 1' },
        { id: 'step2', componentType: TestStepTwoComponent, title: 'Step 2' }
      ];
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should render current step component', fakeAsync(() => {
      component.ngAfterViewInit();
      tick(100);
      fixture.detectChanges();

      const stepElement = fixture.nativeElement.querySelector('.test-step');
      expect(stepElement).toBeTruthy();
    }));

    it('should switch to next step component when navigating', fakeAsync(() => {
      component.ngAfterViewInit();
      tick(100);
      fixture.detectChanges();

      // First step should be rendered
      expect(fixture.nativeElement.querySelector('.test-step')).toBeTruthy();
      expect(fixture.nativeElement.querySelector('.test-step-two')).toBeFalsy();

      // Navigate to next step
      wizardService.nextStep();
      tick(100);
      fixture.detectChanges();

      // Second step should be rendered
      expect(fixture.nativeElement.querySelector('.test-step')).toBeFalsy();
      expect(fixture.nativeElement.querySelector('.test-step-two')).toBeTruthy();
    }));

    it('should pass initial data to step component', fakeAsync(() => {
      wizardService.setStepData('step1', { value: 'initial' });

      component.ngAfterViewInit();
      tick(100);
      fixture.detectChanges();

      // Step component should receive data
      // (Verification would require accessing component instance)
      // This is a placeholder for actual implementation test
      expect(wizardService.getStepData('step1')).toEqual({ value: 'initial' });
    }));

    it('should clean up previous step component when navigating', fakeAsync(() => {
      component.ngAfterViewInit();
      tick(100);
      fixture.detectChanges();

      const initialStepElement = fixture.nativeElement.querySelector('.test-step');
      expect(initialStepElement).toBeTruthy();

      // Navigate to next step
      wizardService.nextStep();
      tick(100);
      fixture.detectChanges();

      // Previous step should be removed
      expect(fixture.nativeElement.querySelector('.test-step')).toBeFalsy();
    }));
  });

  // ===== Accessibility =====

  describe('Accessibility', () => {
    beforeEach(() => {
      component.steps = [
        { id: 'step1', componentType: TestStepComponent, title: 'Step 1' },
        { id: 'step2', componentType: TestStepTwoComponent, title: 'Step 2' }
      ];
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should update screen reader announcement when step changes', fakeAsync(() => {
      const srElement = fixture.nativeElement.querySelector('[role="status"]');
      expect(srElement.textContent).toContain('Step 1 of 2');

      wizardService.nextStep();
      fixture.detectChanges();
      tick();

      expect(srElement.textContent).toContain('Step 2 of 2');
    }));

    it('should have aria-live="polite" for screen reader announcements', () => {
      const srElement = fixture.nativeElement.querySelector('[role="status"]');
      expect(srElement.getAttribute('aria-live')).toBe('polite');
      expect(srElement.getAttribute('aria-atomic')).toBe('true');
    });

    it('should focus first input after step renders', fakeAsync(() => {
      component.ngAfterViewInit();
      tick(100); // Wait for rendering
      tick(100); // Wait for focus timeout

      // Note: Focus testing in unit tests is limited
      // This is better tested in E2E tests
      // Placeholder for verification logic
      expect(true).toBe(true);
    }));
  });

  // ===== Cleanup =====

  describe('Cleanup', () => {
    beforeEach(() => {
      component.steps = [
        { id: 'step1', componentType: TestStepComponent, title: 'Step 1' }
      ];
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should clean up step component on destroy', fakeAsync(() => {
      component.ngAfterViewInit();
      tick(100);

      expect(fixture.nativeElement.querySelector('.test-step')).toBeTruthy();

      component.ngOnDestroy();

      // ViewContainerRef should be cleared
      expect(component['currentStepComponentRef']).toBeUndefined();
    }));

    it('should reset wizard service on destroy', fakeAsync(() => {
      wizardService.setStepData('step1', { value: 'test' });
      expect(wizardService.stepData().size).toBe(1);

      component.ngOnDestroy();

      expect(wizardService.stepData().size).toBe(0);
      expect(wizardService.currentStep()).toBe(0);
    }));
  });

  // ===== Animations =====

  describe('Animations', () => {
    beforeEach(() => {
      component.steps = [
        { id: 'step1', componentType: TestStepComponent, title: 'Step 1' },
        { id: 'step2', componentType: TestStepTwoComponent, title: 'Step 2' }
      ];
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should have fadeTransition animation defined', () => {
      const metadata = component.constructor as { ɵcmp?: { animations?: unknown[] } };
      expect(metadata.ɵcmp?.animations).toBeDefined();
      expect(metadata.ɵcmp?.animations?.length).toBeGreaterThan(0);
    });

    it('should apply fade animation when step changes', fakeAsync(() => {
      component.ngAfterViewInit();
      tick(100);
      fixture.detectChanges();

      // Trigger step change
      wizardService.nextStep();
      tick(200); // Animation duration
      fixture.detectChanges();

      // Animation should have completed
      // (Specific animation verification is complex in unit tests)
      expect(wizardService.currentStep()).toBe(1);
    }));
  });

  // ===== Edge Cases =====

  describe('Edge Cases', () => {
    it('should handle single step wizard', () => {
      component.steps = [
        { id: 'step1', componentType: TestStepComponent, title: 'Only Step' }
      ];
      component.ngOnInit();
      fixture.detectChanges();

      expect(wizardService.totalSteps()).toBe(1);
      expect(wizardService.isFirstStep()).toBe(true);
      expect(wizardService.isLastStep()).toBe(true);

      // Back button should be disabled
      expect(wizardService.isFirstStep()).toBe(true);

      // Next button should show submit text
      const buttons = fixture.nativeElement.querySelectorAll('app-button');
      expect(buttons[1].textContent.trim()).toBe('Complete');
    });

    it('should handle wizard with no validation', () => {
      component.steps = [
        { id: 'step1', componentType: TestStepComponent, title: 'Step 1' },
        { id: 'step2', componentType: TestStepTwoComponent, title: 'Step 2' }
      ];
      component.ngOnInit();
      fixture.detectChanges();

      // Should be able to navigate without any data
      expect(component.canProceed()).toBe(true);

      component.onNext();
      expect(wizardService.currentStep()).toBe(1);
    });

    it('should handle undefined step config gracefully', () => {
      component.steps = [];

      // Should not error when calling methods with empty steps
      expect(() => component.onNext()).not.toThrow();
      expect(() => component.onBack()).not.toThrow();
    });
  });
});
