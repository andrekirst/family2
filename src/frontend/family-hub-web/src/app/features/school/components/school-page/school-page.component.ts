import { Component, inject, signal, OnInit, OnDestroy, effect, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SchoolService, StudentDto } from '../../services/school.service';
import { FamilyPermissionService } from '../../../../core/permissions/family-permission.service';
import { TopBarService } from '../../../../shared/services/top-bar.service';
import { StudentListComponent } from '../student-list/student-list.component';
import { MarkAsStudentDialogComponent } from '../mark-as-student-dialog/mark-as-student-dialog.component';

@Component({
  selector: 'app-school-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, StudentListComponent, MarkAsStudentDialogComponent],
  templateUrl: './school-page.component.html',
})
export class SchoolPageComponent implements OnInit, OnDestroy {
  private schoolService = inject(SchoolService);
  readonly permissions = inject(FamilyPermissionService);
  private topBarService = inject(TopBarService);

  students = signal<StudentDto[]>([]);
  isLoading = signal(true);
  showMarkDialog = signal(false);

  private readonly topBarEffect = effect(() => {
    const canManage = this.permissions.canManageStudents();
    this.topBarService.setConfig({
      title: $localize`:@@school.title:School`,
      actions: canManage
        ? [
            {
              id: 'mark-student',
              label: $localize`:@@school.markAsStudent:Mark as Student`,
              onClick: () => this.showMarkDialog.set(true),
              variant: 'primary',
            },
          ]
        : [],
    });
  });

  ngOnInit(): void {
    this.loadStudents();
  }

  ngOnDestroy(): void {
    this.topBarService.clear();
  }

  loadStudents(): void {
    this.isLoading.set(true);
    this.schoolService.getStudents().subscribe({
      next: (students) => {
        this.students.set(students);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  onStudentMarked(): void {
    this.showMarkDialog.set(false);
    this.loadStudents();
  }

  onDialogClosed(): void {
    this.showMarkDialog.set(false);
  }
}
