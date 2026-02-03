import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { UserService } from '../../core/user/user.service';
import { CreateFamilyDialogComponent } from '../family/components/create-family-dialog/create-family-dialog.component';

/**
 * Dashboard component - main landing page after authentication
 * Displays user profile and family membership from backend
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, CreateFamilyDialogComponent],
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
        @if (isLoading()) {
          <div class="bg-white shadow rounded-lg p-6">
            <div class="animate-pulse">
              <div class="h-6 bg-gray-200 rounded w-1/3 mb-4"></div>
              <div class="h-4 bg-gray-200 rounded w-1/2"></div>
            </div>
          </div>
        } @else if (currentUser()) {
          <div class="bg-white shadow rounded-lg p-6">
            <h2 class="text-xl font-semibold text-gray-900">Welcome, {{ currentUser()!.name }}!</h2>
            <p class="mt-2 text-gray-600">{{ currentUser()!.email }}</p>

            @if (currentUser()!.emailVerified) {
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

            <!-- Family membership display -->
            @if (currentUser()!.family) {
              <div class="mt-6 p-4 bg-blue-50 rounded-lg">
                <h3 class="text-lg font-medium text-gray-900">
                  Family: {{ currentUser()!.family!.name }}
                </h3>
                <p class="mt-1 text-sm text-gray-600">
                  You're part of {{ currentUser()!.family!.name }}. Manage your family below.
                </p>
                <button class="mt-3 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">
                  View Family
                </button>
              </div>
            } @else {
              <div class="mt-6 p-4 bg-gray-50 rounded-lg">
                <h3 class="text-lg font-medium text-gray-900">No Family Yet</h3>
                <p class="mt-1 text-sm text-gray-600">
                  Create your family to start organizing your life together.
                </p>
                <button
                  (click)="openCreateFamilyDialog()"
                  class="mt-3 px-4 py-2 bg-primary text-white rounded hover:bg-blue-600"
                >
                  Create Family
                </button>
              </div>
            }
          </div>
        } @else {
          <div class="bg-red-50 border-l-4 border-red-500 p-4">
            <p class="text-red-700">Failed to load user data. Please try logging in again.</p>
          </div>
        }
      </main>

      <!-- Create Family Dialog -->
      @if (showCreateFamilyDialog()) {
        <app-create-family-dialog
          (familyCreated)="onFamilyCreated()"
          (dialogClosed)="onDialogClosed()"
        />
      }
    </div>
  `,
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // Use backend user data instead of Keycloak token
  currentUser = this.userService.currentUser;
  isLoading = this.userService.isLoading;
  showSuccessMessage = signal(false);
  showCreateFamilyDialog = signal(false);

  async ngOnInit(): Promise<void> {
    // Fetch user data from backend
    try {
      await this.userService.fetchCurrentUser();

      // Show dialog if user has no family (optional, after delay)
      if (this.currentUser() && !this.currentUser()!.family) {
        // Small delay for better UX (user sees dashboard first)
        setTimeout(() => {
          this.showCreateFamilyDialog.set(true);
        }, 500);
      }
    } catch (error) {
      console.error('Failed to fetch user data:', error);
    }

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
    this.userService.clearUser(); // Clear backend user state
    this.authService.logout();
  }

  openCreateFamilyDialog(): void {
    this.showCreateFamilyDialog.set(true);
  }

  onFamilyCreated(): void {
    this.showCreateFamilyDialog.set(false);
    // Refresh user data to get updated family info
    this.userService.fetchCurrentUser();
  }

  onDialogClosed(): void {
    this.showCreateFamilyDialog.set(false);
  }
}
