import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { FamilyService } from '../../../features/family/services/family.service';
import { ButtonComponent } from '../../components/atoms/button/button.component';
import { IconComponent } from '../../components/atoms/icon/icon.component';

/**
 * Main authenticated layout with sidebar navigation.
 * Used by dashboard and family management pages.
 */
@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonComponent, IconComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {
  private readonly authService = inject(AuthService);
  readonly familyService = inject(FamilyService);
  private readonly router = inject(Router);

  user = computed(() => this.authService.currentUser());

  /**
   * Navigation items for the sidebar.
   */
  navItems = [
    {
      label: 'Dashboard',
      icon: 'home',
      route: '/dashboard',
      active: false,
    },
    {
      label: 'Family',
      icon: 'users',
      route: '/family/manage',
      active: false,
    },
    {
      label: 'Profile',
      icon: 'user',
      route: '/profile',
      active: false,
    },
  ];

  /**
   * Checks if a navigation item is active based on current route.
   */
  isActive(route: string): boolean {
    return this.router.url === route;
  }

  /**
   * Signs out the current user.
   */
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
