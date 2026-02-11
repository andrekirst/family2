import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarStateService } from '../services/sidebar-state.service';
import { SidebarComponent } from './sidebar/sidebar.component';
import { TopBarComponent } from './top-bar/top-bar.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, TopBarComponent],
  template: `
    <div class="min-h-screen bg-gray-50 flex">
      <app-sidebar />
      <div
        class="flex-1 flex flex-col transition-all duration-300"
        [style.margin-left]="sidebarState.isCollapsed() ? '64px' : '240px'"
      >
        <app-top-bar />
        <main class="flex-1 overflow-auto">
          <div class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
            <router-outlet />
          </div>
        </main>
      </div>
    </div>
  `,
})
export class LayoutComponent {
  readonly sidebarState = inject(SidebarStateService);
}
