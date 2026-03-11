import { Component, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
import { UserService } from '../../../../core/user/user.service';
import { FamilyService } from '../../services/family.service';
import { InvitationService } from '../../services/invitation.service';
import { InvitationDto } from '../../models/invitation.models';

@Component({
  selector: 'app-family-onboarding',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="max-w-lg w-full mx-4 space-y-6">
        <!-- Header -->
        <div class="text-center">
          <h1 class="text-3xl font-bold text-gray-900" i18n="@@app.name">Family Hub</h1>
          <p class="mt-2 text-gray-600" i18n="@@family.onboarding.subtitle">
            To get started, create a family or accept an invitation.
          </p>
        </div>

        <!-- Error message -->
        @if (error()) {
          <div class="p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-800">
            {{ error() }}
          </div>
        }

        <!-- Create Family section -->
        <div class="bg-white shadow rounded-lg p-6">
          <h2
            class="text-lg font-semibold text-gray-900 mb-4"
            i18n="@@family.onboarding.createTitle"
          >
            Create a New Family
          </h2>
          <form (ngSubmit)="createFamily()" class="space-y-4">
            <div>
              <label
                for="familyName"
                class="block text-sm font-medium text-gray-700 mb-1"
                i18n="@@family.onboarding.nameLabel"
              >
                Family Name
              </label>
              <input
                id="familyName"
                type="text"
                [(ngModel)]="familyName"
                name="familyName"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
                [placeholder]="namePlaceholder"
                [disabled]="isCreating()"
                data-testid="family-name-input"
              />
            </div>
            <button
              type="submit"
              [disabled]="isCreating() || !familyName.trim()"
              class="w-full px-4 py-2.5 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              data-testid="create-family-button"
            >
              {{ isCreating() ? creatingLabel : createFamilyLabel }}
            </button>
          </form>
        </div>

        <!-- Divider -->
        @if (isLoadingInvitations() || pendingInvitations().length > 0) {
          <div class="relative">
            <div class="absolute inset-0 flex items-center">
              <div class="w-full border-t border-gray-300"></div>
            </div>
            <div class="relative flex justify-center text-sm">
              <span class="px-2 bg-gray-50 text-gray-500" i18n="@@family.onboarding.or">or</span>
            </div>
          </div>
        }

        <!-- Pending Invitations section -->
        @if (isLoadingInvitations()) {
          <div class="bg-white shadow rounded-lg p-6 text-center">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
            <p class="mt-3 text-sm text-gray-600" i18n="@@family.onboarding.loadingInvitations">
              Loading invitations...
            </p>
          </div>
        } @else if (pendingInvitations().length > 0) {
          <div class="bg-white shadow rounded-lg p-6">
            <h2
              class="text-lg font-semibold text-gray-900 mb-4"
              i18n="@@family.onboarding.invitationsTitle"
            >
              Pending Invitations
            </h2>
            <div class="space-y-3">
              @for (invitation of pendingInvitations(); track invitation.id) {
                <div
                  class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                  [attr.data-testid]="'invitation-' + invitation.id"
                >
                  <div>
                    <p class="text-sm font-medium text-gray-900">
                      {{ invitation.familyName }}
                    </p>
                    <p class="text-xs text-gray-500">
                      {{ invitedByPrefix }} {{ invitation.invitedByName }} &middot;
                      {{ invitation.role }}
                    </p>
                  </div>
                  <div class="flex gap-2">
                    <button
                      (click)="declineInvitation(invitation.id)"
                      [disabled]="processingInvitationId() === invitation.id"
                      class="px-3 py-1.5 text-xs font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 transition-colors"
                      data-testid="decline-button"
                    >
                      {{ declineLabel }}
                    </button>
                    <button
                      (click)="acceptInvitation(invitation.id)"
                      [disabled]="processingInvitationId() === invitation.id"
                      class="px-3 py-1.5 text-xs font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50 transition-colors"
                      data-testid="accept-button"
                    >
                      {{
                        processingInvitationId() === invitation.id ? processingLabel : acceptLabel
                      }}
                    </button>
                  </div>
                </div>
              }
            </div>
          </div>
        }

        <!-- Logout -->
        <div class="text-center">
          <button
            (click)="logout()"
            class="text-sm text-gray-500 hover:text-gray-700 transition-colors"
            data-testid="logout-button"
            i18n="@@nav.logout"
          >
            Logout
          </button>
        </div>
      </div>
    </div>
  `,
})
export class FamilyOnboardingComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly userService = inject(UserService);
  private readonly familyService = inject(FamilyService);
  private readonly invitationService = inject(InvitationService);

  readonly namePlaceholder = $localize`:@@family.onboarding.namePlaceholder:e.g. The Smiths`;
  readonly creatingLabel = $localize`:@@family.create.creating:Creating...`;
  readonly createFamilyLabel = $localize`:@@family.create.create:Create Family`;
  readonly invitedByPrefix = $localize`:@@family.onboarding.invitedBy:Invited by`;
  readonly declineLabel = $localize`:@@family.accept.decline:Decline`;
  readonly acceptLabel = $localize`:@@family.onboarding.accept:Accept`;
  readonly processingLabel = $localize`:@@common.processing:Processing...`;

  familyName = '';
  readonly isCreating = signal(false);
  readonly isLoadingInvitations = signal(true);
  readonly pendingInvitations = signal<InvitationDto[]>([]);
  readonly processingInvitationId = signal<string | null>(null);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadPendingInvitations();
  }

  createFamily(): void {
    const name = this.familyName.trim();
    if (!name) {
      this.error.set($localize`:@@family.create.nameRequired:Family name is required`);
      return;
    }

    this.isCreating.set(true);
    this.error.set(null);

    this.familyService.createFamily({ name }).subscribe({
      next: async (family) => {
        if (family) {
          await this.userService.fetchCurrentUser();
          await this.router.navigate(['/dashboard']);
        } else {
          this.error.set($localize`:@@family.create.failed:Failed to create family`);
        }
        this.isCreating.set(false);
      },
      error: () => {
        this.error.set($localize`:@@family.create.error:An error occurred`);
        this.isCreating.set(false);
      },
    });
  }

  acceptInvitation(id: string): void {
    this.processingInvitationId.set(id);
    this.error.set(null);

    this.invitationService.acceptInvitationById(id).subscribe({
      next: async (result) => {
        if (result?.success) {
          await this.userService.fetchCurrentUser();
          await this.router.navigate(['/dashboard']);
        } else {
          this.error.set($localize`:@@family.accept.acceptFailed:Failed to accept invitation`);
        }
        this.processingInvitationId.set(null);
      },
      error: (err) => {
        const message =
          err?.graphQLErrors?.[0]?.message ||
          err?.message ||
          $localize`:@@family.accept.acceptError:An error occurred while accepting the invitation`;
        this.error.set(message);
        this.processingInvitationId.set(null);
      },
    });
  }

  declineInvitation(id: string): void {
    this.processingInvitationId.set(id);
    this.error.set(null);

    this.invitationService.declineInvitationById(id).subscribe({
      next: () => {
        this.pendingInvitations.update((list) => list.filter((i) => i.id !== id));
        this.processingInvitationId.set(null);
      },
      error: () => {
        this.error.set(
          $localize`:@@family.accept.declineError:An error occurred while declining the invitation`,
        );
        this.processingInvitationId.set(null);
      },
    });
  }

  logout(): void {
    this.userService.clearUser();
    this.authService.logout();
  }

  private loadPendingInvitations(): void {
    this.invitationService.getMyPendingInvitations().subscribe({
      next: (invitations) => {
        this.pendingInvitations.set(invitations);
        this.isLoadingInvitations.set(false);
      },
      error: () => {
        this.isLoadingInvitations.set(false);
      },
    });
  }
}
