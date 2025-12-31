import { Component, computed } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { ButtonComponent } from '../../shared/components/atoms/button/button.component';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-dashboard',
    imports: [CommonModule, ButtonComponent],
    template: `
    <div class="min-h-screen bg-gray-50">
      <!-- Header -->
      <header class="bg-white shadow">
        <div class="max-w-7xl mx-auto px-4 py-4 sm:px-6 lg:px-8 flex justify-between items-center">
          <h1 class="text-2xl font-bold text-gray-900">
            Family Hub
          </h1>
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
            Welcome to Family Hub! 👋
          </h2>
    
          <div class="space-y-4">
            @if (user()) {
              <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h3 class="font-medium text-blue-900 mb-2">Your Account</h3>
                <dl class="space-y-1 text-sm">
                  <div class="flex">
                    <dt class="font-medium text-blue-900 w-32">Email:</dt>
                    <dd class="text-blue-700">{{ user()?.email }}</dd>
                  </div>
                  <div class="flex">
                    <dt class="font-medium text-blue-900 w-32">Verified:</dt>
                    <dd class="text-blue-700">
                      {{ user()?.emailVerified ? 'Yes ✓' : 'No ✗' }}
                    </dd>
                  </div>
                  <div class="flex">
                    <dt class="font-medium text-blue-900 w-32">Member Since:</dt>
                    <dd class="text-blue-700">
                      {{ user()?.createdAt | date:'medium' }}
                    </dd>
                  </div>
                </dl>
              </div>
            }
    
            <div class="bg-green-50 border border-green-200 rounded-lg p-4">
              <h3 class="font-medium text-green-900 mb-2">✅ OAuth Integration Complete</h3>
              <p class="text-sm text-green-700">
                You've successfully authenticated with Zitadel OAuth 2.0!
                This is a protected route that requires authentication.
              </p>
            </div>
    
            <div class="bg-gray-50 border border-gray-200 rounded-lg p-4">
              <h3 class="font-medium text-gray-900 mb-2">Next Steps</h3>
              <ul class="list-disc list-inside space-y-1 text-sm text-gray-700">
                <li>Calendar module integration</li>
                <li>Task management features</li>
                <li>Family groups and invitations</li>
                <li>Event chain automation</li>
              </ul>
            </div>
          </div>
        </div>
      </main>
    </div>
    `
})
export class DashboardComponent {
  user = computed(() => this.authService.currentUser());

  constructor(private authService: AuthService) {}

  logout(): void {
    if (confirm('Are you sure you want to sign out?')) {
      this.authService.logout();
    }
  }
}
