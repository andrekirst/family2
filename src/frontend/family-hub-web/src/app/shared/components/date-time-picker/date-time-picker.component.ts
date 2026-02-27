import {
  Component,
  Input,
  Output,
  EventEmitter,
  signal,
  computed,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';

export interface DateTimeChangeEvent {
  startTime: string;
  endTime: string;
  isAllDay: boolean;
}

interface CalendarDay {
  date: Date;
  dayNumber: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  isSelected: boolean;
  isInRange: boolean;
}

@Component({
  selector: 'app-date-time-picker',
  standalone: true,
  imports: [],
  template: `
    <div [attr.data-testid]="testId">
      <!-- Month navigation -->
      <div class="flex items-center justify-between mb-3">
        <button
          type="button"
          (click)="previousMonth()"
          [disabled]="disabled"
          class="p-1 rounded hover:bg-gray-100 transition-colors disabled:opacity-50 disabled:cursor-default"
          [attr.data-testid]="testId + '-prev-month'"
        >
          <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 19l-7-7 7-7"
            />
          </svg>
        </button>
        <span
          class="text-sm font-semibold text-gray-900"
          [attr.data-testid]="testId + '-month-label'"
        >
          {{ monthLabel() }}
        </span>
        <button
          type="button"
          (click)="nextMonth()"
          [disabled]="disabled"
          class="p-1 rounded hover:bg-gray-100 transition-colors disabled:opacity-50 disabled:cursor-default"
          [attr.data-testid]="testId + '-next-month'"
        >
          <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M9 5l7 7-7 7"
            />
          </svg>
        </button>
      </div>

      <!-- Day-of-week headers -->
      <div class="grid grid-cols-7 gap-0 mb-1">
        @for (day of weekDays; track day) {
          <div class="text-center text-xs font-medium text-gray-500 py-1">{{ day }}</div>
        }
      </div>

      <!-- Calendar grid -->
      <div class="grid grid-cols-7 gap-0" [attr.data-testid]="testId + '-calendar-grid'">
        @for (day of calendarDays(); track day.date.getTime()) {
          <button
            type="button"
            (click)="selectDate(day.date)"
            [disabled]="disabled"
            class="h-8 w-full text-xs rounded-md transition-colors disabled:cursor-default"
            [class.text-gray-900]="day.isCurrentMonth && !day.isSelected && !day.isInRange"
            [class.text-gray-400]="!day.isCurrentMonth && !day.isSelected && !day.isInRange"
            [class.font-bold]="day.isToday"
            [class.bg-blue-600]="day.isSelected"
            [class.text-white]="day.isSelected"
            [class.bg-blue-100]="day.isInRange && !day.isSelected"
            [class.text-blue-800]="day.isInRange && !day.isSelected"
            [class.hover:bg-gray-100]="!day.isSelected && !day.isInRange && !disabled"
          >
            {{ day.dayNumber }}
          </button>
        }
      </div>

      <!-- All-day toggle -->
      <div class="flex items-center gap-2 mt-3 pt-3 border-t border-gray-200">
        <input
          type="checkbox"
          [checked]="editAllDay()"
          (change)="onAllDayToggle($any($event.target).checked)"
          [disabled]="disabled"
          id="allday-toggle"
          class="h-4 w-4 text-blue-600 border-gray-300 rounded"
          [attr.data-testid]="testId + '-allday'"
        />
        <label for="allday-toggle" class="text-sm text-gray-700" i18n="@@calendar.event.allDay"
          >All day event</label
        >
      </div>

      <!-- Time inputs -->
      @if (!editAllDay()) {
        <div class="mt-3 space-y-2" [attr.data-testid]="testId + '-time-inputs'">
          <!-- Start time -->
          <div class="flex items-center gap-2">
            <span class="text-xs text-gray-500 w-10" i18n="@@calendar.event.start">Start</span>
            <button
              type="button"
              (click)="adjustTime('start', -15)"
              [disabled]="disabled"
              class="p-1 rounded hover:bg-gray-100 text-gray-500 transition-colors disabled:opacity-50 disabled:cursor-default"
              [attr.data-testid]="testId + '-start-minus'"
            >
              &minus;
            </button>
            <span
              class="text-sm font-mono w-16 text-center"
              [attr.data-testid]="testId + '-start-time'"
            >
              {{ formatTimeOnly(editStartTime()) }}
            </span>
            <button
              type="button"
              (click)="adjustTime('start', 15)"
              [disabled]="disabled"
              class="p-1 rounded hover:bg-gray-100 text-gray-500 transition-colors disabled:opacity-50 disabled:cursor-default"
              [attr.data-testid]="testId + '-start-plus'"
            >
              +
            </button>
          </div>
          <!-- End time -->
          <div class="flex items-center gap-2">
            <span class="text-xs text-gray-500 w-10" i18n="@@calendar.event.end">End</span>
            <button
              type="button"
              (click)="adjustTime('end', -15)"
              [disabled]="disabled"
              class="p-1 rounded hover:bg-gray-100 text-gray-500 transition-colors disabled:opacity-50 disabled:cursor-default"
              [attr.data-testid]="testId + '-end-minus'"
            >
              &minus;
            </button>
            <span
              class="text-sm font-mono w-16 text-center"
              [attr.data-testid]="testId + '-end-time'"
            >
              {{ formatTimeOnly(editEndTime()) }}
            </span>
            <button
              type="button"
              (click)="adjustTime('end', 15)"
              [disabled]="disabled"
              class="p-1 rounded hover:bg-gray-100 text-gray-500 transition-colors disabled:opacity-50 disabled:cursor-default"
              [attr.data-testid]="testId + '-end-plus'"
            >
              +
            </button>
          </div>
        </div>
      }
    </div>
  `,
})
export class DateTimePickerComponent implements OnInit, OnChanges {
  @Input() startTime = '';
  @Input() endTime = '';
  @Input() isAllDay = false;
  @Input() disabled = false;
  @Input() testId = 'datetime-picker';

  @Output() dateTimeChanged = new EventEmitter<DateTimeChangeEvent>();

  readonly viewMonth = signal(new Date());
  readonly editStartTime = signal('');
  readonly editEndTime = signal('');
  readonly editAllDay = signal(false);

  readonly weekDays = [
    $localize`:@@calendar.dayMon:Mon`,
    $localize`:@@calendar.dayTue:Tue`,
    $localize`:@@calendar.dayWed:Wed`,
    $localize`:@@calendar.dayThu:Thu`,
    $localize`:@@calendar.dayFri:Fri`,
    $localize`:@@calendar.daySat:Sat`,
    $localize`:@@calendar.daySun:Sun`,
  ];

  readonly monthLabel = computed(() => {
    const d = this.viewMonth();
    const locale = localStorage.getItem('familyhub-locale') ?? 'en';
    return d.toLocaleDateString(locale, { month: 'long', year: 'numeric' });
  });

  readonly calendarDays = computed(() => {
    const viewDate = this.viewMonth();
    const year = viewDate.getFullYear();
    const month = viewDate.getMonth();

    const firstDay = new Date(year, month, 1);
    // Monday-based: 0=Mon, 6=Sun
    let startDayOfWeek = firstDay.getDay() - 1;
    if (startDayOfWeek < 0) startDayOfWeek = 6;

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const rangeStart = this.editStartTime() ? new Date(this.editStartTime()) : null;
    if (rangeStart) rangeStart.setHours(0, 0, 0, 0);

    const rangeEnd = this.editEndTime() ? new Date(this.editEndTime()) : null;
    if (rangeEnd) rangeEnd.setHours(0, 0, 0, 0);

    const days: CalendarDay[] = [];

    // Previous month days
    for (let i = startDayOfWeek - 1; i >= 0; i--) {
      const d = new Date(year, month, -i);
      days.push(this.createCalendarDay(d, false, today, rangeStart, rangeEnd));
    }

    // Current month days
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    for (let i = 1; i <= daysInMonth; i++) {
      const d = new Date(year, month, i);
      days.push(this.createCalendarDay(d, true, today, rangeStart, rangeEnd));
    }

    // Next month days to fill 6 rows
    const remaining = 42 - days.length;
    for (let i = 1; i <= remaining; i++) {
      const d = new Date(year, month + 1, i);
      days.push(this.createCalendarDay(d, false, today, rangeStart, rangeEnd));
    }

    return days;
  });

  ngOnInit(): void {
    this.syncFromInputs();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['startTime'] || changes['endTime'] || changes['isAllDay']) {
      this.syncFromInputs();
    }
  }

  previousMonth(): void {
    const current = this.viewMonth();
    this.viewMonth.set(new Date(current.getFullYear(), current.getMonth() - 1, 1));
  }

  nextMonth(): void {
    const current = this.viewMonth();
    this.viewMonth.set(new Date(current.getFullYear(), current.getMonth() + 1, 1));
  }

  selectDate(date: Date): void {
    if (this.disabled) return;

    const startDate = new Date(this.editStartTime() || new Date());
    const endDate = new Date(this.editEndTime() || new Date());

    // Preserve times, update dates
    const newStart = new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate(),
      startDate.getHours(),
      startDate.getMinutes(),
    );

    // Calculate duration and apply to end
    const duration = endDate.getTime() - startDate.getTime();
    const newEnd = new Date(newStart.getTime() + duration);

    this.editStartTime.set(this.toISOLocal(newStart));
    this.editEndTime.set(this.toISOLocal(newEnd));
    this.emitChange();
  }

  adjustTime(which: 'start' | 'end', minutes: number): void {
    if (this.disabled) return;

    const current = which === 'start' ? this.editStartTime() : this.editEndTime();
    const date = new Date(current);
    date.setMinutes(date.getMinutes() + minutes);

    // Snap to 15 min
    date.setMinutes(Math.round(date.getMinutes() / 15) * 15);
    date.setSeconds(0, 0);

    if (which === 'start') {
      this.editStartTime.set(this.toISOLocal(date));
      // Auto-adjust end if end < start
      const endDate = new Date(this.editEndTime());
      if (endDate <= date) {
        const newEnd = new Date(date.getTime() + 3600000); // +1 hour
        this.editEndTime.set(this.toISOLocal(newEnd));
      }
    } else {
      // Don't allow end before start
      const startDate = new Date(this.editStartTime());
      if (date <= startDate) return;
      this.editEndTime.set(this.toISOLocal(date));
    }

    this.emitChange();
  }

  onAllDayToggle(value: boolean): void {
    if (this.disabled) return;
    this.editAllDay.set(value);
    this.emitChange();
  }

  formatTimeOnly(isoString: string): string {
    if (!isoString) return '--:--';
    const d = new Date(isoString);
    const h = d.getHours().toString().padStart(2, '0');
    const m = d.getMinutes().toString().padStart(2, '0');
    return `${h}:${m}`;
  }

  private syncFromInputs(): void {
    this.editStartTime.set(this.startTime);
    this.editEndTime.set(this.endTime);
    this.editAllDay.set(this.isAllDay);

    if (this.startTime) {
      const d = new Date(this.startTime);
      this.viewMonth.set(new Date(d.getFullYear(), d.getMonth(), 1));
    }
  }

  private emitChange(): void {
    this.dateTimeChanged.emit({
      startTime: this.editStartTime(),
      endTime: this.editEndTime(),
      isAllDay: this.editAllDay(),
    });
  }

  private createCalendarDay(
    date: Date,
    isCurrentMonth: boolean,
    today: Date,
    rangeStart: Date | null,
    rangeEnd: Date | null,
  ): CalendarDay {
    const dateOnly = new Date(date);
    dateOnly.setHours(0, 0, 0, 0);
    const t = dateOnly.getTime();

    const isStart = rangeStart ? t === rangeStart.getTime() : false;
    const isEnd = rangeEnd ? t === rangeEnd.getTime() : false;
    const isInRange =
      rangeStart && rangeEnd ? t >= rangeStart.getTime() && t <= rangeEnd.getTime() : false;

    return {
      date,
      dayNumber: date.getDate(),
      isCurrentMonth,
      isToday: dateOnly.getTime() === today.getTime(),
      isSelected: isStart || isEnd,
      isInRange,
    };
  }

  private toISOLocal(date: Date): string {
    return date.toISOString();
  }
}
