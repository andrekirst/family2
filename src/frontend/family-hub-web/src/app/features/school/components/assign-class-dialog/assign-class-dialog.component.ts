import {
  Component,
  EventEmitter,
  Input,
  Output,
  signal,
  computed,
  inject,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { SchoolService, SchoolDto, SchoolYearDto } from '../../services/school.service';

@Component({
  selector: 'app-assign-class-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule],
  template: `
    <div
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
      (click)="onDismiss()"
    >
      <div
        class="bg-white rounded-lg shadow-xl w-full max-w-md mx-4"
        (click)="$event.stopPropagation()"
      >
        <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200">
          <h2 class="text-lg font-semibold text-gray-900" i18n="@@school.assignClass.title">
            Assign Student to Class
          </h2>
          <button
            class="text-gray-400 hover:text-gray-600 text-xl"
            (click)="onDismiss()"
            aria-label="Close"
          >
            &times;
          </button>
        </div>

        <div class="px-6 py-4 space-y-4">
          <!-- School -->
          <div>
            <label
              class="block text-sm font-medium text-gray-700 mb-1"
              i18n="@@school.assignClass.school"
              >School</label
            >
            <select
              class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
              [value]="selectedSchoolId()"
              (change)="selectedSchoolId.set(asInputValue($event))"
              [disabled]="isSubmitting()"
              data-testid="assign-school-picker"
            >
              <option value="" i18n="@@school.assignClass.selectSchool">-- Select school --</option>
              @for (s of schools; track s.id) {
                <option [value]="s.id">{{ s.name }}</option>
              }
            </select>
          </div>

          <!-- School Year -->
          <div>
            <label
              class="block text-sm font-medium text-gray-700 mb-1"
              i18n="@@school.assignClass.schoolYear"
              >School Year</label
            >
            <select
              class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
              [value]="selectedSchoolYearId()"
              (change)="selectedSchoolYearId.set(asInputValue($event))"
              [disabled]="isSubmitting()"
              data-testid="assign-school-year-picker"
            >
              <option value="" i18n="@@school.assignClass.selectSchoolYear">
                -- Select school year --
              </option>
              @for (sy of schoolYears; track sy.id) {
                <option [value]="sy.id">
                  {{ sy.startYear }}/{{ sy.endYear }} ({{ sy.federalStateName }})
                </option>
              }
            </select>
          </div>

          <!-- Class Name -->
          <div>
            <label
              class="block text-sm font-medium text-gray-700 mb-1"
              i18n="@@school.assignClass.className"
              >Class Name</label
            >
            <input
              type="text"
              class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
              [value]="className()"
              (input)="className.set(asInputValue($event))"
              [disabled]="isSubmitting()"
              placeholder="e.g. 1a"
              data-testid="assign-class-name-input"
            />
          </div>

          @if (errorMessage()) {
            <div
              class="p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700"
              role="alert"
            >
              {{ errorMessage() }}
            </div>
          }

          <div class="flex justify-end gap-3 pt-2">
            <button
              type="button"
              (click)="onDismiss()"
              [disabled]="isSubmitting()"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
              i18n="@@school.assignClass.cancel"
            >
              Cancel
            </button>
            <button
              type="button"
              data-testid="assign-class-submit"
              [disabled]="isSubmitting() || !canSubmit()"
              (click)="onSubmit()"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              i18n="@@school.assignClass.assign"
            >
              Assign
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class AssignClassDialogComponent {
  private schoolService = inject(SchoolService);

  @Input({ required: true }) studentId!: string;
  @Input() schools: SchoolDto[] = [];
  @Input() schoolYears: SchoolYearDto[] = [];
  @Output() assigned = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  selectedSchoolId = signal('');
  selectedSchoolYearId = signal('');
  className = signal('');
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  canSubmit = computed(
    () =>
      this.selectedSchoolId() !== '' &&
      this.selectedSchoolYearId() !== '' &&
      this.className().trim() !== '',
  );

  asInputValue(event: Event): string {
    return (event.target as HTMLInputElement | HTMLSelectElement).value;
  }

  onSubmit(): void {
    if (!this.canSubmit()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.schoolService
      .assignStudentToClass({
        studentId: this.studentId,
        schoolId: this.selectedSchoolId(),
        schoolYearId: this.selectedSchoolYearId(),
        className: this.className().trim(),
      })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.assigned.emit();
        },
        error: (err) => {
          this.errorMessage.set(
            err?.message ??
              $localize`:@@school.assignClass.error:Failed to assign student to class`,
          );
          this.isSubmitting.set(false);
        },
      });
  }

  onDismiss(): void {
    this.dialogClosed.emit();
  }
}
