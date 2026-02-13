import { Component, inject, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { SidebarStateService } from '../../services/sidebar-state.service';
import { AuthService } from '../../../core/auth/auth.service';
import { UserService } from '../../../core/user/user.service';
import { ICONS } from '../../icons/icons';

interface NavItem {
  path: string;
  label: string;
  icon: SafeHtml;
  matchPrefix: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <aside
      class="fixed top-0 left-0 h-screen bg-white border-r border-gray-200 flex flex-col z-40 transition-all duration-300"
      [style.width]="sidebarState.isCollapsed() ? '64px' : '240px'"
      data-testid="sidebar"
    >
      <!-- Header -->
      <div class="h-16 flex items-center border-b border-gray-200 px-4 flex-shrink-0">
        @if (!sidebarState.isCollapsed()) {
          <span class="text-lg font-bold text-gray-900 truncate flex-1">Family Hub</span>
        }
        <button
          (click)="sidebarState.toggle()"
          class="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500 hover:text-gray-700 transition-colors"
          [class.ml-auto]="!sidebarState.isCollapsed()"
          [class.mx-auto]="sidebarState.isCollapsed()"
          data-testid="sidebar-toggle"
          [attr.aria-label]="sidebarState.isCollapsed() ? 'Expand sidebar' : 'Collapse sidebar'"
        >
          <span [innerHTML]="sidebarState.isCollapsed() ? icons.MENU : icons.CHEVRON_LEFT"></span>
        </button>
      </div>

      <!-- Navigation -->
      <nav class="flex-1 py-4 overflow-y-auto">
        @for (item of navItems; track item.path) {
          <a
            [routerLink]="item.path"
            class="flex items-center gap-3 mx-2 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors"
            [class.bg-blue-50]="isActive(item.matchPrefix)"
            [class.text-blue-600]="isActive(item.matchPrefix)"
            [class.text-gray-700]="!isActive(item.matchPrefix)"
            [class.hover:bg-gray-100]="!isActive(item.matchPrefix)"
            [class.justify-center]="sidebarState.isCollapsed()"
            [attr.title]="sidebarState.isCollapsed() ? item.label : null"
            [attr.data-testid]="'nav-' + item.label.toLowerCase()"
          >
            <span [innerHTML]="item.icon" class="flex-shrink-0"></span>
            @if (!sidebarState.isCollapsed()) {
              <span class="truncate">{{ item.label }}</span>
            }
          </a>
        }
      </nav>

      <!-- User section -->
      <div class="border-t border-gray-200 p-3 flex-shrink-0 relative">
        <button
          (click)="toggleUserMenu($event)"
          class="flex items-center gap-3 w-full px-3 py-2.5 rounded-lg text-sm text-gray-700 hover:bg-gray-100 transition-colors"
          [class.justify-center]="sidebarState.isCollapsed()"
          [class.bg-gray-100]="showUserMenu()"
          [attr.title]="sidebarState.isCollapsed() ? userName() : null"
          data-testid="sidebar-user"
        >
          <span [innerHTML]="icons.USER_CIRCLE" class="flex-shrink-0"></span>
          @if (!sidebarState.isCollapsed()) {
            <span class="truncate">{{ userName() }}</span>
          }
        </button>
        @if (showUserMenu()) {
          <div
            class="absolute bottom-full left-2 right-2 mb-2 bg-white border border-gray-200 rounded-lg shadow-lg overflow-hidden"
            data-testid="user-menu"
          >
            <button
              (click)="logout()"
              class="flex items-center gap-2 w-full px-3 py-2.5 text-sm text-gray-700 hover:bg-gray-100 transition-colors"
              data-testid="logout-button"
            >
              <span [innerHTML]="icons.LOGOUT" class="flex-shrink-0"></span>
              <span>Logout</span>
            </button>
          </div>
        }
      </div>
    </aside>
  `,
})
export class SidebarComponent {
  readonly sidebarState = inject(SidebarStateService);
  private readonly authService = inject(AuthService);
  private readonly userService = inject(UserService);
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);

  readonly icons = {
    MENU: this.trustHtml(ICONS.MENU),
    CHEVRON_LEFT: this.trustHtml(ICONS.CHEVRON_LEFT),
    USER_CIRCLE: this.trustHtml(ICONS.USER_CIRCLE),
    LOGOUT: this.trustHtml(ICONS.LOGOUT),
  };
  readonly showUserMenu = signal(false);

  readonly navItems: NavItem[] = [
    {
      path: '/dashboard',
      label: 'Dashboard',
      icon: this.trustHtml(ICONS.HOME),
      matchPrefix: '/dashboard',
    },
    {
      path: '/family/settings',
      label: 'Family',
      icon: this.trustHtml(ICONS.USERS),
      matchPrefix: '/family',
    },
    {
      path: '/calendar',
      label: 'Calendar',
      icon: this.trustHtml(ICONS.CALENDAR),
      matchPrefix: '/calendar',
    },
    {
      path: '/event-chains',
      label: 'Automations',
      icon: this.trustHtml(ICONS.BOLT),
      matchPrefix: '/event-chains',
    },
    {
      path: '/settings',
      label: 'Settings',
      icon: this.trustHtml(ICONS.SETTINGS),
      matchPrefix: '/settings',
    },
  ];

  private trustHtml(html: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }

  userName(): string {
    return this.userService.currentUser()?.name ?? 'User';
  }

  isActive(prefix: string): boolean {
    return this.router.url.startsWith(prefix);
  }

  toggleUserMenu(event: Event): void {
    event.stopPropagation();
    this.showUserMenu.update((v) => !v);
  }

  logout(): void {
    this.showUserMenu.set(false);
    this.userService.clearUser();
    this.authService.logout();
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    this.showUserMenu.set(false);
  }
}
