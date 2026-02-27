import {
  Component,
  inject,
  signal,
  OnInit,
  OnDestroy,
  computed,
  ViewChild,
  TemplateRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { UserService } from '../../../../core/user/user.service';
import { CalendarService, CalendarEventDto } from '../../services/calendar.service';
import { CalendarMonthGridComponent } from '../calendar-month-grid/calendar-month-grid.component';
import { CalendarWeekGridComponent } from '../calendar-week-grid/calendar-week-grid.component';
import { CalendarWeekSkeletonComponent } from '../calendar-week-skeleton/calendar-week-skeleton.component';
import { CalendarViewSwitcherComponent } from '../calendar-view-switcher/calendar-view-switcher.component';
import { EventContextComponent } from '../event-context/event-context.component';
import { TopBarService } from '../../../../shared/services/top-bar.service';
import { ContextPanelService } from '../../../../shared/services/context-panel.service';
import { CalendarViewMode, AGENDA_CONSTANTS, TimeRange } from '../../models/calendar.models';
import { getWeekStart, getWeekEnd, formatWeekLabel, getStoredLocale } from '../../utils/week.utils';
import { getDayStart, getDayEnd, formatDayLabel } from '../../utils/day.utils';
import { getAgendaDateRange } from '../../utils/agenda.utils';
import { CalendarDayGridComponent } from '../calendar-day-grid/calendar-day-grid.component';
import { CalendarDaySkeletonComponent } from '../calendar-day-skeleton/calendar-day-skeleton.component';
import { CalendarAgendaComponent } from '../calendar-agenda/calendar-agenda.component';

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
    EventContextComponent,
    CalendarDayGridComponent,
    CalendarDaySkeletonComponent,
    CalendarAgendaComponent,
  ],
  template: `
    <div class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8 w-full">
      <!-- Navigation -->
      <div class="flex items-center justify-between mb-4">
        <div class="flex items-center gap-2">
          @if (viewMode() !== 'agenda') {
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
          }
          <button
            (click)="goToToday()"
            class="px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            data-testid="today-button"
            i18n="@@calendar.today"
          >
            Today
          </button>
          @if (viewMode() !== 'agenda') {
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
          }
        </div>

        <h2 class="text-xl font-semibold text-gray-900" data-testid="current-period-label">
          {{ navigationLabel() }}
        </h2>

        <app-calendar-view-switcher
          [activeView]="viewMode()"
          (viewChanged)="onViewModeChanged($event)"
        />
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
              (dayClicked)="onMonthDayCellClicked($event)"
              (dateRangeSelected)="onDateRangeSelected($event)"
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
              (timeRangeSelected)="onTimeRangeSelected($event)"
              (dateRangeSelected)="onDateRangeSelected($event)"
              (eventClicked)="onEventClicked($event)"
              (dayHeaderClicked)="onDayHeaderClickedInWeek($event)"
            />
          </div>
        }
      }

      <!-- Day View -->
      @if (viewMode() === 'day') {
        <div class="bg-white shadow rounded-lg overflow-hidden" data-testid="calendar-grid">
          <app-calendar-day-grid
            [selectedDateInput]="currentDay()"
            [eventsInput]="events()"
            [loadingInput]="isLoading()"
            (timeSlotClicked)="onTimeSlotClicked($event)"
            (timeRangeSelected)="onTimeRangeSelected($event)"
            (eventClicked)="onEventClicked($event)"
          />
        </div>
      }

      <!-- Agenda View -->
      @if (viewMode() === 'agenda') {
        @if (isLoading() && agendaBatchCount() === 1) {
          <div class="bg-white shadow rounded-lg p-6">
            <div class="animate-pulse space-y-4">
              @for (i of [1, 2, 3, 4, 5]; track i) {
                <div>
                  <div class="h-4 bg-gray-200 rounded w-1/4 mb-2"></div>
                  <div class="h-12 bg-gray-200 rounded"></div>
                </div>
              }
            </div>
          </div>
        } @else {
          <div class="bg-white shadow rounded-lg overflow-hidden" data-testid="calendar-grid">
            <app-calendar-agenda
              [eventsInput]="agendaEvents()"
              [batchCountInput]="agendaBatchCount()"
              [loadingInput]="isLoading()"
              [loadingMoreInput]="agendaLoadingMore()"
              [hasMoreInput]="agendaHasMore()"
              (eventClicked)="onEventClicked($event)"
              (loadMore)="onLoadMoreAgenda()"
            />
          </div>
        }
      }

      <!-- Context panel content template -->
      <ng-template #eventContextTemplate>
        @if (contextEvent()) {
          <app-event-context
            [event]="contextEvent()"
            (eventUpdated)="onEventUpdated()"
            (eventCancelled)="onEventCancelled()"
          />
        } @else if (selectedDate() || selectedStartDate() || selectedEndDate()) {
          <app-event-context
            [selectedDate]="selectedDate()"
            [selectedStartDate]="selectedStartDate()"
            [selectedEndDate]="selectedEndDate()"
            [isAllDaySelection]="isAllDaySelection()"
            (eventCreated)="onEventCreated($event)"
          />
        }
      </ng-template>
    </div>
  `,
})
export class CalendarPageComponent implements OnInit, OnDestroy {
  private calendarService = inject(CalendarService);
  private userService = inject(UserService);
  private router = inject(Router);
  private topBarService = inject(TopBarService);
  private contextPanelService = inject(ContextPanelService);

