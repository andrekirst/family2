import { Component, computed, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CalendarEventDto } from '../../services/calendar.service';

export interface CalendarDay {
  date: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  events: CalendarEventDto[];
}

@Component({
  selector: 'app-calendar-month-grid',
  standalone: true,
  imports: [CommonModule],
  template: `
    <!-- Day Headers -->
    <div class="grid grid-cols-7 border-b border-gray-200">
      @for (day of dayHeaders; track day) {
        <div
          class="py-2 text-center text-sm font-medium text-gray-500 border-r border-gray-200 last:border-r-0"
        >
          {{ day }}
        </div>
      }
    </div>

    <!-- Calendar Grid -->
    <div class="grid grid-cols-7 flex-1">
      @for (day of calendarDays(); track day.date.toISOString()) {
        <div
          class="min-h-[100px] border-r border-b border-gray-200 last:border-r-0 p-1 cursor-pointer hover:bg-gray-50 transition-colors"
          [class.bg-white]="day.isCurrentMonth"
          [class.bg-gray-50]="!day.isCurrentMonth"
          data-testid="calendar-day"
          (click)="onDayClick(day)"
        >
          <!-- Day Number -->
          <div class="flex justify-end">
            <span
              class="text-sm font-medium w-7 h-7 flex items-center justify-center rounded-full"
              [class.text-gray-900]="day.isCurrentMonth && !day.isToday"
              [class.text-gray-400]="!day.isCurrentMonth"
              [class.bg-blue-600]="day.isToday"
              [class.text-white]="day.isToday"
            >
              {{ day.date.getDate() }}
            </span>
          </div>

          <!-- Event Chips -->
          <div class="mt-1 space-y-0.5">
            @for (event of day.events.slice(0, 3); track event.id) {
              <div
                class="text-xs px-1.5 py-0.5 rounded border truncate cursor-pointer"
                class="bg-blue-100 text-blue-800 border-blue-200"
                [title]="event.title"
                data-testid="calendar-event-chip"
                (click)="onEventClick($event, event)"
              >
                {{ event.title }}
              </div>
            }
            @if (day.events.length > 3) {
              <div class="text-xs text-gray-500 px-1.5" data-testid="event-overflow">
                {{ moreEventsLabel(day.events.length - 3) }}
              </div>
            }
          </div>
        </div>
      }
    </div>
  `,
})
export class CalendarMonthGridComponent {
  readonly dayHeaders = [
    $localize`:@@calendar.dayMon:Mon`,
    $localize`:@@calendar.dayTue:Tue`,
    $localize`:@@calendar.dayWed:Wed`,
    $localize`:@@calendar.dayThu:Thu`,
    $localize`:@@calendar.dayFri:Fri`,
    $localize`:@@calendar.daySat:Sat`,
    $localize`:@@calendar.daySun:Sun`,
  ];

  month = signal<Date>(new Date());
  events = signal<CalendarEventDto[]>([]);

  @Input() set monthInput(value: Date) {
    this.month.set(value);
  }

  @Input() set eventsInput(value: CalendarEventDto[]) {
    this.events.set(value);
  }

  @Output() dayClicked = new EventEmitter<Date>();
  @Output() eventClicked = new EventEmitter<CalendarEventDto>();

  calendarDays = computed<CalendarDay[]>(() => {
    const current = this.month();
    const allEvents = this.events();
    const year = current.getFullYear();
    const month = current.getMonth();

    // First day of the month
    const firstDay = new Date(year, month, 1);
    // Last day of the month
    const lastDay = new Date(year, month + 1, 0);

    // Get the Monday before or on the first day (ISO week starts Monday)
    const startDate = new Date(firstDay);
    const dayOfWeek = startDate.getDay();
    const diff = dayOfWeek === 0 ? 6 : dayOfWeek - 1; // Convert to Monday-based
    startDate.setDate(startDate.getDate() - diff);

    // Get the Sunday after or on the last day
    const endDate = new Date(lastDay);
    const endDayOfWeek = endDate.getDay();
    const endDiff = endDayOfWeek === 0 ? 0 : 7 - endDayOfWeek;
    endDate.setDate(endDate.getDate() + endDiff);

    const days: CalendarDay[] = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const currentDate = new Date(startDate);
    while (currentDate <= endDate) {
      const date = new Date(currentDate);
      const dayStart = new Date(date);
      dayStart.setHours(0, 0, 0, 0);
      const dayEnd = new Date(date);
      dayEnd.setHours(23, 59, 59, 999);

      const dayEvents = allEvents.filter((event) => {
        const eventStart = new Date(event.startTime);
        const eventEnd = new Date(event.endTime);
        return eventStart <= dayEnd && eventEnd >= dayStart;
      });

      days.push({
        date,
        isCurrentMonth: date.getMonth() === month,
        isToday: date.getTime() === today.getTime(),
        events: dayEvents,
      });

      currentDate.setDate(currentDate.getDate() + 1);
    }

    return days;
  });

  moreEventsLabel(count: number): string {
    return $localize`:@@calendar.moreEvents:+${count}:count: more`;
  }

  onDayClick(day: CalendarDay): void {
    this.dayClicked.emit(day.date);
  }

  onEventClick(event: MouseEvent, calendarEvent: CalendarEventDto): void {
    event.stopPropagation(); // Prevent day click
    this.eventClicked.emit(calendarEvent);
  }
}
