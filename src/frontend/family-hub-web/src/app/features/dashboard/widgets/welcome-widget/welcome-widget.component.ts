import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardWidgetComponent } from '../../../../core/dashboard/dashboard-widget.interface';
import { UserService } from '../../../../core/user/user.service';

@Component({
  selector: 'app-welcome-widget',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="h-full flex flex-col justify-center">
      @if (userService.currentUser(); as user) {
        <h2 class="text-xl font-semibold text-gray-900">Welcome, {{ user.name }}!</h2>
        <p class="mt-1 text-sm text-gray-600">{{ user.email }}</p>
        <div class="mt-3 flex gap-2">
          @if (user.familyId) {
            <a
              routerLink="/family/settings"
              class="px-3 py-1.5 text-sm font-medium text-blue-600 bg-blue-50 rounded-md hover:bg-blue-100"
            >
              Family Settings
            </a>
            <a
              routerLink="/calendar"
              class="px-3 py-1.5 text-sm font-medium text-green-600 bg-green-50 rounded-md hover:bg-green-100"
            >
              Calendar
            </a>
          }
        </div>
      } @else {
        <div class="animate-pulse">
          <div class="h-6 bg-gray-200 rounded w-1/3 mb-2"></div>
          <div class="h-4 bg-gray-200 rounded w-1/2"></div>
        </div>
      }
    </div>
  `,
})
export class WelcomeWidgetComponent implements DashboardWidgetComponent {
  widgetConfig = signal<Record<string, unknown> | null>(null);
  userService = inject(UserService);
}
