import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { UserService } from '../../../../core/user/user.service';
import { CalendarService, CalendarEventDto } from '../../services/calendar.service';
import { CalendarMonthGridComponent } from '../calendar-month-grid/calendar-month-grid.component';
import { EventDialogComponent } from '../event-dialog/event-dialog.component';

@Component({
  selector: 'app-calendar-page',
  standalone: true,
  imports: [CommonModule, RouterModule, CalendarMonthGridComponent, EventDialogComponent],
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
          <button
            (click)="openCreateDialog()"
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 text-sm font-medium"
            data-testid="create-event-button"
          >
            + New Event
          </button>
        </div>
      </header>

      <!-- Month Navigation -->
      <div class="max-w-7xl mx-auto px-4 py-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-center gap-4 mb-4">
          <button
            (click)="previousMonth()"
            class="p-2 rounded-lg hover:bg-gray-200 transition-colors"
            data-testid="prev-month"
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
            [disabled]="isCurrentMonth()"
            class="px-3 py-1.5 text-sm font-medium rounded-lg transition-colors"
            [ngClass]="{
              'text-gray-400 cursor-not-allowed': isCurrentMonth(),
              'text-blue-600 hover:bg-blue-50': !isCurrentMonth(),
            }"
            data-testid="today-button"
          >
            Today
          </button>

          <h2 class="text-xl font-semibold text-gray-900" data-testid="current-month-label">
            {{ monthLabel() }}
          </h2>

          <button
            (click)="nextMonth()"
            class="p-2 rounded-lg hover:bg-gray-200 transition-colors"
            data-testid="next-month"
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

  isCurrentMonth = computed(() => {
    const current = this.currentMonth();
    const now = new Date();
    return current.getFullYear() === now.getFullYear() && current.getMonth() === now.getMonth();
  });

  async ngOnInit(): Promise<void> {
    await this.loadEvents();
  }

  goToToday(): void {
    if (this.isCurrentMonth()) return;
    this.currentMonth.set(new Date(new Date().getFullYear(), new Date().getMonth(), 1));
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
