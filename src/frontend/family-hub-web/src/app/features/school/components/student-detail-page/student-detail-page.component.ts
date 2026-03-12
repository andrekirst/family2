import {
  Component,
  inject,
  signal,
  computed,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  SchoolService,
  StudentDto,
  SchoolDto,
  SchoolYearDto,
  ClassAssignmentDto,
} from '../../services/school.service';
import { FamilyPermissionService } from '../../../../core/permissions/family-permission.service';
import { TopBarService } from '../../../../shared/services/top-bar.service';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { AssignClassDialogComponent } from '../assign-class-dialog/assign-class-dialog.component';

@Component({
  selector: 'app-student-detail-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink, ConfirmationDialogComponent, AssignClassDialogComponent],
  templateUrl: './student-detail-page.component.html',
})
export class StudentDetailPageComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private schoolService = inject(SchoolService);
  private topBarService = inject(TopBarService);
  readonly permissions = inject(FamilyPermissionService);

  readonly removeConfirmTitle = $localize`:@@school.studentDetail.removeConfirmTitle:Remove Assignment`;
  readonly removeConfirmMessage = $localize`:@@school.studentDetail.removeConfirmMessage:Are you sure you want to remove this class assignment?`;

  studentId = signal('');
  student = signal<StudentDto | null>(null);
  assignments = signal<ClassAssignmentDto[]>([]);
  schools = signal<SchoolDto[]>([]);
  schoolYears = signal<SchoolYearDto[]>([]);

  isLoading = signal(true);
  isLoadingAssignments = signal(true);
  isAssigning = signal(false);
  isRemoving = signal(false);
  assignError = signal<string | null>(null);

  showAssignDialog = signal(false);
  showRemoveConfirmation = signal(false);
  private removeAssignmentId = signal<string | null>(null);

  quickAssign = {
    schoolId: signal(''),
    schoolYearId: signal(''),
    className: signal(''),
  };

  canQuickAssign = computed(
    () =>
      this.quickAssign.schoolId() !== '' &&
      this.quickAssign.schoolYearId() !== '' &&
      this.quickAssign.className().trim() !== '',
  );

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('studentId') ?? '';
    this.studentId.set(id);

    this.topBarService.setConfig({
      title: $localize`:@@school.studentDetail.title:Student Details`,
      actions: [],
    });

    this.loadStudent();
    this.loadAssignments();
    this.loadSchools();
    this.loadSchoolYears();
  }

  ngOnDestroy(): void {
    this.topBarService.clear();
  }

  asInputValue(event: Event): string {
    return (event.target as HTMLInputElement | HTMLSelectElement).value;
  }

  private loadStudent(): void {
    this.isLoading.set(true);
    this.schoolService.getStudents().subscribe({
      next: (students) => {
        const found = students.find((s) => s.id === this.studentId()) ?? null;
        this.student.set(found);
        if (found) {
          this.topBarService.setTitle(found.memberName);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  private loadAssignments(): void {
    this.isLoadingAssignments.set(true);
    this.schoolService.getStudentClassAssignments(this.studentId()).subscribe({
      next: (assignments) => {
        this.assignments.set(assignments);
        this.isLoadingAssignments.set(false);
      },
      error: () => {
        this.isLoadingAssignments.set(false);
      },
    });
  }

  private loadSchools(): void {
    this.schoolService.getSchools().subscribe({
      next: (schools) => this.schools.set(schools),
    });
  }

  private loadSchoolYears(): void {
    this.schoolService.getSchoolYears().subscribe({
      next: (schoolYears) => this.schoolYears.set(schoolYears),
    });
  }

  onQuickAssign(): void {
    if (!this.canQuickAssign()) return;

    this.isAssigning.set(true);
    this.assignError.set(null);

    this.schoolService
      .assignStudentToClass({
        studentId: this.studentId(),
        schoolId: this.quickAssign.schoolId(),
        schoolYearId: this.quickAssign.schoolYearId(),
        className: this.quickAssign.className().trim(),
      })
      .subscribe({
        next: () => {
          this.quickAssign.schoolId.set('');
          this.quickAssign.schoolYearId.set('');
          this.quickAssign.className.set('');
          this.isAssigning.set(false);
          this.loadAssignments();
          this.loadStudent();
        },
        error: (err) => {
          this.assignError.set(
            err?.message ??
              $localize`:@@school.studentDetail.assignError:Failed to assign student to class`,
          );
          this.isAssigning.set(false);
        },
      });
  }

  onAssignmentCreated(): void {
    this.showAssignDialog.set(false);
    this.loadAssignments();
    this.loadStudent();
  }

  onRemoveAssignment(assignmentId: string): void {
    this.removeAssignmentId.set(assignmentId);
    this.showRemoveConfirmation.set(true);
  }

  onConfirmRemove(): void {
    const id = this.removeAssignmentId();
    if (!id) return;

    this.isRemoving.set(true);
    this.schoolService.removeClassAssignment({ classAssignmentId: id }).subscribe({
      next: () => {
        this.isRemoving.set(false);
        this.showRemoveConfirmation.set(false);
        this.removeAssignmentId.set(null);
        this.loadAssignments();
        this.loadStudent();
      },
      error: () => {
        this.isRemoving.set(false);
        this.showRemoveConfirmation.set(false);
      },
    });
  }
}
