import {
  Component,
  computed,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
  Renderer2,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { CalendarEventDto } from '../../services/calendar.service';
import { TimeRange } from '../../models/calendar.models';

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
    <div class="grid grid-cols-7 flex-1 select-none">
      @for (day of calendarDays(); track day.date.toISOString(); let idx = $index) {
        <div
          class="min-h-[100px] border-r border-b border-gray-200 last:border-r-0 p-1 cursor-pointer hover:bg-gray-50 transition-colors"
          [class.bg-white]="day.isCurrentMonth && !isDayInDragRange(idx)"
          [class.bg-gray-50]="!day.isCurrentMonth && !isDayInDragRange(idx)"
          [class.!bg-blue-100]="isDayInDragRange(idx)"
          [attr.data-day-index]="idx"
          data-testid="calendar-day"
          (mousedown)="onDayMouseDown($event, idx)"
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
export class CalendarMonthGridComponent implements OnDestroy {
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
  @Output() dateRangeSelected = new EventEmitter<TimeRange>();
  @Output() eventClicked = new EventEmitter<CalendarEventDto>();

  // Drag state
  isDragging = signal(false);
  dragStartIndex = signal<number | null>(null);
  dragCurrentIndex = signal<number | null>(null);
  private mouseMoveUnlisten?: () => void;
  private mouseUpUnlisten?: () => void;

  private dragRange = computed(() => {
    const start = this.dragStartIndex();
    const current = this.dragCurrentIndex();
    if (start === null || current === null) return { min: -1, max: -1 };
    return { min: Math.min(start, current), max: Math.max(start, current) };
  });

  constructor(private renderer: Renderer2) {}

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

  isDayInDragRange(index: number): boolean {
    if (!this.isDragging()) return false;
    const range = this.dragRange();
    return index >= range.min && index <= range.max;
  }

  ngOnDestroy(): void {
    this.mouseMoveUnlisten?.();
    this.mouseUpUnlisten?.();
  }

  onDayMouseDown(mouseEvent: MouseEvent, index: number): void {
    const target = mouseEvent.target as HTMLElement;
    if (target.closest('[data-testid="calendar-event-chip"]')) return;

    this.isDragging.set(true);
    this.dragStartIndex.set(index);
    this.dragCurrentIndex.set(index);

    this.mouseMoveUnlisten = this.renderer.listen('document', 'mousemove', (e: MouseEvent) => {
      this.onMouseMove(e);
    });

    this.mouseUpUnlisten = this.renderer.listen('document', 'mouseup', () => {
      this.onMouseUp();
    });

    mouseEvent.preventDefault();
  }

  private onMouseMove(mouseEvent: MouseEvent): void {
    if (!this.isDragging()) return;

    const el = document.elementFromPoint(mouseEvent.clientX, mouseEvent.clientY);
    const dayCell = el?.closest('[data-day-index]') as HTMLElement | null;
    if (dayCell) {
      const idx = parseInt(dayCell.getAttribute('data-day-index')!, 10);
      this.dragCurrentIndex.set(idx);
    }
  }

  private onMouseUp(): void {
    if (!this.isDragging()) return;

    const startIdx = this.dragStartIndex();
    const currentIdx = this.dragCurrentIndex();

    this.isDragging.set(false);

    this.mouseMoveUnlisten?.();
    this.mouseMoveUnlisten = undefined;
    this.mouseUpUnlisten?.();
    this.mouseUpUnlisten = undefined;

    if (startIdx === null || currentIdx === null) return;

    const days = this.calendarDays();

    if (startIdx === currentIdx) {
      // Single day click — use existing behavior
      this.dayClicked.emit(days[startIdx].date);
    } else {
      // Multi-day drag — emit date range as all-day event
      const minIdx = Math.min(startIdx, currentIdx);
      const maxIdx = Math.max(startIdx, currentIdx);

      const startDate = new Date(days[minIdx].date);
      startDate.setHours(0, 0, 0, 0);

      const endDate = new Date(days[maxIdx].date);
      endDate.setHours(23, 59, 59, 999);

      this.dateRangeSelected.emit({ start: startDate, end: endDate });
    }

    this.dragStartIndex.set(null);
    this.dragCurrentIndex.set(null);
  }

  onEventClick(event: MouseEvent, calendarEvent: CalendarEventDto): void {
    event.stopPropagation();
    this.eventClicked.emit(calendarEvent);
  }
}
