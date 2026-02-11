import {
  Component,
  EventEmitter,
  Input,
  Output,
  signal,
  computed,
  inject,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CalendarService, CalendarEventDto } from '../../services/calendar.service';
import { UserService, CurrentUser } from '../../../../core/user/user.service';
import { InvitationService } from '../../../family/services/invitation.service';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';

const EVENT_TYPES = ['Personal', 'Medical', 'School', 'Work', 'Social', 'Travel', 'Other'];

@Component({
  selector: 'app-event-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, ConfirmationDialogComponent],
  template: `
    <div
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50"
      (click)="onDismiss()"
    >
      <div
        class="bg-white rounded-lg max-w-lg w-[90%] shadow-xl max-h-[90vh] overflow-y-auto"
        (click)="$event.stopPropagation()"
      >
        <!-- Header -->
        <div class="flex justify-between items-center px-6 py-4 border-b border-gray-200">
          <h2 class="text-lg font-semibold text-gray-900">
            {{ isEditMode() ? 'Edit Event' : 'New Event' }}
          </h2>
          <button
            (click)="onDismiss()"
            class="text-gray-400 hover:text-gray-600 text-2xl leading-none"
            aria-label="Close"
          >
            &times;
          </button>
        </div>

        <!-- Form -->
        <div class="px-6 py-4">
          <form (ngSubmit)="onSubmit()">
            <!-- Title -->
            <div class="mb-4">
              <label for="event-title" class="block text-sm font-medium text-gray-700 mb-1"
                >Title</label
              >
              <input
                id="event-title"
                type="text"
                data-testid="event-title-input"
                [ngModel]="title()"
                (ngModelChange)="title.set($event)"
                [disabled]="isLoading()"
                name="title"
                placeholder="Event title"
                class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500"
              />
            </div>

            <!-- Description -->
            <div class="mb-4">
              <label for="event-description" class="block text-sm font-medium text-gray-700 mb-1"
                >Description</label
              >
              <textarea
                id="event-description"
                data-testid="event-description-input"
                [ngModel]="description()"
                (ngModelChange)="description.set($event)"
                [disabled]="isLoading()"
                name="description"
                rows="2"
                placeholder="Optional description"
                class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500"
              ></textarea>
            </div>

            <!-- Location -->
            <div class="mb-4">
              <label for="event-location" class="block text-sm font-medium text-gray-700 mb-1"
                >Location</label
              >
              <input
                id="event-location"
                type="text"
                data-testid="event-location-input"
                [ngModel]="location()"
                (ngModelChange)="location.set($event)"
                [disabled]="isLoading()"
                name="location"
                placeholder="Optional location"
                class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500"
              />
            </div>

            <!-- All Day Toggle -->
            <div class="mb-4 flex items-center gap-2">
              <input
                id="event-allday"
                type="checkbox"
                data-testid="event-allday-checkbox"
                [ngModel]="isAllDay()"
                (ngModelChange)="isAllDay.set($event)"
                [disabled]="isLoading()"
                name="isAllDay"
                class="h-4 w-4 text-blue-600 border-gray-300 rounded"
              />
              <label for="event-allday" class="text-sm font-medium text-gray-700"
                >All day event</label
              >
            </div>

            <!-- Date/Time -->
            <div class="grid grid-cols-2 gap-4 mb-4">
              <div>
                <label for="event-start" class="block text-sm font-medium text-gray-700 mb-1"
                  >Start</label
                >
                <input
                  id="event-start"
                  [type]="isAllDay() ? 'date' : 'datetime-local'"
                  data-testid="event-start-input"
                  [ngModel]="startTime()"
                  (ngModelChange)="startTime.set($event)"
                  [disabled]="isLoading()"
                  name="startTime"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
              <div>
                <label for="event-end" class="block text-sm font-medium text-gray-700 mb-1"
                  >End</label
                >
                <input
                  id="event-end"
                  [type]="isAllDay() ? 'date' : 'datetime-local'"
                  data-testid="event-end-input"
                  [ngModel]="endTime()"
                  (ngModelChange)="endTime.set($event)"
                  [disabled]="isLoading()"
                  name="endTime"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
            </div>

            <!-- Event Type -->
            <div class="mb-4">
              <label for="event-type" class="block text-sm font-medium text-gray-700 mb-1"
                >Event Type</label
              >
              <select
                id="event-type"
                data-testid="event-type-select"
                [ngModel]="eventType()"
                (ngModelChange)="eventType.set($event)"
                [disabled]="isLoading()"
                name="eventType"
                class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500"
              >
                @for (type of eventTypes; track type) {
                  <option [value]="type">{{ type }}</option>
                }
              </select>
            </div>

            <!-- Attendees -->
            @if (familyMembers().length > 0) {
              <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Attendees</label>
                <div class="space-y-2 max-h-32 overflow-y-auto">
                  @for (member of familyMembers(); track member.id) {
                    <label class="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        [checked]="selectedAttendees().includes(member.id)"
                        (change)="toggleAttendee(member.id)"
                        [disabled]="isLoading()"
                        class="h-4 w-4 text-blue-600 border-gray-300 rounded"
                        data-testid="attendee-checkbox"
                      />
                      <span class="text-sm text-gray-700">{{ member.name }}</span>
                    </label>
                  }
                </div>
              </div>
            }

            <!-- Error Message -->
            @if (errorMessage()) {
              <div class="mb-4 text-sm text-red-600" role="alert" data-testid="event-error">
                {{ errorMessage() }}
              </div>
            }

            <!-- Actions -->
            <div class="flex gap-3 justify-end mt-6">
              @if (isEditMode()) {
                <button
                  type="button"
                  (click)="onCancel()"
                  [disabled]="isLoading()"
                  class="px-4 py-2 text-sm font-medium text-red-700 bg-red-100 rounded-md hover:bg-red-200 disabled:opacity-50"
                  data-testid="cancel-event-button"
                >
                  Cancel Event
                </button>
              }
              <button
                type="button"
                (click)="onDismiss()"
                [disabled]="isLoading()"
                class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200 disabled:opacity-50"
              >
                Close
              </button>
              <button
                type="submit"
                [disabled]="isLoading()"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
                data-testid="save-event-button"
              >
                {{ isLoading() ? 'Saving...' : isEditMode() ? 'Save Changes' : 'Create Event' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    @if (showCancelConfirmation()) {
      <app-confirmation-dialog
        title="Cancel Event"
        [message]="cancelConfirmationMessage()"
        confirmLabel="Cancel Event"
        cancelLabel="Go Back"
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
export class EventDialogComponent implements OnInit {
  private calendarService = inject(CalendarService);
  private userService = inject(UserService);
  private invitationService = inject(InvitationService);

  @Input() event: CalendarEventDto | null = null;
  @Input() selectedDate: Date | null = null;

  @Output() eventCreated = new EventEmitter<void>();
  @Output() eventUpdated = new EventEmitter<void>();
  @Output() eventCancelled = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  readonly eventTypes = EVENT_TYPES;

  title = signal('');
  description = signal('');
  location = signal('');
  startTime = signal('');
  endTime = signal('');
  isAllDay = signal(false);
  eventType = signal('Personal');
  selectedAttendees = signal<string[]>([]);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  familyMembers = signal<CurrentUser[]>([]);
  isEditMode = signal(false);
  showCancelConfirmation = signal(false);
  isCancelLoading = signal(false);
  cancelConfirmationMessage = computed(
    () => `Are you sure you want to cancel '${this.title()}'? This action cannot be undone.`,
  );

  ngOnInit(): void {
    if (this.event) {
      this.isEditMode.set(true);
      this.title.set(this.event.title);
      this.description.set(this.event.description ?? '');
      this.location.set(this.event.location ?? '');
      this.isAllDay.set(this.event.isAllDay);
      this.eventType.set(this.event.type);
      this.selectedAttendees.set(this.event.attendees.map((a) => a.userId));

      if (this.event.isAllDay) {
        this.startTime.set(this.formatDateOnly(new Date(this.event.startTime)));
        this.endTime.set(this.formatDateOnly(new Date(this.event.endTime)));
      } else {
        this.startTime.set(this.formatDateTimeLocal(new Date(this.event.startTime)));
        this.endTime.set(this.formatDateTimeLocal(new Date(this.event.endTime)));
      }
    } else if (this.selectedDate) {
      const date = this.selectedDate;
      this.startTime.set(
        this.formatDateTimeLocal(
          new Date(date.getFullYear(), date.getMonth(), date.getDate(), 9, 0),
        ),
      );
      this.endTime.set(
        this.formatDateTimeLocal(
          new Date(date.getFullYear(), date.getMonth(), date.getDate(), 10, 0),
        ),
      );
    }

    // Load family members for attendee selection
    this.invitationService.getFamilyMembers().subscribe((members) => {
      // Map FamilyMemberDto → CurrentUser shape for the attendee checkboxes
      const mapped: CurrentUser[] = members.map((m) => ({
        id: m.userId,
        name: m.userName,
        email: m.userEmail,
        emailVerified: true,
        isActive: m.isActive,
        permissions: [],
      }));
      this.familyMembers.set(mapped);

      // Pre-select current user as attendee for new events
      if (!this.isEditMode()) {
        const currentUser = this.userService.currentUser();
        if (currentUser) {
          this.selectedAttendees.set([currentUser.id]);
        }
      }
    });
  }

  onSubmit(): void {
    if (!this.title().trim()) {
      this.errorMessage.set('Event title is required');
      return;
    }

    if (!this.startTime() || !this.endTime()) {
      this.errorMessage.set('Start and end times are required');
      return;
    }

    const startDate = new Date(this.startTime());
    const endDate = new Date(this.endTime());

    if (endDate <= startDate) {
      this.errorMessage.set('End time must be after start time');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    if (this.isEditMode() && this.event) {
      this.calendarService
        .updateCalendarEvent(this.event.id, {
          title: this.title().trim(),
          description: this.description().trim() || null,
          location: this.location().trim() || null,
          startTime: startDate.toISOString(),
          endTime: endDate.toISOString(),
          isAllDay: this.isAllDay(),
          type: this.eventType(),
          attendeeIds: this.selectedAttendees(),
        })
        .subscribe({
          next: (result) => {
            this.isLoading.set(false);
            if (result) {
              this.eventUpdated.emit();
            } else {
              this.errorMessage.set('Failed to update event');
            }
          },
          error: () => {
            this.isLoading.set(false);
            this.errorMessage.set('An error occurred');
          },
        });
    } else {
      this.calendarService
        .createCalendarEvent({
          title: this.title().trim(),
          description: this.description().trim() || null,
          location: this.location().trim() || null,
          startTime: startDate.toISOString(),
          endTime: endDate.toISOString(),
          isAllDay: this.isAllDay(),
          type: this.eventType(),
          attendeeIds: this.selectedAttendees(),
        })
        .subscribe({
          next: (result) => {
            this.isLoading.set(false);
            if (result) {
              this.eventCreated.emit();
            } else {
              this.errorMessage.set('Failed to create event');
            }
          },
          error: () => {
            this.isLoading.set(false);
            this.errorMessage.set('An error occurred');
          },
        });
    }
  }

  onCancel(): void {
    if (!this.event) return;
    this.showCancelConfirmation.set(true);
  }

  onCancelConfirmed(): void {
    if (!this.event) return;

    this.isCancelLoading.set(true);
    this.calendarService.cancelCalendarEvent(this.event.id).subscribe({
      next: (success) => {
        this.isCancelLoading.set(false);
        this.showCancelConfirmation.set(false);
        if (success) {
          this.eventCancelled.emit();
        } else {
          this.errorMessage.set('Failed to cancel event');
        }
      },
      error: () => {
        this.isCancelLoading.set(false);
        this.showCancelConfirmation.set(false);
        this.errorMessage.set('An error occurred');
      },
    });
  }

  onCancelDismissed(): void {
    this.showCancelConfirmation.set(false);
  }

  onDismiss(): void {
    this.dialogClosed.emit();
  }

  toggleAttendee(userId: string): void {
    const current = this.selectedAttendees();
    if (current.includes(userId)) {
      this.selectedAttendees.set(current.filter((id) => id !== userId));
    } else {
      this.selectedAttendees.set([...current, userId]);
    }
  }

  private formatDateTimeLocal(date: Date): string {
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }

  private formatDateOnly(date: Date): string {
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
  }
}
