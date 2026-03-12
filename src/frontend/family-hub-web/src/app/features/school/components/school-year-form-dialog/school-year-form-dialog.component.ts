import {
  Component,
  EventEmitter,
  Input,
  Output,
  signal,
  computed,
  inject,
  OnInit,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { SchoolService, SchoolYearDto } from '../../services/school.service';
import { BaseDataService, FederalStateDto } from '../../../base-data/services/base-data.service';

@Component({
  selector: 'app-school-year-form-dialog',
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
          <h2 class="text-lg font-semibold text-gray-900">
            {{ dialogTitle() }}
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
          <!-- Federal State -->
          <div>
            <label
              class="block text-sm font-medium text-gray-700 mb-1"
              i18n="@@school.schoolYearForm.federalState"
              >Federal State</label
            >
            <select
              class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
              [value]="federalStateId()"
              (change)="federalStateId.set(asInputValue($event))"
              [disabled]="isSubmitting()"
              data-testid="school-year-federal-state-picker"
            >
              <option value="" i18n="@@school.schoolYearForm.selectFederalState">
                -- Select federal state --
              </option>
              @for (state of federalStates(); track state.id) {
                <option [value]="state.id">{{ state.name }}</option>
              }
            </select>
          </div>

          <!-- Start / End Year -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label
                class="block text-sm font-medium text-gray-700 mb-1"
                i18n="@@school.schoolYearForm.startYear"
                >Start Year</label
              >
              <input
                type="number"
                class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
                [value]="startYear()"
                (input)="startYear.set(asNumberValue($event))"
                [disabled]="isSubmitting()"
                data-testid="school-year-start-year-input"
              />
            </div>
            <div>
              <label
                class="block text-sm font-medium text-gray-700 mb-1"
                i18n="@@school.schoolYearForm.endYear"
                >End Year</label
              >
              <input
                type="number"
                class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
                [value]="endYear()"
                (input)="endYear.set(asNumberValue($event))"
                [disabled]="isSubmitting()"
                data-testid="school-year-end-year-input"
              />
            </div>
          </div>

          <!-- Start / End Date -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label
                class="block text-sm font-medium text-gray-700 mb-1"
                i18n="@@school.schoolYearForm.startDate"
                >Start Date</label
              >
              <input
                type="date"
                class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
                [value]="startDate()"
                (input)="startDate.set(asInputValue($event))"
                [disabled]="isSubmitting()"
                data-testid="school-year-start-date-input"
              />
            </div>
            <div>
              <label
                class="block text-sm font-medium text-gray-700 mb-1"
                i18n="@@school.schoolYearForm.endDate"
                >End Date</label
              >
              <input
                type="date"
                class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
                [value]="endDate()"
                (input)="endDate.set(asInputValue($event))"
                [disabled]="isSubmitting()"
                data-testid="school-year-end-date-input"
              />
            </div>
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
              i18n="@@school.schoolYearForm.cancel"
            >
              Cancel
            </button>
            <button
              type="button"
              data-testid="school-year-form-submit"
              [disabled]="isSubmitting() || !canSubmit()"
              (click)="onSubmit()"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {{ submitLabel() }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class SchoolYearFormDialogComponent implements OnInit {
  private schoolService = inject(SchoolService);
  private baseDataService = inject(BaseDataService);

  @Input() schoolYear: SchoolYearDto | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  federalStates = signal<FederalStateDto[]>([]);
  federalStateId = signal('');
  startYear = signal(new Date().getFullYear());
  endYear = signal(new Date().getFullYear() + 1);
  startDate = signal('');
  endDate = signal('');
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  isEditMode = computed(() => this.schoolYear !== null);
  dialogTitle = computed(() =>
    this.isEditMode()
      ? $localize`:@@school.schoolYearForm.editTitle:Edit School Year`
      : $localize`:@@school.schoolYearForm.createTitle:Create School Year`,
  );
  submitLabel = computed(() =>
    this.isEditMode()
      ? $localize`:@@school.schoolYearForm.save:Save`
      : $localize`:@@school.schoolYearForm.create:Create`,
  );
  canSubmit = computed(
    () =>
      this.federalStateId() !== '' &&
      this.startYear() > 0 &&
      this.endYear() > 0 &&
      this.startDate() !== '' &&
      this.endDate() !== '',
  );

  ngOnInit(): void {
    this.baseDataService.getFederalStates().subscribe({
      next: (states) => this.federalStates.set(states),
    });

    if (this.schoolYear) {
      this.federalStateId.set(this.schoolYear.federalStateId);
      this.startYear.set(this.schoolYear.startYear);
      this.endYear.set(this.schoolYear.endYear);
      this.startDate.set(this.schoolYear.startDate);
      this.endDate.set(this.schoolYear.endDate);
    }
  }

  asInputValue(event: Event): string {
    return (event.target as HTMLInputElement | HTMLSelectElement).value;
  }

  asNumberValue(event: Event): number {
    return parseInt((event.target as HTMLInputElement).value, 10) || 0;
  }

  onSubmit(): void {
    if (!this.canSubmit()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    if (this.isEditMode() && this.schoolYear) {
      this.schoolService
        .updateSchoolYear({
          schoolYearId: this.schoolYear.id,
          federalStateId: this.federalStateId(),
          startYear: this.startYear(),
          endYear: this.endYear(),
          startDate: this.startDate(),
          endDate: this.endDate(),
        })
        .subscribe({
          next: () => {
            this.isSubmitting.set(false);
            this.saved.emit();
          },
          error: (err) => {
            this.errorMessage.set(
              err?.message ??
                $localize`:@@school.schoolYearForm.updateError:Failed to update school year`,
            );
            this.isSubmitting.set(false);
          },
        });
    } else {
      this.schoolService
        .createSchoolYear({
          federalStateId: this.federalStateId(),
          startYear: this.startYear(),
          endYear: this.endYear(),
          startDate: this.startDate(),
          endDate: this.endDate(),
        })
        .subscribe({
          next: () => {
            this.isSubmitting.set(false);
            this.saved.emit();
          },
          error: (err) => {
            this.errorMessage.set(
              err?.message ??
                $localize`:@@school.schoolYearForm.createError:Failed to create school year`,
            );
            this.isSubmitting.set(false);
          },
        });
    }
  }

  onDismiss(): void {
    this.dialogClosed.emit();
  }
}
