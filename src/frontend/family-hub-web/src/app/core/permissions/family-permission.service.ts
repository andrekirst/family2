import { Injectable, computed, inject } from '@angular/core';
import { UserService } from '../user/user.service';

@Injectable({ providedIn: 'root' })
export class FamilyPermissionService {
  private userService = inject(UserService);

  private permissions = computed(() => this.userService.currentUser()?.permissions ?? []);

  canInvite = computed(() => this.permissions().includes('family:invite'));
  canRevokeInvitation = computed(() => this.permissions().includes('family:revoke-invitation'));
  canRemoveMembers = computed(() => this.permissions().includes('family:remove-members'));
  canEditFamily = computed(() => this.permissions().includes('family:edit'));
  canDeleteFamily = computed(() => this.permissions().includes('family:delete'));
  canManageRoles = computed(() => this.permissions().includes('family:manage-roles'));

  hasPermission(permission: string): boolean {
    return this.permissions().includes(permission);
  }
}
