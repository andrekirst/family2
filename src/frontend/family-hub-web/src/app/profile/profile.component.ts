import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-4xl font-bold mb-6">Your Profile</h1>

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
            Full profile management coming soon. For now, you can see your basic account info.
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Profile Card -->
        <div class="lg:col-span-1">
          <div class="card bg-base-100 shadow-xl">
            <div class="card-body text-center">
              <div class="avatar placeholder mb-4">
                <div class="bg-primary text-primary-content rounded-full w-24">
                  <span class="text-3xl">{{ getInitials() }}</span>
                </div>
              </div>
              <h2 class="card-title justify-center">{{ currentUserEmail }}</h2>
              <div class="badge badge-success">Email Verified</div>
            </div>
          </div>
        </div>

        <!-- Settings Card -->
        <div class="lg:col-span-2">
          <div class="card bg-base-100 shadow-xl">
            <div class="card-body">
              <h2 class="card-title mb-4">Account Settings</h2>

              <div class="form-control w-full mb-4">
                <label class="label">
                  <span class="label-text">Email Address</span>
                </label>
                <input
                  type="text"
                  class="input input-bordered w-full"
                  [value]="currentUserEmail"
                  disabled
                />
              </div>

              <div class="divider">Coming Soon Features</div>

              <div class="space-y-4 opacity-50">
                <div class="form-control w-full">
                  <label class="label">
                    <span class="label-text">First Name</span>
                  </label>
                  <input
                    type="text"
                    placeholder="John"
                    class="input input-bordered w-full"
                    disabled
                  />
                </div>

                <div class="form-control w-full">
                  <label class="label">
                    <span class="label-text">Last Name</span>
                  </label>
                  <input
                    type="text"
                    placeholder="Doe"
                    class="input input-bordered w-full"
                    disabled
                  />
                </div>

                <div class="form-control w-full">
                  <label class="label">
                    <span class="label-text">Phone Number</span>
                  </label>
                  <input
                    type="tel"
                    placeholder="+49 123 456 7890"
                    class="input input-bordered w-full"
                    disabled
                  />
                </div>

                <div class="form-control w-full">
                  <label class="label">
                    <span class="label-text">Date of Birth</span>
                  </label>
                  <input type="date" class="input input-bordered w-full" disabled />
                </div>
              </div>

              <div class="card-actions justify-end mt-6">
                <button class="btn btn-primary" disabled>Save Changes (Coming Soon)</button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Additional Settings -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
        <div class="card bg-base-100 shadow-xl">
          <div class="card-body">
            <h3 class="card-title">Privacy Settings</h3>
            <p class="text-sm text-gray-600 mb-4">Control who can see your information</p>
            <div class="space-y-2 opacity-50">
              <div class="form-control">
                <label class="label cursor-pointer">
                  <span class="label-text">Show birthday to family</span>
                  <input type="checkbox" class="toggle toggle-primary" disabled />
                </label>
              </div>
              <div class="form-control">
                <label class="label cursor-pointer">
                  <span class="label-text">Show phone to family</span>
                  <input type="checkbox" class="toggle toggle-primary" disabled />
                </label>
              </div>
            </div>
            <div class="badge badge-warning mt-4">Coming Soon</div>
          </div>
        </div>

        <div class="card bg-base-100 shadow-xl">
          <div class="card-body">
            <h3 class="card-title">Notification Preferences</h3>
            <p class="text-sm text-gray-600 mb-4">Choose how you want to be notified</p>
            <div class="space-y-2 opacity-50">
              <div class="form-control">
                <label class="label cursor-pointer">
                  <span class="label-text">Email notifications</span>
                  <input type="checkbox" class="toggle toggle-primary" disabled />
                </label>
              </div>
              <div class="form-control">
                <label class="label cursor-pointer">
                  <span class="label-text">Push notifications</span>
                  <input type="checkbox" class="toggle toggle-primary" disabled />
                </label>
              </div>
            </div>
            <div class="badge badge-warning mt-4">Coming Soon</div>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class ProfileComponent {
  currentUserEmail: string;
  currentUserId: string;

  constructor(private auth: AuthService) {
    const user = this.auth.getCurrentUser();
    this.currentUserEmail = user?.email || 'Unknown';
    this.currentUserId = user?.id || '';
  }

  getInitials(): string {
    const email = this.currentUserEmail;
    if (email && email.includes('@')) {
      return email.substring(0, 2).toUpperCase();
    }
    return 'U';
  }
}
