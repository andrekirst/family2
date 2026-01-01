import { Component, computed, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { FamilyService } from '../family/services/family.service';
import { ButtonComponent } from '../../shared/components/atoms/button/button.component';
import { CreateFamilyModalComponent } from '../family/components/create-family-modal/create-family-modal.component';
import { IconComponent } from '../../shared/components/atoms/icon/icon.component';

/**
 * Dashboard component - main authenticated landing page.
 * Shows CreateFamilyModal when user has no family, otherwise shows family dashboard.
 */
@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    ButtonComponent,
    CreateFamilyModalComponent,
    IconComponent
  ],
  template: `
    <!-- Create Family Modal (shown when no family) -->
    <app-create-family-modal
      [isOpen]="!familyService.hasFamily()"
      (success)="onFamilyCreated()"
    ></app-create-family-modal>

    <!-- Authenticated Dashboard (shown when has family) -->
    @if (familyService.hasFamily()) {
      <div class="min-h-screen bg-gray-50">
      <!-- Header -->
      <header class="bg-white shadow">
        <div class="max-w-7xl mx-auto px-4 py-4 sm:px-6 lg:px-8 flex justify-between items-center">
          <div class="flex items-center space-x-3">
            <app-icon name="users" size="lg" customClass="text-blue-600"></app-icon>
            <div>
              <h1 class="text-2xl font-bold text-gray-900">
                {{ familyService.currentFamily()?.name }}
              </h1>
              <p class="text-sm text-gray-600">
                {{ familyService.currentFamily()?.memberCount }} member(s)
              </p>
            </div>
          </div>
          <div class="flex items-center space-x-4">
            @if (user()) {
              <span class="text-gray-700">
                {{ user()?.email }}
              </span>
            }
            <app-button
              variant="tertiary"
              size="sm"
              (clicked)="logout()"
              >
              Sign Out
            </app-button>
          </div>
        </div>
      </header>
    
      <!-- Main Content -->
      <main class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-xl font-semibold text-gray-900 mb-4">
            Welcome to your Family Hub! 👋
          </h2>
    
          <div class="space-y-4">
            <!-- Family Info Card -->
            <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
              <h3 class="font-medium text-blue-900 mb-2">Your Family</h3>
              <dl class="space-y-1 text-sm">
                <div class="flex">
                  <dt class="font-medium text-blue-900 w-32">Family Name:</dt>
                  <dd class="text-blue-700">{{ familyService.currentFamily()?.name }}</dd>
                </div>
                <div class="flex">
                  <dt class="font-medium text-blue-900 w-32">Members:</dt>
                  <dd class="text-blue-700">
                    {{ familyService.currentFamily()?.memberCount }}
                  </dd>
                </div>
                <div class="flex">
                  <dt class="font-medium text-blue-900 w-32">Created:</dt>
                  <dd class="text-blue-700">
                    {{ familyService.currentFamily()?.createdAt | date:'medium' }}
                  </dd>
                </div>
              </dl>
            </div>

            <!-- User Account Card -->
            @if (user()) {
              <div class="bg-green-50 border border-green-200 rounded-lg p-4">
                <h3 class="font-medium text-green-900 mb-2">Your Account</h3>
                <dl class="space-y-1 text-sm">
                  <div class="flex">
                    <dt class="font-medium text-green-900 w-32">Email:</dt>
                    <dd class="text-green-700">{{ user()?.email }}</dd>
                  </div>
                  <div class="flex">
                    <dt class="font-medium text-green-900 w-32">Verified:</dt>
                    <dd class="text-green-700">
                      {{ user()?.emailVerified ? 'Yes ✓' : 'No ✗' }}
                    </dd>
                  </div>
                </dl>
              </div>
            }

            <!-- Next Steps -->
            <div class="bg-gray-50 border border-gray-200 rounded-lg p-4">
              <h3 class="font-medium text-gray-900 mb-2">Coming Soon</h3>
              <ul class="list-disc list-inside space-y-1 text-sm text-gray-700">
                <li>Family member invitations</li>
                <li>Calendar module integration</li>
                <li>Task management features</li>
                <li>Event chain automation</li>
              </ul>
            </div>
          </div>
        </div>
      </main>
      </div>
    }

    <!-- Loading Overlay -->
    @if (familyService.isLoading()) {
      <div class="fixed inset-0 bg-black bg-opacity-25 flex items-center justify-center z-40">
        <div class="bg-white rounded-lg p-6 shadow-xl">
          <svg class="animate-spin h-8 w-8 text-blue-600 mx-auto" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <p class="mt-2 text-gray-600">Loading...</p>
        </div>
      </div>
    }
  `,
})
export class DashboardComponent implements OnInit {
  authService = inject(AuthService);
  familyService = inject(FamilyService);

  user = computed(() => this.authService.currentUser());

  ngOnInit(): void {
    // Load user's families on dashboard init
    this.familyService.loadUserFamilies();
  }

  onFamilyCreated(): void {
    // Modal will auto-close via hasFamily() computed signal
    console.log('Family created successfully!');
  }

  logout(): void {
    if (confirm('Are you sure you want to sign out?')) {
      this.authService.logout();
    }
  }
}