  @ViewChild('eventContextTemplate') eventContextTemplate!: TemplateRef<unknown>;

  viewMode = signal<CalendarViewMode>('month');
  currentMonth = signal<Date>(new Date());
  currentWeek = signal<Date>(getWeekStart(new Date()));
  currentDay = signal<Date>(new Date());
  events = signal<CalendarEventDto[]>([]);
  isLoading = signal(false);
  selectedDate = signal<Date | null>(null);
  selectedStartDate = signal<Date | null>(null);
  selectedEndDate = signal<Date | null>(null);
  isAllDaySelection = signal(false);
  contextEvent = signal<CalendarEventDto | null>(null);

  // Agenda-specific state
  agendaEvents = signal<CalendarEventDto[]>([]);
  agendaBatchCount = signal<number>(1);
  agendaLoadingMore = signal<boolean>(false);
  agendaHasMore = signal<boolean>(true);

  monthLabel = computed(() => {
    const date = this.currentMonth();
    return date.toLocaleDateString(getStoredLocale(), { month: 'long', year: 'numeric' });
  });

  weekLabel = computed(() => {
    const start = this.currentWeek();
    return formatWeekLabel(start, getWeekEnd(start));
  });

  dayLabel = computed(() => formatDayLabel(this.currentDay()));

  agendaLabel = $localize`:@@calendar.agenda.upcomingEvents:Upcoming Events`;

  navigationLabel = computed(() => {
    const mode = this.viewMode();
    if (mode === 'month') return this.monthLabel();
    if (mode === 'day') return this.dayLabel();
    if (mode === 'agenda') return this.agendaLabel;
    return this.weekLabel();
  });

  async ngOnInit(): Promise<void> {
    this.topBarService.setConfig({
      title: $localize`:@@calendar.title:Family Calendar`,
      actions: [
        {
          id: 'create-event',
          label: $localize`:@@calendar.newEvent:+ New Event`,
          onClick: () => this.openCreatePanel(),
          variant: 'primary',
          testId: 'create-event-button',
        },
      ],
    });

    await this.loadEvents();
  }

  ngOnDestroy(): void {
    this.topBarService.clear();
    this.contextPanelService.close();
  }

  onViewModeChanged(mode: CalendarViewMode): void {
    if (mode === this.viewMode()) return;

    const current = this.viewMode();

    if (mode === 'agenda') {
      // Switching to agenda: reset batch count and load fresh
      this.agendaBatchCount.set(1);
      this.agendaHasMore.set(true);
      this.viewMode.set(mode);
      this.loadAgendaEvents();
      return;
    }

    if (mode === 'day') {
      // Switching to day view
      if (current === 'week') {
        // Use the week's Monday as the starting day
        this.currentDay.set(new Date(this.currentWeek()));
      } else if (current === 'agenda') {
        // From agenda: use today
        this.currentDay.set(new Date());
      } else {
        // Switching from month → day: use today if current month, else 1st of month
        const now = new Date();
        const cm = this.currentMonth();
        if (now.getFullYear() === cm.getFullYear() && now.getMonth() === cm.getMonth()) {
          this.currentDay.set(new Date(now));
        } else {
          this.currentDay.set(new Date(cm.getFullYear(), cm.getMonth(), 1));
        }
      }
    } else if (mode === 'week') {
      if (current === 'day') {
        // Switching day → week: derive week from currentDay
        this.currentWeek.set(getWeekStart(this.currentDay()));
      } else if (current === 'agenda') {
        // From agenda: use current week
        this.currentWeek.set(getWeekStart(new Date()));
      } else {
        // Switching month → week: use today if current month contains today, else 1st of month
        const now = new Date();
        const cm = this.currentMonth();
        if (now.getFullYear() === cm.getFullYear() && now.getMonth() === cm.getMonth()) {
          this.currentWeek.set(getWeekStart(now));
        } else {
          this.currentWeek.set(getWeekStart(cm));
        }
      }
    } else {
      // Switching to month view
      if (current === 'day') {
        // Derive month from currentDay
        const day = this.currentDay();
        this.currentMonth.set(new Date(day.getFullYear(), day.getMonth(), 1));
      } else if (current === 'agenda') {
        // From agenda: use current month
        const now = new Date();
        this.currentMonth.set(new Date(now.getFullYear(), now.getMonth(), 1));
      } else {
        // Switching week → month: use the month containing the week's Monday
        const weekMon = this.currentWeek();
        this.currentMonth.set(new Date(weekMon.getFullYear(), weekMon.getMonth(), 1));
      }
    }

    this.viewMode.set(mode);
    this.loadEvents();
  }

  previousPeriod(): void {
    if (this.viewMode() === 'month') {
      this.previousMonth();
    } else if (this.viewMode() === 'day') {
      this.prevDay();
    } else {
      this.previousWeek();
    }
  }

