import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { UserService } from '../../core/user/user.service';
import { InvitationService } from '../family/services/invitation.service';
import { InvitationDto } from '../family/models/invitation.models';
import { CreateFamilyDialogComponent } from '../family/components/create-family-dialog/create-family-dialog.component';
import { TopBarService } from '../../shared/services/top-bar.service';

/**
 * Dashboard component - main landing page after authentication
 * Displays user profile, pending invitations, and family membership from backend
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, CreateFamilyDialogComponent],
  template: `
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
        @if (currentUser()!.familyId) {
          <div class="mt-6 p-4 bg-blue-50 rounded-lg">
            <h3 class="text-lg font-medium text-gray-900">Family Member</h3>
            <p class="mt-1 text-sm text-gray-600">
              You're part of a family. Manage your family below.
            </p>
            <a
              routerLink="/family/settings"
              class="mt-3 inline-block px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
            >
              Family Settings
            </a>
          </div>
        } @else {
          <!-- Pending Invitations Section -->
          @if (loadingInvitations()) {
            <div class="mt-6 p-4 bg-gray-50 rounded-lg">
              <div class="animate-pulse">
                <div class="h-5 bg-gray-200 rounded w-1/4 mb-3"></div>
                <div class="h-4 bg-gray-200 rounded w-1/2"></div>
              </div>
            </div>
          } @else if (pendingInvitations().length > 0) {
            <div class="mt-6">
              <h3 class="text-lg font-medium text-gray-900 mb-3">Pending Invitations</h3>
              <div class="space-y-3">
                @for (invitation of pendingInvitations(); track invitation.id) {
                  <div class="p-4 bg-blue-50 border border-blue-200 rounded-lg">
                    <div class="flex items-start justify-between">
                      <div>
                        <p class="font-medium text-gray-900">Join {{ invitation.familyName }}</p>
                        <p class="text-sm text-gray-600 mt-1">
                          Invited by <strong>{{ invitation.invitedByName }}</strong>
                        </p>
                        <div class="flex gap-4 mt-2 text-xs text-gray-500">
                          <span>Role: {{ invitation.role }}</span>
                          <span>Expires: {{ invitation.expiresAt | date: 'mediumDate' }}</span>
                        </div>
                      </div>
                      <div class="flex gap-2 flex-shrink-0 ml-4">
                        <button
                          (click)="declinePendingInvitation(invitation.id)"
                          [disabled]="processingInvitation() === invitation.id"
                          class="px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50"
                        >
                          Decline
                        </button>
                        <button
                          (click)="acceptPendingInvitation(invitation.id)"
                          [disabled]="processingInvitation() === invitation.id"
                          class="px-3 py-1.5 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
                        >
                          {{
                            processingInvitation() === invitation.id ? 'Processing...' : 'Accept'
                          }}
                        </button>
                      </div>
                    </div>
                  </div>
                }
              </div>
            </div>

            <!-- Still show create family option below invitations -->
            <div class="mt-6 p-4 bg-gray-50 rounded-lg">
              <h3 class="text-lg font-medium text-gray-900">Or Create Your Own Family</h3>
              <p class="mt-1 text-sm text-gray-600">
                Start a new family instead of joining an existing one.
              </p>
              <button
                (click)="openCreateFamilyDialog()"
                class="mt-3 px-4 py-2 bg-primary text-white rounded hover:bg-blue-600"
              >
                Create Family
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
        }
      </div>
    } @else {
      <div class="bg-red-50 border-l-4 border-red-500 p-4">
        <p class="text-red-700">Failed to load user data. Please try logging in again.</p>
      </div>
    }

    <!-- Create Family Dialog -->
    @if (showCreateFamilyDialog()) {
      <app-create-family-dialog
        (familyCreated)="onFamilyCreated()"
        (dialogClosed)="onDialogClosed()"
      />
    }
  `,
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private invitationService = inject(InvitationService);
  private topBarService = inject(TopBarService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // Use backend user data instead of Keycloak token
  currentUser = this.userService.currentUser;
  isLoading = this.userService.isLoading;
  showSuccessMessage = signal(false);
  showCreateFamilyDialog = signal(false);

  // Pending invitations state
  pendingInvitations = signal<InvitationDto[]>([]);
  loadingInvitations = signal(false);
  processingInvitation = signal<string | null>(null);

  async ngOnInit(): Promise<void> {
    this.topBarService.setConfig({ title: 'Family Hub' });

    // Fetch user data from backend
    try {
      await this.userService.fetchCurrentUser();

      // If user has no family, check for pending invitations
      if (this.currentUser() && !this.currentUser()!.familyId) {
        this.loadPendingInvitations();
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

  private loadPendingInvitations(): void {
    this.loadingInvitations.set(true);
    this.invitationService.getMyPendingInvitations().subscribe({
      next: (invitations) => {
        this.pendingInvitations.set(invitations);
        this.loadingInvitations.set(false);
      },
      error: () => {
        this.loadingInvitations.set(false);
      },
    });
  }

  acceptPendingInvitation(invitationId: string): void {
    this.processingInvitation.set(invitationId);
    this.invitationService.acceptInvitationById(invitationId).subscribe({
      next: async () => {
        // Refresh user data to get updated familyId
        await this.userService.fetchCurrentUser();
        this.processingInvitation.set(null);
      },
      error: () => {
        this.processingInvitation.set(null);
      },
    });
  }

  declinePendingInvitation(invitationId: string): void {
    this.processingInvitation.set(invitationId);
    this.invitationService.declineInvitationById(invitationId).subscribe({
      next: () => {
        // Remove from local list
        this.pendingInvitations.update((list) => list.filter((inv) => inv.id !== invitationId));
        this.processingInvitation.set(null);
      },
      error: () => {
        this.processingInvitation.set(null);
      },
    });
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
