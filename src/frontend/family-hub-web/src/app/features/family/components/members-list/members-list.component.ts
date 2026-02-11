import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvitationService } from '../../services/invitation.service';
import { FamilyMemberDto } from '../../models/invitation.models';

@Component({
  selector: 'app-members-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-3">
      @if (isLoading()) {
        <div class="animate-pulse space-y-3">
          <div class="h-16 bg-gray-200 rounded"></div>
          <div class="h-16 bg-gray-200 rounded"></div>
        </div>
      } @else if (members().length === 0) {
        <p class="text-gray-500 text-sm">No members found.</p>
      } @else {
        @for (member of members(); track member.id) {
          <div class="flex items-center justify-between p-4 bg-white border rounded-lg">
            <div>
              <p class="font-medium text-gray-900">{{ member.userName }}</p>
              <p class="text-sm text-gray-500">{{ member.userEmail }}</p>
            </div>
            <div class="flex items-center gap-3">
              <span
                class="px-2.5 py-0.5 text-xs font-medium rounded-full"
                [class.bg-purple-100]="member.role === 'Owner'"
                [class.text-purple-800]="member.role === 'Owner'"
                [class.bg-blue-100]="member.role === 'Admin'"
                [class.text-blue-800]="member.role === 'Admin'"
                [class.bg-gray-100]="member.role === 'Member'"
                [class.text-gray-800]="member.role === 'Member'"
              >
                {{ member.role }}
              </span>
              <span class="text-xs text-gray-400">
                Joined {{ member.joinedAt | date: 'mediumDate' }}
              </span>
            </div>
          </div>
        }
      }
    </div>
  `,
})
export class MembersListComponent implements OnInit {
  private invitationService = inject(InvitationService);

  members = signal<FamilyMemberDto[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers(): void {
    this.isLoading.set(true);
    this.invitationService.getFamilyMembers().subscribe({
      next: (members) => {
        this.members.set(members);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }
}
