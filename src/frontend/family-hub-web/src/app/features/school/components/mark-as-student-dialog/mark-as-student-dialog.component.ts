import { Component, EventEmitter, Input, Output, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SchoolService, StudentDto } from '../../services/school.service';
import { InvitationService } from '../../../family/services/invitation.service';
import { FamilyMemberDto } from '../../../family/models/invitation.models';

@Component({
  selector: 'app-mark-as-student-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mark-as-student-dialog.component.html',
})
export class MarkAsStudentDialogComponent implements OnInit {
  private schoolService = inject(SchoolService);
  private invitationService = inject(InvitationService);

  @Input() existingStudents: StudentDto[] = [];
  @Output() studentMarked = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  readonly markingLabel = $localize`:@@school.mark.marking:Marking...`;

  availableMembers = signal<FamilyMemberDto[]>([]);
  selectedMemberId = signal<string | null>(null);
  isLoadingMembers = signal(true);
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadFamilyMembers();
  }

  private loadFamilyMembers(): void {
    this.isLoadingMembers.set(true);
    const existingIds = new Set(this.existingStudents.map((s) => s.familyMemberId));

    this.invitationService.getFamilyMembers().subscribe({
      next: (members) => {
        this.availableMembers.set(members.filter((m) => !existingIds.has(m.id)));
        this.isLoadingMembers.set(false);
      },
      error: () => {
        this.errorMessage.set($localize`:@@school.mark.loadError:Failed to load family members`);
        this.isLoadingMembers.set(false);
      },
    });
  }

  selectMember(memberId: string): void {
    this.selectedMemberId.set(this.selectedMemberId() === memberId ? null : memberId);
    this.errorMessage.set(null);
  }

  onSubmit(): void {
    const memberId = this.selectedMemberId();
    if (!memberId) {
      this.errorMessage.set($localize`:@@school.mark.memberRequired:Please select a family member`);
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.schoolService.markAsStudent(memberId).subscribe({
      next: (student) => {
        if (student) {
          this.studentMarked.emit();
        } else {
          this.errorMessage.set($localize`:@@school.mark.failed:Failed to mark as student`);
        }
        this.isSubmitting.set(false);
      },
      error: () => {
        this.errorMessage.set($localize`:@@school.mark.error:An error occurred`);
        this.isSubmitting.set(false);
      },
    });
  }

  onDismiss(): void {
    this.dialogClosed.emit();
  }
}
