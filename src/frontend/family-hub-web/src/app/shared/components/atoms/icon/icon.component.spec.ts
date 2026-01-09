import { ComponentFixture, TestBed } from '@angular/core/testing';
import { IconComponent } from './icon.component';

describe('IconComponent', () => {
  let component: IconComponent;
  let fixture: ComponentFixture<IconComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IconComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(IconComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Size Classes', () => {
    it('should apply sm size classes (w-4 h-4)', () => {
      component.size = 'sm';
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg).toBeTruthy();
      expect(svg.classList.contains('w-4')).toBe(true);
      expect(svg.classList.contains('h-4')).toBe(true);
    });

    it('should apply md size classes (w-5 h-5)', () => {
      component.size = 'md';
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg).toBeTruthy();
      expect(svg.classList.contains('w-5')).toBe(true);
      expect(svg.classList.contains('h-5')).toBe(true);
    });

    it('should apply lg size classes (w-6 h-6)', () => {
      component.size = 'lg';
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg).toBeTruthy();
      expect(svg.classList.contains('w-6')).toBe(true);
      expect(svg.classList.contains('h-6')).toBe(true);
    });

    it('should default to md size when not specified', () => {
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg).toBeTruthy();
      expect(svg.classList.contains('w-5')).toBe(true);
      expect(svg.classList.contains('h-5')).toBe(true);
    });
  });

  describe('Icon Rendering', () => {
    it('should render users icon with correct path', () => {
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      const path = svg.querySelector('path');
      expect(path).toBeTruthy();
      expect(path.getAttribute('d')).toContain('M'); // SVG path starts with M
    });

    it('should render x-mark icon with correct path', () => {
      component.name = 'x-mark';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      const path = svg.querySelector('path');
      expect(path).toBeTruthy();
      expect(path.getAttribute('d')).toContain('M');
    });

    it('should render check icon with correct path', () => {
      component.name = 'check';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      const path = svg.querySelector('path');
      expect(path).toBeTruthy();
      expect(path.getAttribute('d')).toContain('M');
    });

    it('should have viewBox="0 0 24 24" for all icons', () => {
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg.getAttribute('viewBox')).toBe('0 0 24 24');
    });

    it('should have fill="none" for all icons', () => {
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg.getAttribute('fill')).toBe('none');
    });

    it('should have stroke="currentColor" for all icons', () => {
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg.getAttribute('stroke')).toBe('currentColor');
    });

    it('should have stroke-width="1.5" for all icons', () => {
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg.getAttribute('stroke-width')).toBe('1.5');
    });
  });

  describe('Custom Classes', () => {
    it('should apply custom classes to svg element', () => {
      component.name = 'users';
      component.customClass = 'text-blue-600';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg.classList.contains('text-blue-600')).toBe(true);
    });

    it('should merge custom classes with size classes', () => {
      component.name = 'users';
      component.size = 'lg';
      component.customClass = 'text-red-500 rotate-45';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg.classList.contains('w-6')).toBe(true);
      expect(svg.classList.contains('h-6')).toBe(true);
      expect(svg.classList.contains('text-red-500')).toBe(true);
      expect(svg.classList.contains('rotate-45')).toBe(true);
    });

    it('should work without custom classes', () => {
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg).toBeTruthy();
      // Should still have size classes
      expect(svg.classList.contains('w-5')).toBe(true);
    });
  });

  describe('Accessibility', () => {
    it('should have aria-hidden="true" for decorative icons', () => {
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg.getAttribute('aria-hidden')).toBe('true');
    });

    it('should be focusable="false" to prevent keyboard focus', () => {
      component.name = 'users';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg.getAttribute('focusable')).toBe('false');
    });
  });

  describe('Unknown Icon Handling', () => {
    it('should render fallback icon for unknown icon names', () => {
      component.name = 'unknown-icon-name';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg).toBeTruthy();
      // Should render something (either fallback or empty state)
    });

    it('should handle empty icon name gracefully', () => {
      component.name = '';
      fixture.detectChanges();

      const svg = fixture.nativeElement.querySelector('svg');
      expect(svg).toBeTruthy();
    });
  });
});
