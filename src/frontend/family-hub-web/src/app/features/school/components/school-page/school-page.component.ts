import {
  Component,
  inject,
  signal,
  OnInit,
  OnDestroy,
  effect,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { SchoolService, StudentDto } from '../../services/school.service';
import { FamilyPermissionService } from '../../../../core/permissions/family-permission.service';
import { TopBarService } from '../../../../shared/services/top-bar.service';
import { MarkAsStudentDialogComponent } from '../mark-as-student-dialog/mark-as-student-dialog.component';

interface SchoolTab {
  label: string;
  path: string;
  requiresPermission: boolean;
}

@Component({
  selector: 'app-school-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, MarkAsStudentDialogComponent],
  templateUrl: './school-page.component.html',
})
export class SchoolPageComponent implements OnInit, OnDestroy {
  private schoolService = inject(SchoolService);
  readonly permissions = inject(FamilyPermissionService);
  private topBarService = inject(TopBarService);

  students = signal<StudentDto[]>([]);
  isLoading = signal(true);
  showMarkDialog = signal(false);

  readonly allTabs: SchoolTab[] = [
    {
      label: $localize`:@@school.tabs.students:Students`,
      path: 'students',
      requiresPermission: false,
    },
    {
      label: $localize`:@@school.tabs.schools:Schools`,
      path: 'schools',
      requiresPermission: true,
    },
    {
      label: $localize`:@@school.tabs.schoolYears:School Years`,
      path: 'school-years',
      requiresPermission: true,
    },
  ];

  visibleTabs = signal<SchoolTab[]>([]);

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

  private readonly permEffect = effect(() => {
    const canManageSchools = this.permissions.canManageSchools();
    this.visibleTabs.set(this.allTabs.filter((tab) => !tab.requiresPermission || canManageSchools));
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
