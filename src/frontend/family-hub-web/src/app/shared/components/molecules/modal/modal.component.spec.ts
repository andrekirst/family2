import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { ModalComponent } from './modal.component';

@Component({
  standalone: true,
  imports: [ModalComponent],
  template: `
    <app-modal
      [isOpen]="isOpen"
      [title]="title"
      [closeable]="closeable"
      (closeModal)="onClose()"
    >
      <div class="modal-content">Test Content</div>
    </app-modal>
  `
})
class TestHostComponent {
  isOpen = false;
  title = 'Test Modal';
  closeable = true;
  closeCount = 0;

  onClose(): void {
    this.closeCount++;
    this.isOpen = false;
  }
}

describe('ModalComponent', () => {
  let component: TestHostComponent;
  let fixture: ComponentFixture<TestHostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Visibility', () => {
    it('should not render when isOpen is false', () => {
      component.isOpen = false;
      fixture.detectChanges();

      const modal = fixture.nativeElement.querySelector('[role="dialog"]');
      expect(modal).toBeFalsy();
    });

    it('should render when isOpen is true', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const modal = fixture.nativeElement.querySelector('[role="dialog"]');
      expect(modal).toBeTruthy();
    });

    it('should show backdrop when modal is open', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const backdrop = fixture.nativeElement.querySelector('.modal-backdrop');
      expect(backdrop).toBeTruthy();
    });
  });

  describe('Title Display', () => {
    it('should display modal title', () => {
      component.isOpen = true;
      component.title = 'My Custom Title';
      fixture.detectChanges();

      const title = fixture.nativeElement.querySelector('.modal-title');
      expect(title).toBeTruthy();
      expect(title.textContent).toContain('My Custom Title');
    });

    it('should update title when changed', () => {
      component.isOpen = true;
      component.title = 'Initial Title';
      fixture.detectChanges();

      let title = fixture.nativeElement.querySelector('.modal-title');
      expect(title.textContent).toContain('Initial Title');

      component.title = 'Updated Title';
      fixture.detectChanges();

      title = fixture.nativeElement.querySelector('.modal-title');
      expect(title.textContent).toContain('Updated Title');
    });
  });

  describe('Content Projection', () => {
    it('should project ng-content into modal body', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const content = fixture.nativeElement.querySelector('.modal-content');
      expect(content).toBeTruthy();
      expect(content.textContent).toContain('Test Content');
    });
  });

  describe('Close Functionality', () => {
    it('should emit closeModal when backdrop is clicked and closeable is true', () => {
      component.isOpen = true;
      component.closeable = true;
      fixture.detectChanges();

      const backdrop = fixture.nativeElement.querySelector('.modal-backdrop');
      backdrop.click();
      fixture.detectChanges();

      expect(component.closeCount).toBe(1);
    });

    it('should NOT emit closeModal when backdrop is clicked and closeable is false', () => {
      component.isOpen = true;
      component.closeable = false;
      fixture.detectChanges();

      const backdrop = fixture.nativeElement.querySelector('.modal-backdrop');
      backdrop.click();
      fixture.detectChanges();

      expect(component.closeCount).toBe(0);
    });

    it('should emit closeModal when close button is clicked and closeable is true', () => {
      component.isOpen = true;
      component.closeable = true;
      fixture.detectChanges();

      const closeButton = fixture.nativeElement.querySelector('.modal-close-button');
      expect(closeButton).toBeTruthy();

      closeButton.click();
      fixture.detectChanges();

      expect(component.closeCount).toBe(1);
    });

    it('should NOT show close button when closeable is false', () => {
      component.isOpen = true;
      component.closeable = false;
      fixture.detectChanges();

      const closeButton = fixture.nativeElement.querySelector('.modal-close-button');
      expect(closeButton).toBeFalsy();
    });
  });

  describe('Keyboard Navigation', () => {
    it('should emit closeModal on Escape key when closeable is true', () => {
      component.isOpen = true;
      component.closeable = true;
      fixture.detectChanges();

      const modal = fixture.nativeElement.querySelector('[role="dialog"]');
      const event = new KeyboardEvent('keydown', { key: 'Escape' });
      modal.dispatchEvent(event);
      fixture.detectChanges();

      expect(component.closeCount).toBe(1);
    });

    it('should NOT emit closeModal on Escape key when closeable is false', () => {
      component.isOpen = true;
      component.closeable = false;
      fixture.detectChanges();

      const modal = fixture.nativeElement.querySelector('[role="dialog"]');
      const event = new KeyboardEvent('keydown', { key: 'Escape' });
      modal.dispatchEvent(event);
      fixture.detectChanges();

      expect(component.closeCount).toBe(0);
    });

    it('should ignore other keys', () => {
      component.isOpen = true;
      component.closeable = true;
      fixture.detectChanges();

      const modal = fixture.nativeElement.querySelector('[role="dialog"]');
      const event = new KeyboardEvent('keydown', { key: 'Enter' });
      modal.dispatchEvent(event);
      fixture.detectChanges();

      expect(component.closeCount).toBe(0);
    });
  });

  describe('Accessibility', () => {
    it('should have role="dialog"', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const modal = fixture.nativeElement.querySelector('[role="dialog"]');
      expect(modal).toBeTruthy();
    });

    it('should have aria-modal="true"', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const modal = fixture.nativeElement.querySelector('[role="dialog"]');
      expect(modal.getAttribute('aria-modal')).toBe('true');
    });

    it('should have aria-labelledby pointing to title', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const modal = fixture.nativeElement.querySelector('[role="dialog"]');
      const labelledBy = modal.getAttribute('aria-labelledby');
      expect(labelledBy).toBeTruthy();

      const title = fixture.nativeElement.querySelector(`#${labelledBy}`);
      expect(title).toBeTruthy();
      expect(title.classList.contains('modal-title')).toBe(true);
    });

    it('should have tabindex="0" for keyboard focus', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const modal = fixture.nativeElement.querySelector('[role="dialog"]');
      expect(modal.getAttribute('tabindex')).toBe('0');
    });
  });

  describe('Focus Management', () => {
    it('should focus modal container when opened', (done) => {
      component.isOpen = true;
      fixture.detectChanges();

      // Wait for ngAfterViewInit
      setTimeout(() => {
        const modal = fixture.nativeElement.querySelector('[role="dialog"]');
        expect(document.activeElement).toBe(modal);
        done();
      }, 100);
    });
  });

  describe('Styling', () => {
    it('should have overlay with proper z-index (z-50)', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const overlay = fixture.nativeElement.querySelector('.modal-overlay');
      expect(overlay.classList.contains('z-50')).toBe(true);
    });

    it('should have backdrop with semi-transparent background', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const backdrop = fixture.nativeElement.querySelector('.modal-backdrop');
      expect(backdrop.classList.contains('bg-black')).toBe(true);
      expect(backdrop.classList.contains('bg-opacity-50')).toBe(true);
    });

    it('should have modal container with white background and shadow', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const container = fixture.nativeElement.querySelector('.modal-container');
      expect(container.classList.contains('bg-white')).toBe(true);
      expect(container.classList.contains('rounded-lg')).toBe(true);
      expect(container.classList.contains('shadow-xl')).toBe(true);
    });
  });

  describe('Click Outside Handler', () => {
    it('should NOT close modal when clicking inside modal container', () => {
      component.isOpen = true;
      component.closeable = true;
      fixture.detectChanges();

      const container = fixture.nativeElement.querySelector('.modal-container');
      container.click();
      fixture.detectChanges();

      expect(component.closeCount).toBe(0);
    });

    it('should close modal when clicking backdrop (outside container)', () => {
      component.isOpen = true;
      component.closeable = true;
      fixture.detectChanges();

      const backdrop = fixture.nativeElement.querySelector('.modal-backdrop');
      backdrop.click();
      fixture.detectChanges();

      expect(component.closeCount).toBe(1);
    });
  });

  describe('Animation', () => {
    it('should have fade-in animation class', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const overlay = fixture.nativeElement.querySelector('.modal-overlay');
      expect(overlay.classList.contains('transition-opacity')).toBe(true);
      expect(overlay.classList.contains('duration-200')).toBe(true);
    });

    it('should have scale animation on modal container', () => {
      component.isOpen = true;
      fixture.detectChanges();

      const container = fixture.nativeElement.querySelector('.modal-container');
      expect(container.classList.contains('transition-transform')).toBe(true);
      expect(container.classList.contains('duration-200')).toBe(true);
    });
  });
});
