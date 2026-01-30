import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container mx-auto px-4 py-8">
      <!-- Hero Section -->
      <div class="hero bg-base-200 rounded-lg mb-8">
        <div class="hero-content text-center py-12">
          <div class="max-w-2xl">
            <h1 class="text-5xl font-bold">Welcome to Family Hub!</h1>
            <p class="py-6 text-lg">
              Your privacy-first family coordination platform with intelligent automation.
            </p>
            <p class="text-sm text-gray-600">
              Logged in as: <strong>{{ currentUserEmail }}</strong>
            </p>
          </div>
        </div>
      </div>

      <!-- Coming Soon Features Grid -->
      <h2 class="text-3xl font-bold mb-6">Upcoming Features</h2>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <!-- Event Chains (CORE DIFFERENTIATOR) -->
        <a
          routerLink="/events"
          class="card bg-base-100 shadow-xl hover:shadow-2xl transition-shadow cursor-pointer"
        >
          <div class="card-body">
            <div class="badge badge-primary mb-2">CORE FEATURE</div>
            <h3 class="card-title">Event Chains 🔗</h3>
            <p class="text-sm">
              Intelligent automation that no competitor offers. Automatically coordinate tasks
              across your family with zero manual work.
            </p>
            <div class="text-xs text-gray-500 mt-2">
              Example: Birthday → Reminder 1 week before → Create shopping task → Assign to parent
            </div>
            <div class="card-actions justify-end mt-4">
              <div class="badge badge-outline">Click to learn more</div>
            </div>
          </div>
        </a>

        <!-- Family Management -->
        <a
          routerLink="/family"
          class="card bg-base-100 shadow-xl hover:shadow-2xl transition-shadow cursor-pointer"
        >
          <div class="card-body">
            <div class="badge badge-secondary mb-2">FOUNDATION</div>
            <h3 class="card-title">Family Management 👨‍👩‍👧‍👦</h3>
            <p class="text-sm">
              Create and manage your family. Invite members, assign roles, and coordinate together.
            </p>
            <div class="card-actions justify-end mt-4">
              <div class="badge badge-outline">Coming Soon</div>
            </div>
          </div>
        </a>

        <!-- Shared Calendar -->
        <div class="card bg-base-100 shadow-xl">
          <div class="card-body">
            <div class="badge badge-accent mb-2">PHASE 1</div>
            <h3 class="card-title">Shared Calendar 📅</h3>
            <p class="text-sm">
              Family calendar with birthdays, appointments, and events. Everyone stays in sync.
            </p>
            <div class="card-actions justify-end mt-4">
              <div class="badge badge-outline">Coming Soon</div>
            </div>
          </div>
        </div>

        <!-- Shopping Lists -->
        <div class="card bg-base-100 shadow-xl">
          <div class="card-body">
            <div class="badge badge-accent mb-2">PHASE 1</div>
            <h3 class="card-title">Shopping Lists 🛒</h3>
            <p class="text-sm">
              Collaborative shopping lists. Add items, assign who buys what, check off completed.
            </p>
            <div class="card-actions justify-end mt-4">
              <div class="badge badge-outline">Coming Soon</div>
            </div>
          </div>
        </div>

        <!-- Task Assignments -->
        <div class="card bg-base-100 shadow-xl">
          <div class="card-body">
            <div class="badge badge-accent mb-2">PHASE 1</div>
            <h3 class="card-title">Task Management ✅</h3>
            <p class="text-sm">
              Assign chores and tasks to family members. Track completion. Fair distribution.
            </p>
            <div class="card-actions justify-end mt-4">
              <div class="badge badge-outline">Coming Soon</div>
            </div>
          </div>
        </div>

        <!-- Profile Settings -->
        <a
          routerLink="/profile"
          class="card bg-base-100 shadow-xl hover:shadow-2xl transition-shadow cursor-pointer"
        >
          <div class="card-body">
            <div class="badge badge-secondary mb-2">YOUR ACCOUNT</div>
            <h3 class="card-title">Profile Settings ⚙️</h3>
            <p class="text-sm">Manage your account, preferences, and privacy settings.</p>
            <div class="card-actions justify-end mt-4">
              <div class="badge badge-outline">Preview available</div>
            </div>
          </div>
        </a>
      </div>
    </div>
  `,
})
export class DashboardComponent {
  currentUserEmail: string;

  constructor(private auth: AuthService) {
    const user = this.auth.getCurrentUser();
    this.currentUserEmail = user?.email || 'Unknown';
  }
}
