import {
  Component,
  computed,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
  Renderer2,
  signal,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { CalendarEventDto } from '../../services/calendar.service';
import {
  PositionedEvent,
  SpannedAllDayEvent,
  WEEK_GRID_CONSTANTS,
  WeekDay,
  TimeRange,
} from '../../models/calendar.models';
import {
  getWeekDays,
  getStoredLocale,
  partitionEvents,
  layoutTimedEvents,
  getNowIndicatorOffset,
  pixelOffsetToTime,
  formatTimeShort,
} from '../../utils/week.utils';
import { getStoredTimeFormat } from '../../../../core/i18n/format-preferences.utils';

@Component({
  selector: 'app-calendar-week-grid',
  standalone: true,
  imports: [CommonModule],
  template: `
    <!-- Day Headers -->
    <div class="grid grid-cols-[4rem_repeat(7,1fr)] border-b border-gray-200 bg-white">
      <div class="w-16"></div>
      @for (day of weekDays(); track day.date.toISOString(); let i = $index) {
        <div class="py-2 text-center border-l border-gray-200" [class.bg-blue-50]="day.isToday">
          <div class="text-xs font-medium text-gray-500 uppercase">{{ day.dayLabel }}</div>
          <div
            class="text-lg font-semibold mt-0.5 w-8 h-8 flex items-center justify-center mx-auto rounded-full cursor-pointer hover:ring-2 hover:ring-blue-400"
            [class.bg-blue-600]="day.isToday"
            [class.text-white]="day.isToday"
            [class.text-gray-900]="!day.isToday"
            (click)="onDayHeaderClick(day.date)"
          >
            {{ day.dayNumber }}
          </div>
        </div>
      }
    </div>

    <!-- All-Day Events Row -->
    @if (hasAllDayEvents()) {
      <div class="grid grid-cols-[4rem_repeat(7,1fr)] border-b border-gray-200 bg-white">
        <div class="w-16 py-1 px-1 text-[10px] text-gray-400 font-medium" i18n="@@calendar.allDay">
          ALL DAY
        </div>
        <!-- Spanning container: 7 columns with relative positioning for stacked rows -->
        <div class="col-span-7 relative" [style.min-height.px]="allDayRowHeight()">
          <!-- Column borders (visual grid lines) -->
          @for (i of [0, 1, 2, 3, 4, 5, 6]; track i) {
            @if (i > 0) {
              <div
                class="absolute top-0 bottom-0 border-l border-gray-200"
                [style.left]="(i / 7) * 100 + '%'"
              ></div>
            }
          }
          <!-- Spanning event bars -->
          @for (se of spannedAllDayEvents(); track se.event.id) {
            <div
              class="absolute text-xs px-1.5 py-0.5 truncate cursor-pointer bg-blue-100 text-blue-800 border border-blue-200 rounded-sm hover:bg-blue-200 transition-colors"
              [style.left]="'calc(' + (se.startCol / 7) * 100 + '% + 2px)'"
              [style.width]="'calc(' + (se.span / 7) * 100 + '% - 4px)'"
              [style.top.px]="se.row * 22 + 4"
              style="height: 20px; line-height: 18px;"
              [title]="se.event.title"
              (click)="onEventClick($event, se.event)"
            >
              {{ se.event.title }}
            </div>
          }
        </div>
      </div>
    }

    <!-- Scrollable Time Grid -->
    <div
      #scrollContainer
      class="overflow-y-auto overflow-x-auto"
      style="max-height: calc(100vh - 280px); min-width: 700px"
    >
      <div class="grid grid-cols-[4rem_repeat(7,1fr)] relative" [style.height.px]="totalHeight">
        <!-- Time Gutter -->
        <div class="relative w-16">
          @for (hour of hours; track hour) {
            <div
              class="absolute w-full px-1 text-[11px] text-gray-400 text-right pr-2 -translate-y-1/2"
              [style.top.px]="hour * hourHeight"
            >
              {{ formatHourLabel(hour) }}
            </div>
          }
        </div>

        <!-- Day Columns -->
        @for (day of weekDays(); track day.date.toISOString(); let dayIdx = $index) {
          <div
            class="relative border-l border-gray-200 select-none"
            [class.bg-blue-50/30]="day.isToday && !isDayInCrossDayRange(dayIdx)"
            [class.bg-blue-100/50]="isDayInCrossDayRange(dayIdx)"
            [attr.data-day-index]="dayIdx"
            (mousedown)="onMouseDown($event, dayIdx)"
          >
            <!-- Hour grid lines -->
            @for (hour of hours; track hour) {
              <div
                class="absolute w-full border-t border-gray-100"
                [style.top.px]="hour * hourHeight"
              ></div>
            }

            <!-- Positioned timed events -->
            @for (pe of getPositionedEvents(dayIdx); track pe.event.id) {
              <div
                class="absolute rounded-md px-1.5 py-0.5 text-xs cursor-pointer overflow-hidden border-l-[3px] bg-white shadow-sm hover:shadow-md transition-shadow z-10"
                class="border-l-blue-500"
                [style.top.px]="pe.top"
                [style.height.px]="pe.height"
                [style.left]="getEventLeft(pe)"
                [style.width]="getEventWidth(pe)"
                [title]="pe.event.title"
                (click)="onEventClick($event, pe.event)"
              >
                <div class="font-medium truncate">{{ pe.event.title }}</div>
                @if (pe.height >= 30) {
                  <div class="text-[10px] text-gray-500 truncate">
                    {{ formatEventTime(pe.event) }}
                  </div>
                }
              </div>
            }

            <!-- Now Indicator -->
            @if (day.isToday && nowOffset() >= 0) {
              <div class="absolute w-full z-20 pointer-events-none" [style.top.px]="nowOffset()">
                <div class="relative">
                  <div class="absolute -left-1.5 -top-1.5 w-3 h-3 bg-red-500 rounded-full"></div>
                  <div class="w-full h-0.5 bg-red-500"></div>
                </div>
              </div>
            }

            <!-- Same-day Drag Overlay (time range) -->
            @if (isDragging() && !isCrossDayDrag() && dragDayIndex() === dayIdx) {
              <div
                class="absolute w-full bg-blue-500/30 border border-blue-500 rounded-sm pointer-events-none z-30"
                [style.top.px]="dragOverlayTop()"
                [style.height.px]="dragOverlayHeight()"
              >
                <div
                  class="absolute -top-5 left-1 text-xs font-medium text-blue-700 bg-white px-1 rounded shadow-sm"
                >
                  {{ dragStartTimeLabel() }}
                </div>
                <div
                  class="absolute -bottom-5 left-1 text-xs font-medium text-blue-700 bg-white px-1 rounded shadow-sm"
                >
                  {{ dragEndTimeLabel() }}
                </div>
              </div>
            }

            <!-- Cross-day Drag Overlay (full column highlight) -->
            @if (isDragging() && isCrossDayDrag() && isDayInCrossDayRange(dayIdx)) {
              <div
                class="absolute inset-0 bg-blue-500/20 border border-blue-400 pointer-events-none z-30"
              ></div>
            }
          </div>
        }
      </div>
    </div>
  `,
})
export class CalendarWeekGridComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('scrollContainer') scrollContainer!: ElementRef<HTMLDivElement>;

  weekStart = signal<Date>(new Date());
  events = signal<CalendarEventDto[]>([]);

  @Input() set weekStartInput(value: Date) {
    this.weekStart.set(value);
  }

  @Input() set eventsInput(value: CalendarEventDto[]) {
    this.events.set(value);
  }

  @Output() timeSlotClicked = new EventEmitter<Date>();
  @Output() eventClicked = new EventEmitter<CalendarEventDto>();
  @Output() dayHeaderClicked = new EventEmitter<Date>();
  @Output() timeRangeSelected = new EventEmitter<TimeRange>();
  @Output() dateRangeSelected = new EventEmitter<TimeRange>();

  readonly hourHeight = WEEK_GRID_CONSTANTS.HOUR_HEIGHT;
  readonly totalHeight = WEEK_GRID_CONSTANTS.TOTAL_HEIGHT;
  readonly hours = Array.from({ length: 24 }, (_, i) => i);

  nowOffset = signal<number>(getNowIndicatorOffset());
  private nowInterval: ReturnType<typeof setInterval> | null = null;

  // Drag state tracking
  isDragging = signal<boolean>(false);
  dragStartY = signal<number>(0);
  dragCurrentY = signal<number>(0);
  dragDayIndex = signal<number | null>(null);
  dragCurrentDayIndex = signal<number | null>(null);

  // Cross-day drag detection
  isCrossDayDrag = computed<boolean>(() => {
    const start = this.dragDayIndex();
    const current = this.dragCurrentDayIndex();
    return start !== null && current !== null && start !== current;
  });

  crossDayDragRange = computed<{ min: number; max: number }>(() => {
    const start = this.dragDayIndex();
    const current = this.dragCurrentDayIndex();
    if (start === null || current === null) return { min: -1, max: -1 };
    return { min: Math.min(start, current), max: Math.max(start, current) };
  });

  // Global listener cleanup functions
  private mouseMoveUnlisten?: () => void;
  private mouseUpUnlisten?: () => void;

  weekDays = computed<WeekDay[]>(() => getWeekDays(this.weekStart()));

  // Drag overlay computed values
  dragOverlayTop = computed<number>(() => {
    const startY = this.dragStartY();
    const currentY = this.dragCurrentY();
    return Math.min(startY, currentY);
  });

  dragOverlayHeight = computed<number>(() => {
    const startY = this.dragStartY();
    const currentY = this.dragCurrentY();
    return Math.abs(currentY - startY);
  });

  dragStartTimeLabel = computed<string>(() => {
    const dayIdx = this.dragDayIndex();
    if (dayIdx === null) {
      return '';
    }
    const day = this.weekDays()[dayIdx];
    if (!day) {
      return '';
    }
    const startY = this.dragStartY();
    const time = pixelOffsetToTime(startY, day.date);
    return formatTimeShort(time);
  });

  dragEndTimeLabel = computed<string>(() => {
    const dayIdx = this.dragDayIndex();
    if (dayIdx === null) {
      return '';
    }
    const day = this.weekDays()[dayIdx];
    if (!day) {
      return '';
    }
    const currentY = this.dragCurrentY();
    const time = pixelOffsetToTime(currentY, day.date);
    return formatTimeShort(time);
  });

  constructor(private renderer: Renderer2) {}

  hasAllDayEvents = computed(() => {
    const days = this.weekDays();
    const allEvents = this.events();
    return days.some((day) => {
      const { allDay } = partitionEvents(allEvents, day.date);
      return allDay.length > 0;
    });
  });

  spannedAllDayEvents = computed<SpannedAllDayEvent[]>(() => {
    const days = this.weekDays();
    const allEvents = this.events();
    if (days.length === 0) return [];

    const weekStartTime = new Date(days[0].date);
    weekStartTime.setHours(0, 0, 0, 0);
    const weekEndTime = new Date(days[6].date);
    weekEndTime.setHours(23, 59, 59, 999);

    // Collect unique all-day events that overlap with this week
    const seen = new Set<string>();
    const allDayEvents: CalendarEventDto[] = [];
    for (const day of days) {
      const { allDay } = partitionEvents(allEvents, day.date);
      for (const event of allDay) {
        if (!seen.has(event.id)) {
          seen.add(event.id);
          allDayEvents.push(event);
        }
      }
    }

    // Sort by start time then by duration (longer first for better row packing)
    allDayEvents.sort((a, b) => {
      const aStart = new Date(a.startTime).getTime();
      const bStart = new Date(b.startTime).getTime();
      if (aStart !== bStart) return aStart - bStart;
      const aDur = new Date(a.endTime).getTime() - aStart;
      const bDur = new Date(b.endTime).getTime() - new Date(b.startTime).getTime();
      return bDur - aDur;
    });

    // Compute column span for each event, clipped to week boundaries
    const result: SpannedAllDayEvent[] = [];
    // Track which columns are occupied in each row
    const rowOccupancy: boolean[][] = [];

    for (const event of allDayEvents) {
      const eventStart = new Date(event.startTime);
      const eventEnd = new Date(event.endTime);

      // Clip to week bounds
      const clippedStart = eventStart < weekStartTime ? weekStartTime : eventStart;
      const clippedEnd = eventEnd > weekEndTime ? weekEndTime : eventEnd;

      // Find which day columns this event occupies
      let startCol = -1;
      let endCol = -1;
      for (let i = 0; i < 7; i++) {
        const dayStart = new Date(days[i].date);
        dayStart.setHours(0, 0, 0, 0);
        const dayEnd = new Date(days[i].date);
        dayEnd.setHours(23, 59, 59, 999);

        if (clippedStart <= dayEnd && clippedEnd >= dayStart) {
          if (startCol === -1) startCol = i;
          endCol = i;
        }
      }

      if (startCol === -1) continue;

      const span = endCol - startCol + 1;

      // Find the first row where this event fits (no column overlap)
      let row = 0;
      while (true) {
        if (row >= rowOccupancy.length) {
          rowOccupancy.push(new Array(7).fill(false));
        }
        let fits = true;
        for (let c = startCol; c <= endCol; c++) {
          if (rowOccupancy[row][c]) {
            fits = false;
            break;
          }
        }
        if (fits) break;
        row++;
      }

      // Mark columns as occupied in this row
      for (let c = startCol; c <= endCol; c++) {
        rowOccupancy[row][c] = true;
      }

      result.push({ event, startCol, span, row });
    }

    return result;
  });

  allDayRowHeight = computed<number>(() => {
    const events = this.spannedAllDayEvents();
    if (events.length === 0) return 28;
    const maxRow = Math.max(...events.map((e) => e.row));
    return (maxRow + 1) * 22 + 8; // 22px per row + 8px padding
  });

  // Cache layout per-computation cycle
  private layoutCache = computed(() => {
    const days = this.weekDays();
    const allEvents = this.events();
    return days.map((day) => {
      const { timed } = partitionEvents(allEvents, day.date);
      return layoutTimedEvents(timed, day.date);
    });
  });

  ngOnInit(): void {
    this.nowInterval = setInterval(() => {
      this.nowOffset.set(getNowIndicatorOffset());
    }, WEEK_GRID_CONSTANTS.NOW_INDICATOR_INTERVAL);
  }

  ngAfterViewInit(): void {
    if (this.scrollContainer) {
      this.scrollContainer.nativeElement.scrollTop =
        WEEK_GRID_CONSTANTS.HOUR_HEIGHT * WEEK_GRID_CONSTANTS.DEFAULT_SCROLL_HOUR;
    }
  }

  ngOnDestroy(): void {
    if (this.nowInterval) {
      clearInterval(this.nowInterval);
    }

    // Clean up global mouse listeners
    if (this.mouseMoveUnlisten) {
      this.mouseMoveUnlisten();
    }

    if (this.mouseUpUnlisten) {
      this.mouseUpUnlisten();
    }
  }

  getPositionedEvents(dayIndex: number): PositionedEvent[] {
    return this.layoutCache()[dayIndex] ?? [];
  }

  isDayInCrossDayRange(dayIdx: number): boolean {
    if (!this.isDragging() || !this.isCrossDayDrag()) return false;
    const range = this.crossDayDragRange();
    return dayIdx >= range.min && dayIdx <= range.max;
  }

  getEventLeft(pe: PositionedEvent): string {
    return `calc(${pe.column} / ${pe.totalColumns} * 100%)`;
  }

  getEventWidth(pe: PositionedEvent): string {
    return `calc(100% / ${pe.totalColumns} - 2px)`;
  }

  formatHourLabel(hour: number): string {
    const date = new Date(2000, 0, 1, hour, 0);
    return date.toLocaleTimeString(getStoredLocale(), {
      hour: 'numeric',
      minute: undefined,
      hour12: getStoredTimeFormat() === '12h',
    });
  }

  formatEventTime(event: CalendarEventDto): string {
    return `${formatTimeShort(new Date(event.startTime))} – ${formatTimeShort(new Date(event.endTime))}`;
  }

  onEventClick(mouseEvent: MouseEvent, event: CalendarEventDto): void {
    mouseEvent.stopPropagation();
    this.eventClicked.emit(event);
  }

  onDayHeaderClick(date: Date): void {
    this.dayHeaderClicked.emit(date);
  }

  onMouseDown(mouseEvent: MouseEvent, dayIndex: number): void {
    // Prevent drag on existing events
    const target = mouseEvent.target as HTMLElement;
    if (target.closest('.z-10')) {
      return;
    }

    const rect = (mouseEvent.currentTarget as HTMLElement).getBoundingClientRect();
    const yOffset = mouseEvent.clientY - rect.top;

    this.isDragging.set(true);
    this.dragStartY.set(yOffset);
    this.dragCurrentY.set(yOffset);
    this.dragDayIndex.set(dayIndex);
    this.dragCurrentDayIndex.set(dayIndex);

    // Attach global listeners using Renderer2
    this.mouseMoveUnlisten = this.renderer.listen('document', 'mousemove', (e: MouseEvent) => {
      this.onMouseMove(e);
    });

    this.mouseUpUnlisten = this.renderer.listen('document', 'mouseup', (e: MouseEvent) => {
      this.onMouseUp(e);
    });

    mouseEvent.preventDefault();
  }

  onMouseMove(mouseEvent: MouseEvent): void {
    if (!this.isDragging()) {
      return;
    }

    const dayIdx = this.dragDayIndex();
    if (dayIdx === null) {
      return;
    }

    // Detect which day column the cursor is over
    const el = document.elementFromPoint(mouseEvent.clientX, mouseEvent.clientY);
    const dayColumn = el?.closest('[data-day-index]') as HTMLElement | null;
    if (dayColumn) {
      const hoveredDayIdx = parseInt(dayColumn.getAttribute('data-day-index')!, 10);
      this.dragCurrentDayIndex.set(hoveredDayIdx);
    }

    // Update Y position relative to the start day's column
    const container = this.scrollContainer.nativeElement;
    const startColumn = container.querySelector(`[data-day-index="${dayIdx}"]`) as HTMLElement;

    if (!startColumn) {
      return;
    }

    const rect = startColumn.getBoundingClientRect();
    const yOffset = mouseEvent.clientY - rect.top;

    this.dragCurrentY.set(yOffset);
  }

  onMouseUp(mouseEvent: MouseEvent): void {
    if (!this.isDragging()) {
      return;
    }

    const dayIdx = this.dragDayIndex();
    const currentDayIdx = this.dragCurrentDayIndex();
    const startY = this.dragStartY();
    const currentY = this.dragCurrentY();
    const crossDay = this.isCrossDayDrag();

    this.isDragging.set(false);

    // Clean up global listeners
    if (this.mouseMoveUnlisten) {
      this.mouseMoveUnlisten();
      this.mouseMoveUnlisten = undefined;
    }

    if (this.mouseUpUnlisten) {
      this.mouseUpUnlisten();
      this.mouseUpUnlisten = undefined;
    }

    // Cross-day drag: emit as all-day date range
    if (crossDay && dayIdx !== null && currentDayIdx !== null) {
      const days = this.weekDays();
      const minIdx = Math.min(dayIdx, currentDayIdx);
      const maxIdx = Math.max(dayIdx, currentDayIdx);

      const startDate = new Date(days[minIdx].date);
      startDate.setHours(0, 0, 0, 0);

      const endDate = new Date(days[maxIdx].date);
      endDate.setHours(23, 59, 59, 999);

      this.dateRangeSelected.emit({ start: startDate, end: endDate });

      this.dragStartY.set(0);
      this.dragCurrentY.set(0);
      this.dragDayIndex.set(null);
      this.dragCurrentDayIndex.set(null);
      return;
    }

    // Same-day: calculate drag distance to differentiate click from drag
    const dragDistance = Math.abs(currentY - startY);
    const CLICK_THRESHOLD_PX = 15;

    // If drag distance is less than threshold, treat as click
    if (dragDistance < CLICK_THRESHOLD_PX) {
      if (dayIdx !== null) {
        const day = this.weekDays()[dayIdx];
        const clickedTime = pixelOffsetToTime(startY, day.date);
        this.timeSlotClicked.emit(clickedTime);
      }
      this.dragStartY.set(0);
      this.dragCurrentY.set(0);
      this.dragDayIndex.set(null);
      this.dragCurrentDayIndex.set(null);
      return;
    }

    // Same-day drag: calculate time range from drag positions
    if (dayIdx !== null) {
      const day = this.weekDays()[dayIdx];

      // Convert pixel offsets to times (already snapped to 15-min intervals by pixelOffsetToTime)
      const time1 = pixelOffsetToTime(startY, day.date);
      const time2 = pixelOffsetToTime(currentY, day.date);

      // Support bidirectional drag: ensure startTime < endTime
      const startTime = time1 < time2 ? time1 : time2;
      const endTime = time1 < time2 ? time2 : time1;

      // Ensure minimum 15-minute duration
      const MIN_DURATION_MS = 15 * 60 * 1000;
      if (endTime.getTime() - startTime.getTime() < MIN_DURATION_MS) {
        endTime.setMinutes(endTime.getMinutes() + 15);
      }

      // Emit the time range selection
      this.timeRangeSelected.emit({
        start: startTime,
        end: endTime,
      });
    }

    // Reset drag state
    this.dragStartY.set(0);
    this.dragCurrentY.set(0);
    this.dragDayIndex.set(null);
    this.dragCurrentDayIndex.set(null);
  }
}
