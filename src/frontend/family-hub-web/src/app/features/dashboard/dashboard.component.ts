import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

/**
 * Dashboard component - main landing page after authentication
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="min-h-screen bg-gray-50">
      <!-- Header -->
      <header class="bg-white shadow">
        <div class="max-w-7xl mx-auto px-4 py-4 sm:px-6 lg:px-8 flex justify-between items-center">
          <h1 class="text-2xl font-bold text-gray-900">Family Hub</h1>
          <button (click)="logout()" class="px-4 py-2 text-sm text-gray-700 hover:text-gray-900">
            Logout
          </button>
        </div>
      </header>

      <!-- Main content -->
      <main class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
        @if (userProfile()) {
          <div class="bg-white shadow rounded-lg p-6">
            <h2 class="text-xl font-semibold text-gray-900">Welcome, {{ userProfile()!.name }}!</h2>
            <p class="mt-2 text-gray-600">{{ userProfile()!.email }}</p>

            @if (userProfile()!.emailVerified) {
              <span
                class="mt-2 inline-block px-2 py-1 text-xs font-medium text-green-700 bg-green-100 rounded"
              >
                Email Verified
              </span>
            } @else {
              <span
                class="mt-2 inline-block px-2 py-1 text-xs font-medium text-yellow-700 bg-yellow-100 rounded"
              >
                Email Not Verified
              </span>
            }

            @if (hasFamily()) {
              <div class="mt-6">
                <h3 class="text-lg font-medium text-gray-900">Your Family</h3>
                <p class="mt-1 text-sm text-gray-600">Family ID: {{ userProfile()!.familyId }}</p>
                <p class="text-sm text-gray-600">Role: {{ userProfile()!.familyRole }}</p>
                <a
                  routerLink="/family"
                  class="mt-4 inline-block px-4 py-2 bg-primary text-white rounded hover:bg-blue-600"
                >
                  View Family
                </a>
              </div>
            } @else {
              <div class="mt-6 p-4 bg-blue-50 rounded-lg">
                <h3 class="text-lg font-medium text-gray-900">Create Your Family</h3>
                <p class="mt-1 text-sm text-gray-600">
                  You haven't created a family yet. Create one to start organizing your household.
                </p>
                <button
                  (click)="createFamily()"
                  class="mt-4 px-4 py-2 bg-primary text-white rounded hover:bg-blue-600"
                >
                  Create Family
                </button>
              </div>
            }
          </div>
        }
      </main>
    </div>
  `,
})
export class DashboardComponent {
  private authService = inject(AuthService);

  userProfile = this.authService.userProfile;
  hasFamily = computed(() => !!this.userProfile()?.familyId);

  logout(): void {
    this.authService.logout();
  }

  createFamily(): void {
    // This will be implemented when we add family creation
    console.log('Create family - to be implemented');
  }
}
