import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-family-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-4xl font-bold mb-6">Your Families</h1>

      <div class="alert alert-info mb-6">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          class="stroke-current shrink-0 w-6 h-6"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
          ></path>
        </svg>
        <div>
          <h3 class="font-bold">Feature Mockup</h3>
          <div class="text-sm">
            This page will allow you to create and manage your families. Invite members, assign
            roles, and coordinate together.
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div class="card bg-base-100 shadow-xl">
        <div class="card-body text-center py-12">
          <div class="text-6xl mb-4">👨‍👩‍👧‍👦</div>
          <h2 class="text-2xl font-bold mb-2">Create Your First Family</h2>
          <p class="text-gray-600 mb-6">
            Get started by creating a family. You'll be able to invite members, assign roles
            (Parent, Child, Guardian), and start coordinating together.
          </p>

          <div class="mockup-window border border-base-300 bg-base-200 max-w-lg mx-auto">
            <div class="px-6 py-4">
              <p class="text-sm font-semibold mb-3">What you'll be able to do:</p>
              <ul class="text-left space-y-2 text-sm">
                <li>✅ Create unlimited families (extended family, close family, etc.)</li>
                <li>✅ Invite members via email</li>
                <li>✅ Assign roles with different permissions</li>
                <li>✅ Manage family settings and preferences</li>
                <li>✅ View all family members in one place</li>
              </ul>
            </div>
          </div>

          <div class="mt-8">
            <button class="btn btn-primary btn-lg" disabled>Create Family (Coming Soon)</button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class FamilyListComponent {}
