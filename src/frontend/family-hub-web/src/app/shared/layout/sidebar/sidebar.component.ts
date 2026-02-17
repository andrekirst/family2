import { Component, inject, signal, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { SidebarStateService } from '../../services/sidebar-state.service';
import { AuthService } from '../../../core/auth/auth.service';
import { UserService } from '../../../core/user/user.service';
import { I18nService } from '../../../core/i18n/i18n.service';
import { ICONS } from '../../icons/icons';
import { AvatarDisplayComponent } from '../../../core/avatar';

interface NavItem {
  path: string;
  label: string;
  icon: SafeHtml;
  matchPrefix: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, AvatarDisplayComponent],
  template: `
    <aside
      class="fixed top-0 left-0 h-screen bg-white border-r border-gray-200 flex flex-col z-40 transition-all duration-300"
      [style.width]="sidebarState.isCollapsed() ? '64px' : '240px'"
      data-testid="sidebar"
    >
      <!-- Header -->
      <div class="h-16 flex items-center border-b border-gray-200 px-4 flex-shrink-0">
        @if (!sidebarState.isCollapsed()) {
          <span class="text-lg font-bold text-gray-900 truncate flex-1" i18n="@@app.name"
            >Family Hub</span
          >
        }
        <button
          (click)="sidebarState.toggle()"
          class="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500 hover:text-gray-700 transition-colors"
          [class.ml-auto]="!sidebarState.isCollapsed()"
          [class.mx-auto]="sidebarState.isCollapsed()"
          data-testid="sidebar-toggle"
          [attr.aria-label]="sidebarState.isCollapsed() ? expandSidebarLabel : collapseSidebarLabel"
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
          [attr.aria-haspopup]="true"
          [attr.aria-expanded]="showUserMenu()"
          data-testid="sidebar-user"
        >
          <app-avatar-display
            [avatarId]="userAvatarId()"
            [name]="userName()"
            size="tiny"
            class="flex-shrink-0"
          />
          @if (!sidebarState.isCollapsed()) {
            <span class="truncate">{{ userName() }}</span>
          }
        </button>
        @if (showUserMenu()) {
          <div
            class="absolute bottom-full mb-2 bg-white border border-gray-200 rounded-lg shadow-lg overflow-hidden"
            [class.left-2]="!sidebarState.isCollapsed()"
            [class.right-2]="!sidebarState.isCollapsed()"
            [class.left-0]="sidebarState.isCollapsed()"
            [style.min-width]="sidebarState.isCollapsed() ? '220px' : 'auto'"
            role="menu"
            data-testid="user-menu"
          >
            <!-- Identity header -->
            <div class="px-3 py-2.5 border-b border-gray-100" data-testid="user-menu-header">
              <p class="text-xs text-gray-500" data-testid="user-menu-signed-in-label">
                {{ signedInAsLabel }}
              </p>
              <p class="text-sm font-semibold text-gray-900 truncate" data-testid="user-menu-name">
                {{ userName() }}
              </p>
              <p class="text-xs text-gray-500 truncate" data-testid="user-menu-email">
                {{ userEmail() }}
              </p>
            </div>
            <!-- Settings -->
            <a
              routerLink="/settings"
              (click)="showUserMenu.set(false)"
              class="flex items-center gap-2 w-full px-3 py-2.5 text-sm text-gray-700 hover:bg-gray-100 transition-colors"
              role="menuitem"
              data-testid="settings-link"
            >
              <span [innerHTML]="icons.SETTINGS" class="flex-shrink-0"></span>
              <span class="flex-1" i18n="@@nav.settings">Settings</span>
              <span
                class="text-xs font-medium text-gray-500 bg-gray-100 px-1.5 py-0.5 rounded"
                data-testid="locale-badge"
                >{{ currentLocaleLabel() }}</span
              >
            </a>
            <!-- Divider -->
            <div class="border-t border-gray-100"></div>
            <!-- Logout -->
            <button
              (click)="navigateToProfile()"
              class="flex items-center gap-2 w-full px-3 py-2.5 text-sm text-gray-700 hover:bg-gray-100 transition-colors"
              data-testid="profile-button"
            >
              <span [innerHTML]="icons.USER_CIRCLE" class="flex-shrink-0"></span>
              <span>Profile</span>
            </button>
            <button
              (click)="logout()"
              class="flex items-center gap-2 w-full px-3 py-2.5 text-sm text-gray-700 hover:bg-gray-100 transition-colors"
              role="menuitem"
              data-testid="logout-button"
            >
              <span [innerHTML]="icons.LOGOUT" class="flex-shrink-0"></span>
              <span i18n="@@nav.logout">Logout</span>
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
  private readonly i18nService = inject(I18nService);
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);

  readonly icons = {
    MENU: this.trustHtml(ICONS.MENU),
    CHEVRON_LEFT: this.trustHtml(ICONS.CHEVRON_LEFT),
    USER_CIRCLE: this.trustHtml(ICONS.USER_CIRCLE),
    LOGOUT: this.trustHtml(ICONS.LOGOUT),
    SETTINGS: this.trustHtml(ICONS.SETTINGS),
  };
  readonly showUserMenu = signal(false);
  readonly userAvatarId = computed(() => this.userService.currentUser()?.avatarId ?? null);

  readonly expandSidebarLabel = $localize`:@@nav.expandSidebar:Expand sidebar`;
  readonly collapseSidebarLabel = $localize`:@@nav.collapseSidebar:Collapse sidebar`;
  readonly signedInAsLabel = $localize`:@@userMenu.signedInAs:Signed in as`;

  readonly navItems: NavItem[] = [
    {
      path: '/dashboard',
      label: $localize`:@@nav.dashboard:Dashboard`,
      icon: this.trustHtml(ICONS.HOME),
      matchPrefix: '/dashboard',
    },
    {
      path: '/family/settings',
      label: $localize`:@@nav.family:Family`,
      icon: this.trustHtml(ICONS.USERS),
      matchPrefix: '/family',
    },
    {
      path: '/calendar',
      label: $localize`:@@nav.calendar:Calendar`,
      icon: this.trustHtml(ICONS.CALENDAR),
      matchPrefix: '/calendar',
    },
    {
      path: '/event-chains',
      label: $localize`:@@nav.automations:Automations`,
      icon: this.trustHtml(ICONS.BOLT),
      matchPrefix: '/event-chains',
    },
  ];

  private trustHtml(html: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }

  userName(): string {
    return this.userService.currentUser()?.name ?? 'User';
  }

  userEmail(): string {
    return this.userService.currentUser()?.email ?? '';
  }

  currentLocaleLabel(): string {
    return this.i18nService.currentLocale().toUpperCase();
  }

  isActive(prefix: string): boolean {
    return this.router.url.startsWith(prefix);
  }

  toggleUserMenu(event: Event): void {
    event.stopPropagation();
    this.showUserMenu.update((v) => !v);
  }

  navigateToProfile(): void {
    this.showUserMenu.set(false);
    this.router.navigate(['/profile']);
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
