import { Component, Input, HostListener, inject } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { DomSanitizer } from '@angular/platform-browser';
import { ContextPanelService } from '../../services/context-panel.service';
import { ICONS } from '../../icons/icons';

@Component({
  selector: 'app-context-panel',
  standalone: true,
  imports: [NgTemplateOutlet],
  template: `
    <!-- Mobile backdrop -->
    @if (!isDesktop && panelService.isOpen()) {
      <div
        class="fixed inset-0 bg-black/30 z-30 transition-opacity duration-300"
        (click)="panelService.close()"
        data-testid="context-panel-backdrop"
      ></div>
    }

    <!-- Panel -->
    <aside
      role="complementary"
      aria-label="Detail panel"
      data-testid="context-panel"
      [class]="panelClasses()"
    >
      @if (panelService.isOpen()) {
        <!-- Header -->
        <div class="flex items-center justify-between p-4 border-b border-gray-200">
          <h2
            class="text-sm font-semibold text-gray-500 uppercase tracking-wide"
            data-testid="context-panel-header"
          >
            {{ panelService.mode() === 'create' ? 'New Event' : 'Details' }}
          </h2>
          <button
            (click)="panelService.close()"
            class="p-1 rounded-md text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
            aria-label="Close detail panel"
            data-testid="context-panel-close"
            [innerHTML]="closeIcon"
          ></button>
        </div>

        <!-- Content -->
        <div class="flex-1 overflow-y-auto" data-testid="context-panel-content">
          @if (panelService.template()) {
            <ng-container *ngTemplateOutlet="panelService.template()" />
          }
        </div>
      }
    </aside>
  `,
})
export class ContextPanelComponent {
  @Input() isDesktop = true;

  readonly panelService = inject(ContextPanelService);
  private readonly sanitizer = inject(DomSanitizer);

  readonly closeIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.CLOSE);

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.panelService.isOpen()) {
      this.panelService.close();
    }
  }

  panelClasses(): string {
    const open = this.panelService.isOpen();

    if (this.isDesktop) {
      return [
        'flex flex-col bg-white border-l border-gray-200 flex-shrink-0 overflow-hidden',
        'transition-[width] duration-300',
        open ? 'w-96' : 'w-0',
      ].join(' ');
    }

    return [
      'fixed top-0 right-0 h-screen w-96 z-30 flex flex-col bg-white border-l border-gray-200 shadow-xl',
      'transition-transform duration-300',
      open ? 'translate-x-0' : 'translate-x-full',
    ].join(' ');
  }
}
