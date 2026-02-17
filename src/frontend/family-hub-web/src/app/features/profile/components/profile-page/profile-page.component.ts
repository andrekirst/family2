import { Component, inject, signal, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TopBarService } from '../../../../shared/services/top-bar.service';
import { UserService } from '../../../../core/user/user.service';
import {
  AvatarDisplayComponent,
  AvatarUploadComponent,
  AvatarService,
} from '../../../../core/avatar';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [CommonModule, AvatarDisplayComponent, AvatarUploadComponent],
  template: `
    <div class="max-w-md">
      <h3 class="text-lg font-medium text-gray-900 mb-4">Your Avatar</h3>
      <div class="flex items-start gap-6 mb-6">
        <app-avatar-display
          [avatarId]="currentAvatarId()"
          [name]="currentUserName()"
          size="large"
        />
        <div class="flex-1 pt-2">
          <p class="text-sm text-gray-600 mb-1">{{ currentUserName() }}</p>
          <p class="text-xs text-gray-400 mb-3">{{ currentUserEmail() }}</p>
          @if (currentAvatarId()) {
            <button
              (click)="removeAvatar()"
              [disabled]="isRemovingAvatar()"
              class="text-sm text-red-600 hover:text-red-700 disabled:opacity-50"
            >
              {{ isRemovingAvatar() ? 'Removing...' : 'Remove avatar' }}
            </button>
          }
        </div>
      </div>
      <app-avatar-upload (avatarUploaded)="onAvatarUploaded($event)" />
    </div>
  `,
})
export class ProfilePageComponent implements OnDestroy {
  private readonly topBarService = inject(TopBarService);
  private readonly userService = inject(UserService);
  private readonly avatarService = inject(AvatarService);

  isRemovingAvatar = signal(false);

  currentAvatarId = () => this.userService.currentUser()?.avatarId ?? null;
  currentUserName = () => this.userService.currentUser()?.name ?? '';
  currentUserEmail = () => this.userService.currentUser()?.email ?? '';

  constructor() {
    this.topBarService.setConfig({ title: 'Profile', actions: [] });
  }

  ngOnDestroy(): void {
    this.topBarService.clear();
  }

  onAvatarUploaded(avatarId: string): void {
    // Optimistic update: immediately show the new avatar without waiting for refetch
    const user = this.userService.currentUser();
    if (user) {
      this.userService.currentUser.set({ ...user, avatarId });
    }
    // Background refetch for full profile sync
    this.userService.fetchCurrentUser().catch(() => {});
  }

  removeAvatar(): void {
    this.isRemovingAvatar.set(true);
    this.avatarService.removeAvatar().subscribe({
      next: (success) => {
        this.isRemovingAvatar.set(false);
        if (success) {
          // Optimistic update: immediately clear the avatar
          const user = this.userService.currentUser();
          if (user) {
            this.userService.currentUser.set({ ...user, avatarId: null });
          }
          this.userService.fetchCurrentUser().catch(() => {});
        }
      },
      error: () => {
        this.isRemovingAvatar.set(false);
      },
    });
  }
}
