import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
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
      <!-- Success Alert -->
      @if (showSuccessMessage()) {
        <div class="fixed top-4 right-4 z-50 animate-fade-in">
          <div class="bg-green-50 border-l-4 border-green-500 p-4 shadow-lg rounded">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-5 w-5 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                  <path
                    fill-rule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                    clip-rule="evenodd"
                  />
                </svg>
              </div>
              <div class="ml-3">
                <p class="text-sm font-medium text-green-800">Successfully logged in!</p>
              </div>
            </div>
          </div>
        </div>
      }

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
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  userProfile = this.authService.userProfile;
  showSuccessMessage = signal(false);

  ngOnInit(): void {
    // Check for login success query parameter
    this.route.queryParams.subscribe((params) => {
      if (params['login'] === 'success') {
        this.showSuccessMessage.set(true);

        // Auto-dismiss after 3 seconds
        setTimeout(() => {
          this.showSuccessMessage.set(false);
          // Clean up URL
          this.router.navigate([], {
            queryParams: {},
            replaceUrl: true,
          });
        }, 3000);
      }
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
