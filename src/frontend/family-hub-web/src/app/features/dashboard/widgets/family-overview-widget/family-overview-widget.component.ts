import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardWidgetComponent } from '../../../../core/dashboard/dashboard-widget.interface';
import { UserService } from '../../../../core/user/user.service';

@Component({
  selector: 'app-family-overview-widget',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    @if (isLoading()) {
      <div class="animate-pulse space-y-2">
        <div class="h-4 bg-gray-200 rounded w-2/3"></div>
        <div class="h-4 bg-gray-200 rounded w-1/2"></div>
      </div>
    } @else if (userService.currentUser()?.familyId) {
      <div class="space-y-3">
        <p class="text-sm text-gray-600">Your family is set up and ready.</p>
        <a
          routerLink="/family/settings"
          class="inline-block px-3 py-1.5 text-sm font-medium text-blue-600 bg-blue-50 rounded-md hover:bg-blue-100"
        >
          View Members
        </a>
      </div>
    } @else {
      <div class="text-center py-4">
        <p class="text-sm text-gray-500">No family yet.</p>
        <p class="text-xs text-gray-400 mt-1">Create or join a family to see your overview.</p>
      </div>
    }
  `,
})
export class FamilyOverviewWidgetComponent implements DashboardWidgetComponent, OnInit {
  widgetConfig = signal<Record<string, unknown> | null>(null);
  isLoading = signal(true);
  userService = inject(UserService);

  ngOnInit(): void {
    // Data comes from UserService which is already loaded
    this.isLoading.set(false);
  }
}
