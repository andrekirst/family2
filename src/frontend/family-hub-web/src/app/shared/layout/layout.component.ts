import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarStateService } from '../services/sidebar-state.service';
import { SidebarComponent } from './sidebar/sidebar.component';
import { TopBarComponent } from './top-bar/top-bar.component';
import { ContextPanelComponent } from './context-panel/context-panel.component';
import { ToastContainerComponent } from '../components/toast-container/toast-container.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    SidebarComponent,
    TopBarComponent,
    ContextPanelComponent,
    ToastContainerComponent,
  ],
  template: `
    <div class="min-h-screen bg-gray-50 flex">
      <app-sidebar />
      <div
        class="flex-1 flex flex-col transition-all duration-300 min-w-0"
        [style.margin-left]="sidebarState.isCollapsed() ? '64px' : '240px'"
      >
        <app-top-bar />
        <div class="flex-1 flex overflow-hidden">
          <main class="flex-1 overflow-hidden flex flex-col">
            <div
              class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8 flex-1 min-h-0 flex flex-col w-full"
            >
              <router-outlet />
            </div>
          </main>
          <app-context-panel [isDesktop]="isDesktop()" />
        </div>
      </div>
    </div>
    <app-toast-container />
  `,
})
export class LayoutComponent implements OnInit, OnDestroy {
  readonly sidebarState = inject(SidebarStateService);
  readonly isDesktop = signal(false);

  private mediaQuery?: MediaQueryList;
  private mediaHandler = (e: MediaQueryListEvent) => this.isDesktop.set(e.matches);

  ngOnInit(): void {
    this.mediaQuery = window.matchMedia('(min-width: 1024px)');
    this.isDesktop.set(this.mediaQuery.matches);
    this.mediaQuery.addEventListener('change', this.mediaHandler);
  }

  ngOnDestroy(): void {
    this.mediaQuery?.removeEventListener('change', this.mediaHandler);
  }
}
