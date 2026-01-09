import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProgressBarComponent } from './progress-bar.component';

describe('ProgressBarComponent', () => {
  let component: ProgressBarComponent;
  let fixture: ComponentFixture<ProgressBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProgressBarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ProgressBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('progressPercentage', () => {
    it('should calculate 0% for step 1 of 4', () => {
      component.currentStep = 1;
      component.totalSteps = 4;
      expect(component.progressPercentage).toBe(0);
    });

    it('should calculate 33.33% for step 2 of 4', () => {
      component.currentStep = 2;
      component.totalSteps = 4;
      expect(component.progressPercentage).toBeCloseTo(33.33, 2);
    });

    it('should calculate 66.67% for step 3 of 4', () => {
      component.currentStep = 3;
      component.totalSteps = 4;
      expect(component.progressPercentage).toBeCloseTo(66.67, 2);
    });

    it('should calculate 100% for step 4 of 4', () => {
      component.currentStep = 4;
      component.totalSteps = 4;
      expect(component.progressPercentage).toBe(100);
    });

    it('should return 100% when totalSteps is 1', () => {
      component.currentStep = 1;
      component.totalSteps = 1;
      expect(component.progressPercentage).toBe(100);
    });
  });

  describe('stepText', () => {
    it('should return "Step 2 of 4"', () => {
      component.currentStep = 2;
      component.totalSteps = 4;
      expect(component.stepText).toBe('Step 2 of 4');
    });
  });

  describe('ariaLabel', () => {
    it('should return accessible label', () => {
      component.currentStep = 2;
      component.totalSteps = 4;
      expect(component.ariaLabel).toBe('Step 2 of 4');
    });
  });

  describe('steps', () => {
    it('should generate array [1, 2, 3, 4] for totalSteps=4', () => {
      component.totalSteps = 4;
      expect(component.steps).toEqual([1, 2, 3, 4]);
    });

    it('should generate array [1, 2, 3] for totalSteps=3', () => {
      component.totalSteps = 3;
      expect(component.steps).toEqual([1, 2, 3]);
    });
  });

  describe('getDotClasses', () => {
    beforeEach(() => {
      component.currentStep = 2;
      component.totalSteps = 4;
    });

    it('should return blue-600 for completed step (step < currentStep)', () => {
      const classes = component.getDotClasses(1);
      expect(classes).toContain('bg-blue-600');
    });

    it('should return blue-600 for current step (step === currentStep)', () => {
      const classes = component.getDotClasses(2);
      expect(classes).toContain('bg-blue-600');
    });

    it('should return gray-300 for upcoming step (step > currentStep)', () => {
      const classes = component.getDotClasses(3);
      expect(classes).toContain('bg-gray-300');
    });

    it('should include base classes for all dots', () => {
      const classes = component.getDotClasses(1);
      expect(classes).toContain('w-2');
      expect(classes).toContain('h-2');
      expect(classes).toContain('rounded-full');
      expect(classes).toContain('transition-colors');
    });
  });

  describe('getDotAriaLabel', () => {
    beforeEach(() => {
      component.currentStep = 2;
      component.totalSteps = 4;
    });

    it('should return "Step 1 completed" for completed step', () => {
      expect(component.getDotAriaLabel(1)).toBe('Step 1 completed');
    });

    it('should return "Step 2 current" for current step', () => {
      expect(component.getDotAriaLabel(2)).toBe('Step 2 current');
    });

    it('should return "Step 3 upcoming" for upcoming step', () => {
      expect(component.getDotAriaLabel(3)).toBe('Step 3 upcoming');
    });
  });

  describe('Accessibility', () => {
    it('should have role="progressbar"', () => {
      const progressBar = fixture.nativeElement.querySelector('[role="progressbar"]');
      expect(progressBar).toBeTruthy();
    });

    it('should have aria-valuenow attribute', () => {
      component.currentStep = 2;
      fixture.detectChanges();
      const progressBar = fixture.nativeElement.querySelector('[role="progressbar"]');
      expect(progressBar.getAttribute('aria-valuenow')).toBe('2');
    });

    it('should have aria-valuemin="1"', () => {
      const progressBar = fixture.nativeElement.querySelector('[role="progressbar"]');
      expect(progressBar.getAttribute('aria-valuemin')).toBe('1');
    });

    it('should have aria-valuemax attribute', () => {
      component.totalSteps = 4;
      fixture.detectChanges();
      const progressBar = fixture.nativeElement.querySelector('[role="progressbar"]');
      expect(progressBar.getAttribute('aria-valuemax')).toBe('4');
    });

    it('should have descriptive aria-label', () => {
      component.currentStep = 2;
      component.totalSteps = 4;
      fixture.detectChanges();
      const progressBar = fixture.nativeElement.querySelector('[role="progressbar"]');
      expect(progressBar.getAttribute('aria-label')).toBe('Step 2 of 4');
    });
  });

  describe('Variants', () => {
    it('should render responsive variant by default', () => {
      expect(component.variant).toBe('responsive');
    });

    it('should render linear variant when specified', () => {
      component.variant = 'linear';
      fixture.detectChanges();
      const linearBar = fixture.nativeElement.querySelector('.bg-gray-200.rounded-full.h-2');
      expect(linearBar).toBeTruthy();
    });

    it('should render dots variant when specified', () => {
      component.variant = 'dots';
      component.totalSteps = 4;
      fixture.detectChanges();
      const dots = fixture.nativeElement.querySelectorAll('.w-2.h-2.rounded-full');
      expect(dots.length).toBe(4);
    });

    it('should render both linear and dots for responsive variant', () => {
      component.variant = 'responsive';
      component.totalSteps = 4;
      fixture.detectChanges();
      const linearBar = fixture.nativeElement.querySelector('.hidden.md\\:block .bg-gray-200');
      const dotsContainer = fixture.nativeElement.querySelector('.flex.md\\:hidden');
      expect(linearBar).toBeTruthy();
      expect(dotsContainer).toBeTruthy();
    });
  });

  describe('Design System Alignment', () => {
    it('should use blue-600 for progress fill (brand primary)', () => {
      component.variant = 'linear';
      fixture.detectChanges();
      const progressFill = fixture.nativeElement.querySelector('.bg-blue-600.h-2');
      expect(progressFill).toBeTruthy();
    });

    it('should use gray-200 for background (neutral)', () => {
      component.variant = 'linear';
      fixture.detectChanges();
      const progressBg = fixture.nativeElement.querySelector('.bg-gray-200.rounded-full.h-2');
      expect(progressBg).toBeTruthy();
    });

    it('should use text-sm for step text', () => {
      component.variant = 'linear';
      fixture.detectChanges();
      const stepText = fixture.nativeElement.querySelector('.text-sm.text-gray-600');
      expect(stepText).toBeTruthy();
    });

    it('should use 8px gap between dots (space-x-2 = 0.5rem = 8px)', () => {
      component.variant = 'dots';
      fixture.detectChanges();
      const dotsContainer = fixture.nativeElement.querySelector('.space-x-2');
      expect(dotsContainer).toBeTruthy();
    });

    it('should have 8px height for progress bar (h-2 = 0.5rem = 8px)', () => {
      component.variant = 'linear';
      fixture.detectChanges();
      const progressBar = fixture.nativeElement.querySelector('.h-2.bg-gray-200');
      expect(progressBar).toBeTruthy();
    });
  });

  describe('Animation & Transitions', () => {
    it('should have transition-all duration-300 on linear bar', () => {
      component.variant = 'linear';
      fixture.detectChanges();
      const progressFill = fixture.nativeElement.querySelector('.transition-all.duration-300');
      expect(progressFill).toBeTruthy();
    });

    it('should have ease-out timing function', () => {
      component.variant = 'linear';
      fixture.detectChanges();
      const progressFill = fixture.nativeElement.querySelector('.ease-out');
      expect(progressFill).toBeTruthy();
    });

    it('should have transition-colors duration-200 on dots', () => {
      component.variant = 'dots';
      fixture.detectChanges();
      const dot = fixture.nativeElement.querySelector('.transition-colors.duration-200');
      expect(dot).toBeTruthy();
    });

    it('should respect prefers-reduced-motion', () => {
      component.variant = 'linear';
      fixture.detectChanges();
      const progressFill = fixture.nativeElement.querySelector('.motion-reduce\\:transition-none');
      expect(progressFill).toBeTruthy();
    });
  });
});
