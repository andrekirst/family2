import { Component, inject, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SchoolService, SchoolYearDto } from '../../services/school.service';
import { FamilyPermissionService } from '../../../../core/permissions/family-permission.service';
import { SchoolYearFormDialogComponent } from '../school-year-form-dialog/school-year-form-dialog.component';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-school-years-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, SchoolYearFormDialogComponent, ConfirmationDialogComponent],
  templateUrl: './school-years-page.component.html',
})
export class SchoolYearsPageComponent implements OnInit {
  private schoolService = inject(SchoolService);
  readonly permissions = inject(FamilyPermissionService);

  readonly deleteConfirmTitle = $localize`:@@school.schoolYears.deleteConfirmTitle:Delete School Year`;
  readonly deleteConfirmMessage = $localize`:@@school.schoolYears.deleteConfirmMessage:Are you sure you want to delete this school year? This cannot be undone.`;

  schoolYears = signal<SchoolYearDto[]>([]);
  isLoading = signal(true);
  isDeleting = signal(false);

  showFormDialog = signal(false);
  editingSchoolYear = signal<SchoolYearDto | null>(null);
  showDeleteConfirmation = signal(false);
  deleteError = signal<string | null>(null);
  private deletingSchoolYearId = signal<string | null>(null);

  ngOnInit(): void {
    this.loadSchoolYears();
  }

  private loadSchoolYears(): void {
    this.isLoading.set(true);
    this.schoolService.getSchoolYears().subscribe({
      next: (schoolYears) => {
        this.schoolYears.set(schoolYears);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  onEdit(schoolYear: SchoolYearDto): void {
    this.editingSchoolYear.set(schoolYear);
    this.showFormDialog.set(true);
  }

  onDelete(schoolYear: SchoolYearDto): void {
    this.deletingSchoolYearId.set(schoolYear.id);
    this.deleteError.set(null);
    this.showDeleteConfirmation.set(true);
  }

  onSaved(): void {
    this.showFormDialog.set(false);
    this.editingSchoolYear.set(null);
    this.loadSchoolYears();
  }

  onFormDialogClosed(): void {
    this.showFormDialog.set(false);
    this.editingSchoolYear.set(null);
  }

  onConfirmDelete(): void {
    const id = this.deletingSchoolYearId();
    if (!id) return;

    this.isDeleting.set(true);
    this.schoolService.deleteSchoolYear({ schoolYearId: id }).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.showDeleteConfirmation.set(false);
        this.deletingSchoolYearId.set(null);
        this.loadSchoolYears();
      },
      error: (err) => {
        this.isDeleting.set(false);
        this.showDeleteConfirmation.set(false);
        this.deleteError.set(
          err?.message ??
            $localize`:@@school.schoolYears.deleteErrorGeneric:This school year is in use and cannot be deleted.`,
        );
      },
    });
  }
}