  nextPeriod(): void {
    if (this.viewMode() === 'month') {
      this.nextMonth();
    } else if (this.viewMode() === 'day') {
      this.nextDay();
    } else {
      this.nextWeek();
    }
  }

  goToToday(): void {
    const now = new Date();
    this.currentMonth.set(new Date(now.getFullYear(), now.getMonth(), 1));
    this.currentWeek.set(getWeekStart(now));
    this.currentDay.set(new Date(now));

    if (this.viewMode() === 'agenda') {
      this.agendaBatchCount.set(1);
      this.agendaHasMore.set(true);
      this.loadAgendaEvents();
    } else {
      this.loadEvents();
    }
  }

  prevDay(): void {
    const current = this.currentDay();
    const prev = new Date(current);
    prev.setDate(prev.getDate() - 1);
    this.currentDay.set(prev);
    this.loadEvents();
  }

  nextDay(): void {
    const current = this.currentDay();
    const next = new Date(current);
    next.setDate(next.getDate() + 1);
    this.currentDay.set(next);
    this.loadEvents();
  }

  onDayHeaderClickedInWeek(date: Date): void {
    this.currentDay.set(new Date(date));
    this.viewMode.set('day');
    this.loadEvents();
  }

  onMonthDayCellClicked(date: Date): void {
    this.currentDay.set(new Date(date));
    this.viewMode.set('day');
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
    this.contextEvent.set(null);
    this.selectedDate.set(date);
    this.selectedStartDate.set(null);
    this.selectedEndDate.set(null);
    this.isAllDaySelection.set(false);
    this.contextPanelService.open(this.eventContextTemplate);
  }

  onTimeSlotClicked(date: Date): void {
    this.contextEvent.set(null);
    this.selectedDate.set(date);
    this.selectedStartDate.set(null);
    this.selectedEndDate.set(null);
    this.isAllDaySelection.set(false);
    this.contextPanelService.open(this.eventContextTemplate);
  }

  onTimeRangeSelected(range: TimeRange): void {
    this.contextEvent.set(null);
    this.selectedDate.set(null);
    this.isAllDaySelection.set(false);
    this.selectedStartDate.set(range.start);
    this.selectedEndDate.set(range.end);
    this.contextPanelService.open(this.eventContextTemplate);
  }

  onDateRangeSelected(range: TimeRange): void {
    this.contextEvent.set(null);
    this.selectedDate.set(null);
    this.isAllDaySelection.set(true);
    this.selectedStartDate.set(range.start);
    this.selectedEndDate.set(range.end);
    this.contextPanelService.open(this.eventContextTemplate);
  }

  onEventClicked(event: CalendarEventDto): void {
    this.selectedDate.set(null);
    this.selectedStartDate.set(null);
    this.selectedEndDate.set(null);
    this.contextEvent.set(event);
    this.contextPanelService.open(this.eventContextTemplate, event.id);
  }

  openCreatePanel(): void {
    this.contextEvent.set(null);
    this.selectedDate.set(new Date());
    this.selectedStartDate.set(null);
    this.selectedEndDate.set(null);
    this.isAllDaySelection.set(false);
    this.contextPanelService.open(this.eventContextTemplate);
  }

  onLoadMoreAgenda(): void {
    const newBatch = this.agendaBatchCount() + 1;
    if (newBatch > AGENDA_CONSTANTS.MAX_BATCHES) {
      this.agendaHasMore.set(false);
      return;
    }
    this.agendaBatchCount.set(newBatch);
    this.loadAgendaEvents(true);
  }

  onEventCreated(event: CalendarEventDto): void {
    this.contextEvent.set(event);
    this.selectedDate.set(null);
    this.selectedStartDate.set(null);
    this.selectedEndDate.set(null);
    this.reloadCurrentView();
  }

  onEventUpdated(): void {
    this.reloadCurrentView();
  }

  onEventCancelled(): void {
    this.reloadCurrentView();
  }

  private reloadCurrentView(): void {
    if (this.viewMode() === 'agenda') {
      this.loadAgendaEvents();
    } else {
      this.loadEvents();
    }
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
    } else if (this.viewMode() === 'day') {
      startDate = getDayStart(this.currentDay());
      endDate = getDayEnd(this.currentDay());
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

  private loadAgendaEvents(isLoadMore = false): void {
    const user = this.userService.currentUser();
    if (!user?.familyId) return;

    const { start, end } = getAgendaDateRange(this.agendaBatchCount());

    if (isLoadMore) {
      this.agendaLoadingMore.set(true);
    } else {
      this.isLoading.set(true);
    }

    this.calendarService
      .getCalendarEvents(user.familyId, start.toISOString(), end.toISOString())
      .subscribe({
        next: (events) => {
          this.agendaEvents.set(events);
          this.agendaLoadingMore.set(false);
          this.isLoading.set(false);

          if (this.agendaBatchCount() >= AGENDA_CONSTANTS.MAX_BATCHES) {
            this.agendaHasMore.set(false);
          }
        },
        error: () => {
          this.agendaLoadingMore.set(false);
          this.isLoading.set(false);
        },
      });
  }
}
