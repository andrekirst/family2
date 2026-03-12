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
import { SchoolService, SchoolDto } from '../../services/school.service';
import { BaseDataService, FederalStateDto } from '../../../base-data/services/base-data.service';

@Component({
  selector: 'app-school-form-dialog',
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
          <!-- Name -->
          <div>
            <label
              class="block text-sm font-medium text-gray-700 mb-1"
              i18n="@@school.schoolForm.name"
              >School Name</label
            >
            <input
              type="text"
              class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
              [value]="name()"
              (input)="name.set(asInputValue($event))"
              [disabled]="isSubmitting()"
              data-testid="school-name-input"
            />
          </div>

          <!-- Federal State -->
          <div>
            <label
              class="block text-sm font-medium text-gray-700 mb-1"
              i18n="@@school.schoolForm.federalState"
              >Federal State</label
            >
            <select
              class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
              [value]="federalStateId()"
              (change)="federalStateId.set(asInputValue($event))"
              [disabled]="isSubmitting()"
              data-testid="school-federal-state-picker"
            >
              <option value="" i18n="@@school.schoolForm.selectFederalState">
                -- Select federal state --
              </option>
              @for (state of federalStates(); track state.id) {
                <option [value]="state.id">{{ state.name }}</option>
              }
            </select>
          </div>

          <!-- City -->
          <div>
            <label
              class="block text-sm font-medium text-gray-700 mb-1"
              i18n="@@school.schoolForm.city"
              >City</label
            >
            <input
              type="text"
              class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
              [value]="city()"
              (input)="city.set(asInputValue($event))"
              [disabled]="isSubmitting()"
              data-testid="school-city-input"
            />
          </div>

          <!-- Postal Code -->
          <div>
            <label
              class="block text-sm font-medium text-gray-700 mb-1"
              i18n="@@school.schoolForm.postalCode"
              >Postal Code</label
            >
            <input
              type="text"
              class="w-full rounded-lg border-gray-300 shadow-sm text-sm focus:border-blue-500 focus:ring-blue-500"
              [value]="postalCode()"
              (input)="postalCode.set(asInputValue($event))"
              [disabled]="isSubmitting()"
              data-testid="school-postal-code-input"
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
              i18n="@@school.schoolForm.cancel"
            >
              Cancel
            </button>
            <button
              type="button"
              data-testid="school-form-submit"
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
export class SchoolFormDialogComponent implements OnInit {
  private schoolService = inject(SchoolService);
  private baseDataService = inject(BaseDataService);

  @Input() school: SchoolDto | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  federalStates = signal<FederalStateDto[]>([]);
  name = signal('');
  federalStateId = signal('');
  city = signal('');
  postalCode = signal('');
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  isEditMode = computed(() => this.school !== null);
  dialogTitle = computed(() =>
    this.isEditMode()
      ? $localize`:@@school.schoolForm.editTitle:Edit School`
      : $localize`:@@school.schoolForm.createTitle:Create School`,
  );
  submitLabel = computed(() =>
    this.isEditMode()
      ? $localize`:@@school.schoolForm.save:Save`
      : $localize`:@@school.schoolForm.create:Create`,
  );
  canSubmit = computed(
    () =>
      this.name().trim() !== '' &&
      this.federalStateId() !== '' &&
      this.city().trim() !== '' &&
      this.postalCode().trim() !== '',
  );

  ngOnInit(): void {
    this.baseDataService.getFederalStates().subscribe({
      next: (states) => this.federalStates.set(states),
    });

    if (this.school) {
      this.name.set(this.school.name);
      this.federalStateId.set(this.school.federalStateId);
      this.city.set(this.school.city);
      this.postalCode.set(this.school.postalCode);
    }
  }

  asInputValue(event: Event): string {
    return (event.target as HTMLInputElement | HTMLSelectElement).value;
  }

  onSubmit(): void {
    if (!this.canSubmit()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    if (this.isEditMode() && this.school) {
      this.schoolService
        .updateSchool({
          schoolId: this.school.id,
          name: this.name().trim(),
          federalStateId: this.federalStateId(),
          city: this.city().trim(),
          postalCode: this.postalCode().trim(),
        })
        .subscribe({
          next: () => {
            this.isSubmitting.set(false);
            this.saved.emit();
          },
          error: (err) => {
            this.errorMessage.set(
              err?.message ?? $localize`:@@school.schoolForm.updateError:Failed to update school`,
            );
            this.isSubmitting.set(false);
          },
        });
    } else {
      this.schoolService
        .createSchool({
          name: this.name().trim(),
          federalStateId: this.federalStateId(),
          city: this.city().trim(),
          postalCode: this.postalCode().trim(),
        })
        .subscribe({
          next: () => {
            this.isSubmitting.set(false);
            this.saved.emit();
          },
          error: (err) => {
            this.errorMessage.set(
              err?.message ?? $localize`:@@school.schoolForm.createError:Failed to create school`,
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
