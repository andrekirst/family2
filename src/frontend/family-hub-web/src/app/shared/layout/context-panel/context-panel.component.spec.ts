import { Component, ViewChild, TemplateRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ContextPanelComponent } from './context-panel.component';
import { ContextPanelService } from '../../services/context-panel.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';

@Component({
  standalone: true,
  imports: [ContextPanelComponent],
  template: `
    <app-context-panel [isDesktop]="isDesktop" />
    <ng-template #testTemplate><div data-testid="test-content">Hello</div></ng-template>
  `,
})
class TestHostComponent {
  @ViewChild('testTemplate') testTemplate!: TemplateRef<unknown>;
  isDesktop = true;
}

describe('ContextPanelComponent', () => {
  let hostFixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;
  let nativeElement: HTMLElement;
  let panelService: ContextPanelService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
      providers: [
        ContextPanelService,
        { provide: Router, useValue: { events: new Subject().asObservable() } },
      ],
    }).compileComponents();

    hostFixture = TestBed.createComponent(TestHostComponent);
    host = hostFixture.componentInstance;
    nativeElement = hostFixture.nativeElement;
    panelService = TestBed.inject(ContextPanelService);
  });

  function render(): void {
    hostFixture.detectChanges();
  }

  function queryByTestId(testId: string): HTMLElement | null {
    return nativeElement.querySelector(`[data-testid="${testId}"]`);
  }

  function openPanel(): void {
    render(); // resolve ViewChild first
    panelService.open(host.testTemplate, 'item-1');
    render();
  }

  it('should create', () => {
    expect(queryByTestId('context-panel')).toBeTruthy();
  });

  it('should have w-0 when closed on desktop', () => {
    host.isDesktop = true;
    render();

    const panel = queryByTestId('context-panel');
    expect(panel?.classList.contains('w-0')).toBe(true);
  });

  it('should have translate-x-full when closed on mobile', () => {
    host.isDesktop = false;
    render();

    const panel = queryByTestId('context-panel');
    expect(panel?.classList.contains('translate-x-full')).toBe(true);
  });

  it('should show close button and content area when open', () => {
    openPanel();

    expect(queryByTestId('context-panel-close')).toBeTruthy();
    expect(queryByTestId('context-panel-content')).toBeTruthy();
  });

  it('should render template content when open', () => {
    openPanel();

    expect(queryByTestId('test-content')?.textContent).toBe('Hello');
  });

  it('should not show close button or content when closed', () => {
    render();

    expect(queryByTestId('context-panel-close')).toBeNull();
    expect(queryByTestId('context-panel-content')).toBeNull();
  });

  it('should call close on close button click', () => {
    openPanel();

    const closeSpy = vi.spyOn(panelService, 'close');
    queryByTestId('context-panel-close')?.click();

    expect(closeSpy).toHaveBeenCalledTimes(1);
  });

  it('should close on Escape key when open', () => {
    openPanel();

    const closeSpy = vi.spyOn(panelService, 'close');
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));

    expect(closeSpy).toHaveBeenCalledTimes(1);
  });

  it('should not close on Escape key when closed', () => {
    render();

    const closeSpy = vi.spyOn(panelService, 'close');
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));

    expect(closeSpy).not.toHaveBeenCalled();
  });

  it('should show backdrop only on mobile when open', () => {
    host.isDesktop = false;
    openPanel();

    expect(queryByTestId('context-panel-backdrop')).toBeTruthy();
  });

  it('should not show backdrop on desktop when open', () => {
    host.isDesktop = true;
    openPanel();

    expect(queryByTestId('context-panel-backdrop')).toBeNull();
  });

  it('should not show backdrop when closed', () => {
    host.isDesktop = false;
    render();

    expect(queryByTestId('context-panel-backdrop')).toBeNull();
  });

  it('should close on backdrop click', () => {
    host.isDesktop = false;
    openPanel();

    const closeSpy = vi.spyOn(panelService, 'close');
    queryByTestId('context-panel-backdrop')?.click();

    expect(closeSpy).toHaveBeenCalledTimes(1);
  });

  it('should have correct accessibility attributes', () => {
    render();

    const panel = queryByTestId('context-panel');
    expect(panel?.getAttribute('role')).toBe('complementary');
    expect(panel?.getAttribute('aria-label')).toBe('Detail panel');
  });

  it('should have aria-label on close button', () => {
    openPanel();

    const closeBtn = queryByTestId('context-panel-close');
    expect(closeBtn?.getAttribute('aria-label')).toBe('Close detail panel');
  });

  it('should have w-96 when open on desktop', () => {
    host.isDesktop = true;
    openPanel();

    const panel = queryByTestId('context-panel');
    expect(panel?.classList.contains('w-96')).toBe(true);
  });

  it('should have translate-x-0 when open on mobile', () => {
    host.isDesktop = false;
    openPanel();

    const panel = queryByTestId('context-panel');
    expect(panel?.classList.contains('translate-x-0')).toBe(true);
  });
});
