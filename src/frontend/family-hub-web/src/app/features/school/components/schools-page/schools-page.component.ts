import { Component, inject, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SchoolService, SchoolDto } from '../../services/school.service';
import { FamilyPermissionService } from '../../../../core/permissions/family-permission.service';
import { SchoolFormDialogComponent } from '../school-form-dialog/school-form-dialog.component';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-schools-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, SchoolFormDialogComponent, ConfirmationDialogComponent],
  templateUrl: './schools-page.component.html',
})
export class SchoolsPageComponent implements OnInit {
  private schoolService = inject(SchoolService);
  readonly permissions = inject(FamilyPermissionService);

  readonly deleteConfirmTitle = $localize`:@@school.schools.deleteConfirmTitle:Delete School`;
  readonly deleteConfirmMessage = $localize`:@@school.schools.deleteConfirmMessage:Are you sure you want to delete this school? This cannot be undone.`;

  schools = signal<SchoolDto[]>([]);
  isLoading = signal(true);
  isDeleting = signal(false);

  showFormDialog = signal(false);
  editingSchool = signal<SchoolDto | null>(null);
  showDeleteConfirmation = signal(false);
  deleteError = signal<string | null>(null);
  private deletingSchoolId = signal<string | null>(null);

  ngOnInit(): void {
    this.loadSchools();
  }

  private loadSchools(): void {
    this.isLoading.set(true);
    this.schoolService.getSchools().subscribe({
      next: (schools) => {
        this.schools.set(schools);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  onEdit(school: SchoolDto): void {
    this.editingSchool.set(school);
    this.showFormDialog.set(true);
  }

  onDelete(school: SchoolDto): void {
    this.deletingSchoolId.set(school.id);
    this.deleteError.set(null);
    this.showDeleteConfirmation.set(true);
  }

  onSaved(): void {
    this.showFormDialog.set(false);
    this.editingSchool.set(null);
    this.loadSchools();
  }

  onFormDialogClosed(): void {
    this.showFormDialog.set(false);
    this.editingSchool.set(null);
  }

  onConfirmDelete(): void {
    const id = this.deletingSchoolId();
    if (!id) return;

    this.isDeleting.set(true);
    this.schoolService.deleteSchool({ schoolId: id }).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.showDeleteConfirmation.set(false);
        this.deletingSchoolId.set(null);
        this.loadSchools();
      },
      error: (err) => {
        this.isDeleting.set(false);
        this.showDeleteConfirmation.set(false);
        this.deleteError.set(
          err?.message ??
            $localize`:@@school.schools.deleteErrorGeneric:This school is in use and cannot be deleted.`,
        );
      },
    });
  }
}
