import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardWidgetComponent } from '../../../../core/dashboard/dashboard-widget.interface';
import { UserService } from '../../../../core/user/user.service';
import { FamilyService } from '../../../family/services/family.service';

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
        <!-- Temporary: quick create for testing -->
        <button
          (click)="createTestFamily()"
          [disabled]="isCreating()"
          class="mt-3 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50 transition-colors"
        >
          {{ isCreating() ? 'Creating...' : 'Create Family' }}
        </button>
      </div>
    }
  `,
})
export class FamilyOverviewWidgetComponent implements DashboardWidgetComponent, OnInit {
  widgetConfig = signal<Record<string, unknown> | null>(null);
  isLoading = signal(true);
  isCreating = signal(false);
  userService = inject(UserService);
  private familyService = inject(FamilyService);

  ngOnInit(): void {
    // Data comes from UserService which is already loaded
    this.isLoading.set(false);
  }

  /** Temporary: create a family for testing */
  createTestFamily(): void {
    this.isCreating.set(true);
    this.familyService.createFamily({ name: 'My Family' }).subscribe({
      next: (family) => {
        this.isCreating.set(false);
        if (family) {
          this.userService.fetchCurrentUser();
        }
      },
      error: () => this.isCreating.set(false),
    });
  }
}
