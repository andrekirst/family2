import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { UserService } from '../../../../core/user/user.service';
import { CalendarService, CalendarEventDto } from '../../services/calendar.service';
import { CalendarMonthGridComponent } from '../calendar-month-grid/calendar-month-grid.component';
import { CalendarWeekGridComponent } from '../calendar-week-grid/calendar-week-grid.component';
import { CalendarWeekSkeletonComponent } from '../calendar-week-skeleton/calendar-week-skeleton.component';
import { CalendarViewSwitcherComponent } from '../calendar-view-switcher/calendar-view-switcher.component';
import { EventDialogComponent } from '../event-dialog/event-dialog.component';
import { CalendarViewMode } from '../../models/calendar.models';
import { getWeekStart, getWeekEnd, formatWeekLabel } from '../../utils/week.utils';

@Component({
  selector: 'app-calendar-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    CalendarMonthGridComponent,
    CalendarWeekGridComponent,
    CalendarWeekSkeletonComponent,
    CalendarViewSwitcherComponent,
    EventDialogComponent,
  ],
  template: `
    <div class="min-h-screen bg-gray-50">
      <!-- Header -->
      <header class="bg-white shadow">
        <div class="max-w-7xl mx-auto px-4 py-4 sm:px-6 lg:px-8 flex justify-between items-center">
          <div class="flex items-center gap-4">
            <a
              routerLink="/dashboard"
              class="text-gray-500 hover:text-gray-700"
              data-testid="back-to-dashboard"
            >
              <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M15 19l-7-7 7-7"
                />
              </svg>
            </a>
            <h1 class="text-2xl font-bold text-gray-900">Family Calendar</h1>
          </div>
          <div class="flex items-center gap-3">
            <app-calendar-view-switcher
              [activeView]="viewMode()"
              (viewChanged)="onViewModeChanged($event)"
            />
            <button
              (click)="openCreateDialog()"
              class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 text-sm font-medium"
              data-testid="create-event-button"
            >
              + New Event
            </button>
          </div>
        </div>
      </header>

      <!-- Navigation -->
      <div class="max-w-7xl mx-auto px-4 py-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-2">
            <button
              (click)="previousPeriod()"
              class="p-2 rounded-lg hover:bg-gray-200 transition-colors"
              data-testid="prev-period"
            >
              <svg
                class="h-5 w-5 text-gray-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M15 19l-7-7 7-7"
                />
              </svg>
            </button>
            <button
              (click)="goToToday()"
              class="px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
              data-testid="today-button"
            >
              Today
            </button>
            <button
              (click)="nextPeriod()"
              class="p-2 rounded-lg hover:bg-gray-200 transition-colors"
              data-testid="next-period"
            >
              <svg
                class="h-5 w-5 text-gray-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M9 5l7 7-7 7"
                />
              </svg>
            </button>
          </div>

          <h2 class="text-xl font-semibold text-gray-900" data-testid="current-period-label">
            {{ navigationLabel() }}
          </h2>

          <!-- Spacer to balance layout -->
          <div class="w-[140px]"></div>
        </div>

        <!-- Month View -->
        @if (viewMode() === 'month') {
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
            <div class="bg-white shadow rounded-lg overflow-hidden" data-testid="calendar-grid">
              <app-calendar-month-grid
                [monthInput]="currentMonth()"
                [eventsInput]="events()"
                (dayClicked)="onDayClicked($event)"
                (eventClicked)="onEventClicked($event)"
              />
            </div>
          }
        }

        <!-- Week View -->
        @if (viewMode() === 'week') {
          @if (isLoading()) {
            <app-calendar-week-skeleton />
          } @else {
            <div class="bg-white shadow rounded-lg overflow-hidden" data-testid="calendar-grid">
              <app-calendar-week-grid
                [weekStartInput]="currentWeek()"
                [eventsInput]="events()"
                (timeSlotClicked)="onTimeSlotClicked($event)"
                (eventClicked)="onEventClicked($event)"
              />
            </div>
          }
        }
      </div>

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
    </div>
  `,
})
export class CalendarPageComponent implements OnInit {
  private calendarService = inject(CalendarService);
  private userService = inject(UserService);
  private router = inject(Router);

  viewMode = signal<CalendarViewMode>('month');
  currentMonth = signal<Date>(new Date());
  currentWeek = signal<Date>(getWeekStart(new Date()));
  events = signal<CalendarEventDto[]>([]);
  isLoading = signal(false);
  selectedDate = signal<Date | null>(null);
  showCreateDialog = signal(false);
  editingEvent = signal<CalendarEventDto | null>(null);

  monthLabel = computed(() => {
    const date = this.currentMonth();
    return date.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  });

  weekLabel = computed(() => {
    const start = this.currentWeek();
    return formatWeekLabel(start, getWeekEnd(start));
  });

  navigationLabel = computed(() =>
    this.viewMode() === 'month' ? this.monthLabel() : this.weekLabel(),
  );

  async ngOnInit(): Promise<void> {
    await this.loadEvents();
  }

  onViewModeChanged(mode: CalendarViewMode): void {
    if (mode === this.viewMode()) return;

    if (mode === 'week') {
      // Switching month → week: use today if current month contains today, else 1st of month
      const now = new Date();
      const cm = this.currentMonth();
      if (now.getFullYear() === cm.getFullYear() && now.getMonth() === cm.getMonth()) {
        this.currentWeek.set(getWeekStart(now));
      } else {
        this.currentWeek.set(getWeekStart(cm));
      }
    } else {
      // Switching week → month: use the month containing the week's Monday
      const weekMon = this.currentWeek();
      this.currentMonth.set(new Date(weekMon.getFullYear(), weekMon.getMonth(), 1));
    }

    this.viewMode.set(mode);
    this.loadEvents();
  }

  previousPeriod(): void {
    if (this.viewMode() === 'month') {
      this.previousMonth();
    } else {
      this.previousWeek();
    }
  }

  nextPeriod(): void {
    if (this.viewMode() === 'month') {
      this.nextMonth();
    } else {
      this.nextWeek();
    }
  }

  goToToday(): void {
    const now = new Date();
    this.currentMonth.set(new Date(now.getFullYear(), now.getMonth(), 1));
    this.currentWeek.set(getWeekStart(now));
    this.loadEvents();
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

  previousWeek(): void {
    const current = this.currentWeek();
    const prev = new Date(current);
    prev.setDate(prev.getDate() - 7);
    this.currentWeek.set(prev);
    this.loadEvents();
  }

  nextWeek(): void {
    const current = this.currentWeek();
    const next = new Date(current);
    next.setDate(next.getDate() + 7);
    this.currentWeek.set(next);
    this.loadEvents();
  }

  onDayClicked(date: Date): void {
    this.selectedDate.set(date);
    this.showCreateDialog.set(true);
  }

  onTimeSlotClicked(date: Date): void {
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

    let startDate: Date;
    let endDate: Date;

    if (this.viewMode() === 'month') {
      const current = this.currentMonth();
      const year = current.getFullYear();
      const month = current.getMonth();
      // Fetch a wider range to include events visible in adjacent month days
      startDate = new Date(year, month - 1, 20);
      endDate = new Date(year, month + 1, 12);
    } else {
      startDate = this.currentWeek();
      endDate = getWeekEnd(this.currentWeek());
    }

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
