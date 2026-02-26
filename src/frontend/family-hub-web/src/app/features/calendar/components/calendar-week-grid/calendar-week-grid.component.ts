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
        @for (day of weekDays(); track day.date.toISOString()) {
          <div class="py-1 px-1 border-l border-gray-200 space-y-0.5">
            @for (event of getAllDayEvents(day.date); track event.id) {
              <div
                class="text-xs px-1.5 py-0.5 rounded border truncate cursor-pointer"
                class="bg-blue-100 text-blue-800 border-blue-200"
                [title]="event.title"
                (click)="onEventClick($event, event)"
              >
                {{ event.title }}
              </div>
            }
          </div>
        }
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
            class="relative border-l border-gray-200"
            [class.bg-blue-50/30]="day.isToday"
            (click)="onTimeSlotClick($event, dayIdx)"
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

  // Global listener cleanup functions
  private mouseMoveUnlisten?: () => void;
  private mouseUpUnlisten?: () => void;

  weekDays = computed<WeekDay[]>(() => getWeekDays(this.weekStart()));

  constructor(private renderer: Renderer2) {}

  hasAllDayEvents = computed(() => {
    const days = this.weekDays();
    const allEvents = this.events();
    return days.some((day) => {
      const { allDay } = partitionEvents(allEvents, day.date);
      return allDay.length > 0;
    });
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

  getAllDayEvents(dayDate: Date): CalendarEventDto[] {
    return partitionEvents(this.events(), dayDate).allDay;
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

  onTimeSlotClick(mouseEvent: MouseEvent, dayIndex: number): void {
    // Only handle clicks on the day column background, not on events
    if (
      (mouseEvent.target as HTMLElement).closest('[class*="cursor-pointer"]') !==
      mouseEvent.currentTarget
    ) {
      // Check if the click target is an event block (not the column div itself)
      const target = mouseEvent.target as HTMLElement;
      if (target !== mouseEvent.currentTarget && target.closest('.z-10')) {
        return;
      }
    }

    const container = this.scrollContainer.nativeElement;
    const rect = (mouseEvent.currentTarget as HTMLElement).getBoundingClientRect();
    const yOffset = mouseEvent.clientY - rect.top + container.scrollTop;

    const day = this.weekDays()[dayIndex];
    const clickedTime = pixelOffsetToTime(yOffset, day.date);
    this.timeSlotClicked.emit(clickedTime);
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

    const container = this.scrollContainer.nativeElement;
    const rect = (mouseEvent.currentTarget as HTMLElement).getBoundingClientRect();
    const yOffset = mouseEvent.clientY - rect.top + container.scrollTop;

    this.isDragging.set(true);
    this.dragStartY.set(yOffset);
    this.dragCurrentY.set(yOffset);
    this.dragDayIndex.set(dayIndex);

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

    const container = this.scrollContainer.nativeElement;
    const dayColumn = container.querySelector(
      `.grid > div:nth-child(${dayIdx + 2})`
    ) as HTMLElement;

    if (!dayColumn) {
      return;
    }

    const rect = dayColumn.getBoundingClientRect();
    const yOffset = mouseEvent.clientY - rect.top + container.scrollTop;

    this.dragCurrentY.set(yOffset);
  }

  onMouseUp(mouseEvent: MouseEvent): void {
    if (!this.isDragging()) {
      return;
    }

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

    // Reset drag state
    this.dragStartY.set(0);
    this.dragCurrentY.set(0);
    this.dragDayIndex.set(null);
  }
}
