import { Component, inject, signal, OnInit, OnDestroy, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { UserService } from '../../../../core/user/user.service';
import { CalendarService, CalendarEventDto } from '../../services/calendar.service';
import { CalendarMonthGridComponent } from '../calendar-month-grid/calendar-month-grid.component';
import { EventDialogComponent } from '../event-dialog/event-dialog.component';
import { TopBarService } from '../../../../shared/services/top-bar.service';

@Component({
  selector: 'app-calendar-page',
  standalone: true,
  imports: [CommonModule, RouterModule, CalendarMonthGridComponent, EventDialogComponent],
  template: `
    <!-- Month Navigation -->
    <div class="flex items-center justify-between mb-4">
      <button
        (click)="previousMonth()"
        class="p-2 rounded-lg hover:bg-gray-200 transition-colors"
        data-testid="prev-month"
      >
        <svg class="h-5 w-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M15 19l-7-7 7-7"
          />
        </svg>
      </button>

      <h2 class="text-xl font-semibold text-gray-900" data-testid="current-month-label">
        {{ monthLabel() }}
      </h2>

      <button
        (click)="nextMonth()"
        class="p-2 rounded-lg hover:bg-gray-200 transition-colors"
        data-testid="next-month"
      >
        <svg class="h-5 w-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
        </svg>
      </button>
    </div>

    <!-- Loading State -->
    @if (isLoading()) {
      <div class="bg-white shadow rounded-lg p-6">
        <div class="animate-pulse">
          <div class="h-6 bg-gray-200 rounded w-1/3 mb-4"></div>
          <div class="grid grid-cols-7 gap-2">
            @for (i of [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]; track i) {
              <div class="h-24 bg-gray-200 rounded"></div>
            }
          </div>
        </div>
      </div>
    } @else {
      <!-- Calendar Grid -->
      <div class="bg-white shadow rounded-lg overflow-hidden" data-testid="calendar-grid">
        <app-calendar-month-grid
          [monthInput]="currentMonth()"
          [eventsInput]="events()"
          (dayClicked)="onDayClicked($event)"
          (eventClicked)="onEventClicked($event)"
        />
      </div>
    }

    <!-- Event Dialog -->
    @if (showCreateDialog()) {
      <app-event-dialog
        [selectedDate]="selectedDate()"
        (eventCreated)="onEventCreated()"
        (dialogClosed)="onDialogClosed()"
      />
    }

    @if (editingEvent()) {
      <app-event-dialog
        [event]="editingEvent()"
        (eventUpdated)="onEventUpdated()"
        (eventCancelled)="onEventCancelled()"
        (dialogClosed)="onEditDialogClosed()"
      />
    }
  `,
})
export class CalendarPageComponent implements OnInit, OnDestroy {
  private calendarService = inject(CalendarService);
  private userService = inject(UserService);
  private router = inject(Router);
  private topBarService = inject(TopBarService);

  currentMonth = signal<Date>(new Date());
  events = signal<CalendarEventDto[]>([]);
  isLoading = signal(false);
  selectedDate = signal<Date | null>(null);
  showCreateDialog = signal(false);
  editingEvent = signal<CalendarEventDto | null>(null);

  monthLabel = computed(() => {
    const date = this.currentMonth();
    return date.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  });

  async ngOnInit(): Promise<void> {
    this.topBarService.setConfig({
      title: 'Family Calendar',
      actions: [
        {
          id: 'create-event',
          label: '+ New Event',
          onClick: () => this.openCreateDialog(),
          variant: 'primary',
          testId: 'create-event-button',
        },
      ],
    });

    await this.loadEvents();
  }

  ngOnDestroy(): void {
    this.topBarService.clear();
  }

  previousMonth(): void {
    const current = this.currentMonth();
    this.currentMonth.set(new Date(current.getFullYear(), current.getMonth() - 1, 1));
    this.loadEvents();
  }

  nextMonth(): void {
    const current = this.currentMonth();
    this.currentMonth.set(new Date(current.getFullYear(), current.getMonth() + 1, 1));
    this.loadEvents();
  }

  onDayClicked(date: Date): void {
    this.selectedDate.set(date);
    this.showCreateDialog.set(true);
  }

  onEventClicked(event: CalendarEventDto): void {
    this.editingEvent.set(event);
  }

  openCreateDialog(): void {
    this.selectedDate.set(new Date());
    this.showCreateDialog.set(true);
  }

  onEventCreated(): void {
    this.showCreateDialog.set(false);
    this.selectedDate.set(null);
    this.loadEvents();
  }

  onEventUpdated(): void {
    this.editingEvent.set(null);
    this.loadEvents();
  }

  onEventCancelled(): void {
    this.editingEvent.set(null);
    this.loadEvents();
  }

  onDialogClosed(): void {
    this.showCreateDialog.set(false);
    this.selectedDate.set(null);
  }

  onEditDialogClosed(): void {
    this.editingEvent.set(null);
  }

  private loadEvents(): void {
    const user = this.userService.currentUser();
    if (!user?.familyId) return;

    const current = this.currentMonth();
    const year = current.getFullYear();
    const month = current.getMonth();

    // Fetch a wider range to include events visible in adjacent month days
    const startDate = new Date(year, month - 1, 20);
    const endDate = new Date(year, month + 1, 12);

    this.isLoading.set(true);

    this.calendarService
      .getCalendarEvents(user.familyId, startDate.toISOString(), endDate.toISOString())
      .subscribe({
        next: (events) => {
          this.events.set(events);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        },
      });
  }
}
