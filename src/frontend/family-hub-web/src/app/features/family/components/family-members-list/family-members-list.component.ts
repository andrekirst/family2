import { Component, Input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FamilyMember, UserRole } from '../../models/family.models';
import { RoleBadgeComponent } from '../role-badge/role-badge.component';

/**
 * Displays a list of family members with their roles and status.
 * Shows email verification status and provides visual role badges.
 *
 * @example
 * ```html
 * <app-family-members-list
 *   [members]="familyMembers()"
 *   [currentUserRole]="currentUserRole()" />
 * ```
 */
@Component({
  selector: 'app-family-members-list',
  standalone: true,
  imports: [CommonModule, RoleBadgeComponent],
  templateUrl: './family-members-list.component.html',
  styleUrl: './family-members-list.component.scss',
})
export class FamilyMembersListComponent {
  /**
   * List of family members to display.
   */
  @Input({ required: true }) set members(value: FamilyMember[]) {
    this._members.set(value);
  }

  /**
   * Current user's role for permission checks (future use).
   */
  @Input() currentUserRole?: UserRole;

  /**
   * Internal signal for members.
   */
  private _members = signal<FamilyMember[]>([]);

  /**
   * Sorted members: Owner → Admin → Member.
   * Within each role, sort alphabetically by email.
   */
  sortedMembers = computed(() => {
    const roleOrder: Record<UserRole, number> = {
      OWNER: 1,
      ADMIN: 2,
      MEMBER: 3,
    };

    return [...this._members()].sort((a, b) => {
      // First, sort by role priority
      const roleDiff = roleOrder[a.role] - roleOrder[b.role];
      if (roleDiff !== 0) return roleDiff;

      // Within same role, sort alphabetically by email
      return a.email.localeCompare(b.email);
    });
  });

  /**
   * Checks if a member has a verified email.
   */
  isEmailVerified(member: FamilyMember): boolean {
    return member.emailVerified;
  }

  /**
   * Gets display name for a member (email prefix as fallback).
   */
  getDisplayName(member: FamilyMember): string {
    return member.email.split('@')[0]; // Use email prefix as display name
  }
}
