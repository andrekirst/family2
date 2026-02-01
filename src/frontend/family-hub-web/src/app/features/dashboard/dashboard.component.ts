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

            <div class="mt-6 p-4 bg-blue-50 rounded-lg">
              <h3 class="text-lg font-medium text-gray-900">Welcome to Family Hub!</h3>
              <p class="mt-1 text-sm text-gray-600">
                You're successfully authenticated. Family features will be available soon.
              </p>
              <p class="mt-2 text-xs text-gray-500">
                Note: Family context is stored in the database and will be fetched via GraphQL
                queries.
              </p>
            </div>
          </div>
        }
      </main>
    </div>
  `,
})
export class DashboardComponent {
  private authService = inject(AuthService);

  userProfile = this.authService.userProfile;

  logout(): void {
    this.authService.logout();
  }
}
