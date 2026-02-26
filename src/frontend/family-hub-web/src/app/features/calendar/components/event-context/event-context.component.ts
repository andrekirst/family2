import {
  Component,
  Input,
  Output,
  EventEmitter,
  inject,
  signal,
  computed,
  OnInit,
  OnChanges,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { CalendarService, CalendarEventDto } from '../../services/calendar.service';
import { UserService } from '../../../../core/user/user.service';
import { InvitationService } from '../../../family/services/invitation.service';
import { FamilyMemberDto } from '../../../family/models/invitation.models';
import { ContextPanelService } from '../../../../shared/services/context-panel.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { InlineEditTextComponent } from '../../../../shared/components/inline-edit-text/inline-edit-text.component';
import {
  DateTimePickerComponent,
  DateTimeChangeEvent,
} from '../../../../shared/components/date-time-picker/date-time-picker.component';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';

interface FormSnapshot {
  title: string;
  description: string | null;
  startTime: string;
  endTime: string;
  isAllDay: boolean;
}

@Component({
  selector: 'app-event-context',
  standalone: true,
  imports: [InlineEditTextComponent, DateTimePickerComponent, ConfirmationDialogComponent],
  template: `
    <div class="p-4 space-y-4" data-testid="event-context">
      @if (isCancelled()) {
        <span
          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800 border border-red-200"
          data-testid="event-context-cancelled"
        >
          <span i18n="@@calendar.context.cancelled">Cancelled</span>
        </span>
      }

      <!-- Title (inline-editable) -->
      <div>
        <app-inline-edit-text
          #titleEditor
          [value]="title()"
          [placeholder]="eventTitlePlaceholder"
          [disabled]="isCancelled()"
          testId="event-context-title"
          displayClasses="text-lg font-semibold text-gray-900"
          (saved)="onTitleSaved($event)"
        />
        @if (titleError()) {
          <p class="mt-1 text-xs text-red-600" data-testid="event-context-title-error">
            {{ titleError() }}
          </p>
        }
      </div>

      <!-- Date & Time (inline-editable picker) -->
      <app-date-time-picker
        class="!mt-6"
        [startTime]="startTime()"
        [endTime]="endTime()"
        [isAllDay]="isAllDay()"
        [disabled]="isCancelled()"
        testId="event-context-datetime"
        (dateTimeChanged)="onDateTimeChanged($event)"
      />

      <!-- Location (display-only) -->
      @if (location()) {
        <div
          class="flex items-start gap-2 text-sm text-gray-600"
          data-testid="event-context-location"
        >
          <svg
            class="h-4 w-4 flex-shrink-0 mt-0.5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
            />
          </svg>
          <span>{{ location() }}</span>
        </div>
      }

      <!-- Description (inline-editable) -->
      <div>
        <h4 class="text-sm font-medium text-gray-700 mb-1" i18n="@@calendar.event.description">
          Description
        </h4>
        <app-inline-edit-text
          [value]="description()"
          [placeholder]="descriptionPlaceholder"
          [multiline]="true"
          [disabled]="isCancelled()"
          testId="event-context-description"
          displayClasses="text-sm text-gray-600 whitespace-pre-wrap"
          (saved)="onDescriptionSaved($event)"
        />
      </div>

      <!-- Attendees (display-only, resolved names) -->
      @if (resolvedAttendees().length > 0) {
        <div data-testid="event-context-attendees">
          <h4 class="text-sm font-medium text-gray-700 mb-2" i18n="@@calendar.event.attendees">
            Attendees
          </h4>
          <div class="flex flex-wrap gap-1.5">
            @for (attendee of resolvedAttendees(); track attendee.userId) {
              <span
                class="inline-flex items-center px-2 py-0.5 rounded-md bg-gray-100 text-xs text-gray-700"
              >
                {{ attendee.userName }}
              </span>
            }
          </div>
        </div>
      }

      <!-- Save / Create button -->
      @if (!isCancelled()) {
        <div class="pt-2">
          <button
            (click)="save()"
            [disabled]="!canSave() && !saveSuccess()"
            [class]="
              saveSuccess()
                ? 'w-full px-4 py-2 text-sm font-medium text-white bg-green-600 rounded-lg transition-colors duration-300'
                : 'w-full px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors duration-300 disabled:opacity-50 disabled:cursor-not-allowed'
            "
            data-testid="event-context-save"
          >
            @if (isSaving()) {
              <span class="flex items-center justify-center gap-2">
                <svg class="animate-spin h-4 w-4" viewBox="0 0 24 24">
                  <circle
                    class="opacity-25"
                    cx="12"
                    cy="12"
                    r="10"
                    stroke="currentColor"
                    stroke-width="4"
                    fill="none"
                  />
                  <path
                    class="opacity-75"
                    fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
                  />
                </svg>
                <span i18n="@@calendar.event.saving">Saving...</span>
              </span>
            } @else if (saveSuccess()) {
              <span class="flex items-center justify-center gap-2">
                <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M5 13l4 4L19 7"
                  />
                </svg>
                <span i18n="@@calendar.context.saved">Saved!</span>
              </span>
            } @else {
              {{ eventId() ? saveChangesLabel : createEventLabel }}
            }
          </button>
        </div>
      }

      <!-- Cancel Event button -->
      @if (eventId() && !isCancelled()) {
        <div class="pt-2 border-t border-gray-200">
          <button
            (click)="onCancelEvent()"
            [disabled]="isSaving()"
            class="w-full px-4 py-2 text-sm font-medium text-red-700 bg-red-50 rounded-lg hover:bg-red-100 transition-colors disabled:opacity-50"
            data-testid="event-context-cancel"
          >
            <span i18n="@@calendar.event.cancelEvent">Cancel Event</span>
          </button>
        </div>
      }

      @if (errorMessage()) {
        <div class="text-xs text-red-600" data-testid="event-context-error">
          {{ errorMessage() }}
        </div>
      }
    </div>

    @if (showCancelConfirmation()) {
      <app-confirmation-dialog
        [title]="cancelEventLabel"
        [message]="cancelConfirmationMessage()"
        [confirmLabel]="cancelEventLabel"
        [cancelLabel]="goBackLabel"
        variant="danger"
        icon="trash"
        [isLoading]="isCancelLoading()"
        (confirmed)="onCancelConfirmed()"
        (cancelled)="onCancelDismissed()"
        data-testid="cancel-event-confirmation"
      />
    }
  `,
})
export class EventContextComponent implements OnInit, OnChanges {
  private readonly calendarService = inject(CalendarService);
  private readonly userService = inject(UserService);
  private readonly invitationService = inject(InvitationService);
  private readonly contextPanelService = inject(ContextPanelService);
  private readonly toastService = inject(ToastService);

  @Input() event: CalendarEventDto | null = null;
  @Input() selectedDate: Date | null = null;
  @Input() selectedStartDate: Date | null = null;
  @Input() selectedEndDate: Date | null = null;

  @Output() eventCreated = new EventEmitter<CalendarEventDto>();
  @Output() eventUpdated = new EventEmitter<void>();
  @Output() eventCancelled = new EventEmitter<void>();

  @ViewChild('titleEditor') titleEditor?: InlineEditTextComponent;

  // i18n labels
  readonly eventTitlePlaceholder = $localize`:@@calendar.event.titlePlaceholder:Event title`;
  readonly descriptionPlaceholder = $localize`:@@calendar.context.descriptionPlaceholder:Add description`;
  readonly saveChangesLabel = $localize`:@@calendar.event.saveChanges:Save Changes`;
  readonly createEventLabel = $localize`:@@calendar.event.createEvent:Create Event`;
  readonly cancelEventLabel = $localize`:@@calendar.event.cancelEvent:Cancel Event`;
  readonly goBackLabel = $localize`:@@calendar.event.goBack:Go Back`;

  // Form state
  readonly eventId = signal<string | null>(null);
  readonly title = signal('');
  readonly description = signal<string | null>('');
  readonly location = signal<string | null>('');
  readonly startTime = signal('');
  readonly endTime = signal('');
  readonly isAllDay = signal(false);
  readonly selectedAttendees = signal<string[]>([]);
  readonly isCancelled = signal(false);

  // UI state
  readonly isSaving = signal(false);
  readonly saveSuccess = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly titleError = signal<string | null>(null);
  readonly showCancelConfirmation = signal(false);
  readonly isCancelLoading = signal(false);

  // Dirty tracking
  private readonly savedSnapshot = signal<FormSnapshot | null>(null);

  readonly isDirty = computed(() => {
    const snapshot = this.savedSnapshot();
    if (!snapshot) return this.title().trim().length > 0; // create mode
    return (
      this.title() !== snapshot.title ||
      this.description() !== snapshot.description ||
      this.startTime() !== snapshot.startTime ||
      this.endTime() !== snapshot.endTime ||
      this.isAllDay() !== snapshot.isAllDay
    );
  });

  readonly canSave = computed(
    () => !!this.title().trim() && this.isDirty() && !this.isSaving() && !this.isCancelled(),
  );

  // Family members for attendee resolution
  readonly familyMembers = signal<FamilyMemberDto[]>([]);

  readonly resolvedAttendees = computed(() => {
    const members = this.familyMembers();
    const attendeeIds = this.selectedAttendees();
    return attendeeIds
      .map((id) => members.find((m) => m.userId === id))
      .filter((m): m is FamilyMemberDto => m != null);
  });

  readonly cancelConfirmationMessage = computed(() => {
    const title = this.title();
    return $localize`:@@calendar.event.cancelConfirm:Are you sure you want to cancel '${title}:title:'? This action cannot be undone.`;
  });

  ngOnInit(): void {
    this.initializeForm();
    this.loadFamilyMembers();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['event']?.isFirstChange() && !changes['selectedDate']?.isFirstChange()) {
      if (changes['event'] || changes['selectedDate']) {
        this.initializeForm();
      }
    }
  }

  private initializeForm(): void {
    if (this.event) {
      // Edit mode: populate from existing event
      this.eventId.set(this.event.id);
      this.title.set(this.event.title);
      this.description.set(this.event.description);
      this.location.set(this.event.location);
      this.isAllDay.set(this.event.isAllDay);
      this.isCancelled.set(this.event.isCancelled);
      this.selectedAttendees.set(this.event.attendees.map((a) => a.userId));

      const startIso = new Date(this.event.startTime).toISOString();
      const endIso = new Date(this.event.endTime).toISOString();
      this.startTime.set(startIso);
      this.endTime.set(endIso);

      this.savedSnapshot.set({
        title: this.event.title,
        description: this.event.description,
        startTime: startIso,
        endTime: endIso,
        isAllDay: this.event.isAllDay,
      });
    } else if (this.selectedDate) {
      // Create mode: set defaults
      this.eventId.set(null);
      this.title.set('');
      this.description.set('');
      this.location.set('');
      this.isAllDay.set(false);
      this.isCancelled.set(false);

      const date = this.selectedDate;
      const hasTimeInfo = date.getHours() !== 0 || date.getMinutes() !== 0;

      if (hasTimeInfo) {
        this.startTime.set(date.toISOString());
        this.endTime.set(new Date(date.getTime() + 3600000).toISOString());
      } else {
        const start = new Date(date.getFullYear(), date.getMonth(), date.getDate(), 9, 0);
        const end = new Date(date.getFullYear(), date.getMonth(), date.getDate(), 10, 0);
        this.startTime.set(start.toISOString());
        this.endTime.set(end.toISOString());
      }

      // Pre-select current user as attendee
      const currentUser = this.userService.currentUser();
      if (currentUser) {
        this.selectedAttendees.set([currentUser.id]);
      }

      // Create mode has no saved snapshot
      this.savedSnapshot.set(null);

      // Auto-focus title after render
      setTimeout(() => this.titleEditor?.startEditing());
    }

    this.errorMessage.set(null);
    this.titleError.set(null);
  }

  private loadFamilyMembers(): void {
    this.invitationService.getFamilyMembers().subscribe((members) => {
      this.familyMembers.set(members);
    });
  }

  onTitleSaved(newTitle: string): void {
    if (!newTitle.trim()) {
      this.titleError.set($localize`:@@calendar.event.titleRequired:Event title is required`);
      return;
    }
    this.titleError.set(null);
    this.title.set(newTitle);
    this.errorMessage.set(null);
  }

  onDescriptionSaved(newDescription: string): void {
    this.description.set(newDescription || null);
    this.errorMessage.set(null);
  }

  onDateTimeChanged(event: DateTimeChangeEvent): void {
    this.startTime.set(event.startTime);
    this.endTime.set(event.endTime);
    this.isAllDay.set(event.isAllDay);
    this.errorMessage.set(null);
  }

  save(): void {
    if (!this.canSave()) return;

    this.isSaving.set(true);
    this.errorMessage.set(null);

    const payload = {
      title: this.title().trim(),
      description: this.description()?.trim() || null,
      location: this.location()?.trim() || null,
      startTime: new Date(this.startTime()).toISOString(),
      endTime: new Date(this.endTime()).toISOString(),
      isAllDay: this.isAllDay(),
      attendeeIds: this.selectedAttendees(),
    };

    if (this.eventId()) {
      // Update existing event
      this.calendarService.updateCalendarEvent(this.eventId()!, payload).subscribe({
        next: (result) => {
          this.isSaving.set(false);
          if (result) {
            this.updateSnapshot();
            this.showSaveSuccess('Changes saved');
            this.eventUpdated.emit();
          } else {
            this.errorMessage.set($localize`:@@calendar.event.updateFailed:Failed to update event`);
          }
        },
        error: () => {
          this.isSaving.set(false);
          this.errorMessage.set($localize`:@@calendar.event.error:An error occurred`);
        },
      });
    } else {
      // Create new event
      this.calendarService.createCalendarEvent(payload).subscribe({
        next: (result) => {
          this.isSaving.set(false);
          if (result) {
            this.eventId.set(result.id);
            this.updateSnapshot();
            this.showSaveSuccess('Event created');
            this.contextPanelService.setItemId(result.id);
            this.eventCreated.emit(result);
          } else {
            this.errorMessage.set($localize`:@@calendar.event.createFailed:Failed to create event`);
          }
        },
        error: () => {
          this.isSaving.set(false);
          this.errorMessage.set($localize`:@@calendar.event.error:An error occurred`);
        },
      });
    }
  }

  private updateSnapshot(): void {
    this.savedSnapshot.set({
      title: this.title(),
      description: this.description(),
      startTime: this.startTime(),
      endTime: this.endTime(),
      isAllDay: this.isAllDay(),
    });
  }

  private showSaveSuccess(message: string): void {
    this.saveSuccess.set(true);
    this.toastService.success(message);
    setTimeout(() => this.saveSuccess.set(false), 2000);
  }

  onCancelEvent(): void {
    this.showCancelConfirmation.set(true);
  }

  onCancelConfirmed(): void {
    if (!this.eventId()) return;

    this.isCancelLoading.set(true);
    this.calendarService.cancelCalendarEvent(this.eventId()!).subscribe({
      next: (success) => {
        this.isCancelLoading.set(false);
        this.showCancelConfirmation.set(false);
        if (success) {
          this.eventCancelled.emit();
          this.contextPanelService.close();
        } else {
          this.errorMessage.set($localize`:@@calendar.event.cancelFailed:Failed to cancel event`);
        }
      },
      error: () => {
        this.isCancelLoading.set(false);
        this.showCancelConfirmation.set(false);
        this.errorMessage.set($localize`:@@calendar.event.error:An error occurred`);
      },
    });
  }

  onCancelDismissed(): void {
    this.showCancelConfirmation.set(false);
  }
}
