import {
  Component,
  computed,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
  signal,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { CalendarEventDto } from '../../services/calendar.service';
import { PositionedEvent, WEEK_GRID_CONSTANTS } from '../../models/calendar.models';
import {
  partitionEvents,
  layoutTimedEvents,
  getNowIndicatorOffset,
  pixelOffsetToTime,
  formatTimeShort,
  getStoredLocale,
} from '../../utils/week.utils';
import { isToday } from '../../utils/day.utils';
import { getStoredTimeFormat } from '../../../../core/i18n/format-preferences.utils';
import { CalendarDaySkeletonComponent } from '../calendar-day-skeleton/calendar-day-skeleton.component';

@Component({
  selector: 'app-calendar-day-grid',
  standalone: true,
  imports: [CommonModule, CalendarDaySkeletonComponent],
  template: `
    @if (loading()) {
      <app-calendar-day-skeleton />
    } @else {
      <!-- All-Day Events Row -->
      @if (hasAllDayEvents()) {
        <div class="grid grid-cols-[4rem_1fr] border-b border-gray-200 bg-white">
          <div
            class="w-16 py-1 px-1 text-[10px] text-gray-400 font-medium"
            i18n="@@calendar.allDay"
          >
            ALL DAY
          </div>
          <div class="py-1 px-1 border-l border-gray-200 space-y-0.5">
            @for (event of allDayEvents(); track event.id) {
              <div
                class="text-xs px-1.5 py-0.5 rounded border truncate cursor-pointer bg-blue-100 text-blue-800 border-blue-200"
                [title]="event.title"
                (click)="onEventClick($event, event)"
              >
                {{ event.title }}
              </div>
            }
          </div>
        </div>
      }

      <!-- Scrollable Time Grid -->
      <div
        #scrollContainer
        class="overflow-y-auto"
        style="max-height: calc(100vh - 280px)"
      >
        <div class="grid grid-cols-[4rem_1fr] relative" [style.height.px]="totalHeight">
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

          <!-- Day Column -->
          <div
            class="relative border-l border-gray-200"
            [class.bg-blue-50/30]="isTodaySelected()"
            (click)="onTimeSlotClick($event)"
          >
            <!-- Hour grid lines -->
            @for (hour of hours; track hour) {
              <div
                class="absolute w-full border-t border-gray-100"
                [style.top.px]="hour * hourHeight"
              ></div>
            }

            <!-- Positioned timed events -->
            @for (pe of positionedEvents(); track pe.event.id) {
              <div
                class="absolute rounded-md px-1.5 py-0.5 text-xs cursor-pointer overflow-hidden border-l-[3px] bg-white shadow-sm hover:shadow-md transition-shadow z-10 border-l-blue-500"
                [style.top.px]="pe.top"
                [style.height.px]="pe.height"
                [style.left]="getEventLeft(pe)"
                [style.width]="getEventWidth(pe)"
                [title]="pe.event.title"
                (click)="onEventClick($event, pe.event)"
              >
                <div class="font-semibold truncate">{{ pe.event.title }}</div>
                <div class="text-[10px] text-gray-500 truncate">
                  {{ formatEventTime(pe.event) }}
                </div>
                @if (pe.height >= 50 && pe.event.location) {
                  <div class="flex items-center gap-1 text-[10px] text-gray-500 truncate mt-0.5">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      class="h-3 w-3 shrink-0"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      stroke-width="2"
                      stroke-linecap="round"
                      stroke-linejoin="round"
                    >
                      <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z" />
                      <circle cx="12" cy="10" r="3" />
                    </svg>
                    <span class="truncate">{{ pe.event.location }}</span>
                  </div>
                }
                @if (pe.height >= 65 && pe.event.attendees.length > 0) {
                  <div class="flex items-center gap-1 text-[10px] text-gray-500 mt-0.5">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      class="h-3 w-3 shrink-0"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      stroke-width="2"
                      stroke-linecap="round"
                      stroke-linejoin="round"
                    >
                      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
                      <circle cx="9" cy="7" r="4" />
                      <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
                      <path d="M16 3.13a4 4 0 0 1 0 7.75" />
                    </svg>
                    <span>{{ pe.event.attendees.length }}</span>
                  </div>
                }
              </div>
            }

            <!-- Now Indicator -->
            @if (isTodaySelected() && nowOffset() >= 0) {
              <div
                class="absolute w-full z-20 pointer-events-none"
                [style.top.px]="nowOffset()"
              >
                <div class="relative">
                  <div class="absolute -left-1.5 -top-1.5 w-3 h-3 bg-red-500 rounded-full"></div>
                  <div class="w-full h-0.5 bg-red-500"></div>
                </div>
              </div>
            }
          </div>
        </div>
      </div>
    }
  `,
})
export class CalendarDayGridComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('scrollContainer') scrollContainer!: ElementRef<HTMLDivElement>;

  selectedDate = signal<Date>(new Date());
  events = signal<CalendarEventDto[]>([]);
  loading = signal<boolean>(false);

  @Input() set selectedDateInput(value: Date) {
    this.selectedDate.set(value);
  }

  @Input() set eventsInput(value: CalendarEventDto[]) {
    this.events.set(value);
  }

  @Input() set loadingInput(value: boolean) {
    this.loading.set(value);
  }

  @Output() timeSlotClicked = new EventEmitter<Date>();
  @Output() eventClicked = new EventEmitter<CalendarEventDto>();

  readonly hourHeight = WEEK_GRID_CONSTANTS.HOUR_HEIGHT;
  readonly totalHeight = WEEK_GRID_CONSTANTS.TOTAL_HEIGHT;
  readonly hours = Array.from({ length: 24 }, (_, i) => i);

  nowOffset = signal<number>(getNowIndicatorOffset());
  private nowInterval: ReturnType<typeof setInterval> | null = null;

  isTodaySelected = computed(() => isToday(this.selectedDate()));

  private partitioned = computed(() => partitionEvents(this.events(), this.selectedDate()));

  hasAllDayEvents = computed(() => this.partitioned().allDay.length > 0);

  allDayEvents = computed(() => this.partitioned().allDay);

  positionedEvents = computed<PositionedEvent[]>(() =>
    layoutTimedEvents(this.partitioned().timed, this.selectedDate()),
  );

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

  onTimeSlotClick(mouseEvent: MouseEvent): void {
    const target = mouseEvent.target as HTMLElement;
    if (target !== mouseEvent.currentTarget && target.closest('.z-10')) {
      return;
    }

    const container = this.scrollContainer.nativeElement;
    const rect = (mouseEvent.currentTarget as HTMLElement).getBoundingClientRect();
    const yOffset = mouseEvent.clientY - rect.top + container.scrollTop;

    const clickedTime = pixelOffsetToTime(yOffset, this.selectedDate());
    this.timeSlotClicked.emit(clickedTime);
  }

  onEventClick(mouseEvent: MouseEvent, event: CalendarEventDto): void {
    mouseEvent.stopPropagation();
    this.eventClicked.emit(event);
  }
}
