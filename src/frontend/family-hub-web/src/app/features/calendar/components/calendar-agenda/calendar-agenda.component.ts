import {
  Component,
  computed,
  ElementRef,
  EventEmitter,
  Input,
  Output,
  signal,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { CalendarEventDto } from '../../services/calendar.service';
import { AgendaDayGroup } from '../../models/calendar.models';
import { groupEventsByDay, formatAgendaEventTime } from '../../utils/agenda.utils';

@Component({
  selector: 'app-calendar-agenda',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (dayGroups().length === 0 && !loading()) {
      <!-- Empty State -->
      <div
        class="flex flex-col items-center justify-center py-16 text-gray-400"
        data-testid="agenda-empty-state"
      >
        <svg class="h-16 w-16 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="1.5"
            d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
          />
        </svg>
        <p class="text-lg font-medium" i18n="@@calendar.agenda.noEvents">No upcoming events</p>
        <p class="text-sm mt-1" i18n="@@calendar.agenda.noEventsHint">
          Events you create will appear here
        </p>
      </div>
    } @else {
      <div #scrollContainer class="overflow-y-auto" style="max-height: calc(100vh - 220px)">
        <!-- Day Groups -->
        @for (group of dayGroups(); track group.date.getTime()) {
          <div
            class="mb-1"
            [attr.data-testid]="'agenda-day-group-' + group.date.toISOString().slice(0, 10)"
          >
            <!-- Sticky Day Header -->
            <div
              class="sticky top-0 z-10 px-4 py-2 text-sm font-semibold border-b border-gray-200"
              [class.bg-blue-50]="group.isToday"
              [class.text-blue-800]="group.isToday"
              [class.bg-gray-50]="!group.isToday"
              [class.text-gray-700]="!group.isToday"
              [attr.data-testid]="'agenda-day-header-' + group.date.toISOString().slice(0, 10)"
            >
              {{ group.label }}
            </div>

            <!-- All-Day Events -->
            @for (event of group.allDayEvents; track event.id) {
              <div
                class="flex items-center gap-3 px-4 py-2.5 border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors"
                (click)="onEventClick(event)"
                [attr.data-testid]="'agenda-event-' + event.id"
              >
                <div class="w-2.5 h-2.5 rounded-full bg-blue-500 shrink-0"></div>
                <div class="flex-1 min-w-0">
                  <div class="font-medium text-sm text-gray-900 truncate">
                    {{ event.title }}
                  </div>
                  <div class="text-xs text-gray-500" i18n="@@calendar.agenda.allDay">All day</div>
                </div>
                @if (event.location) {
                  <div class="flex items-center gap-1 text-xs text-gray-400 shrink-0">
                    <svg class="h-3.5 w-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
                      />
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
                      />
                    </svg>
                    <span class="truncate max-w-[120px]">{{ event.location }}</span>
                  </div>
                }
              </div>
            }

            <!-- Timed Events -->
            @for (event of group.timedEvents; track event.id) {
              <div
                class="flex items-start gap-3 px-4 py-2.5 border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors"
                (click)="onEventClick(event)"
                [attr.data-testid]="'agenda-event-' + event.id"
              >
                <div
                  class="border-l-[3px] border-blue-500 self-stretch rounded-full shrink-0"
                ></div>
                <div class="w-[6.5rem] shrink-0 text-xs text-gray-500 pt-0.5 leading-relaxed">
                  {{ formatEventTime(event, group.date) }}
                </div>
                <div class="flex-1 min-w-0">
                  <div class="font-medium text-sm text-gray-900 truncate">
                    {{ event.title }}
                  </div>
                  @if (event.description) {
                    <div class="text-xs text-gray-500 truncate mt-0.5">
                      {{ event.description }}
                    </div>
                  }
                  @if (event.location) {
                    <div class="flex items-center gap-1 text-xs text-gray-400 mt-0.5">
                      <svg
                        class="h-3 w-3 shrink-0"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
                        />
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
                        />
                      </svg>
                      <span class="truncate">{{ event.location }}</span>
                    </div>
                  }
                </div>
                @if (event.attendees.length > 0) {
                  <div class="flex items-center gap-1 text-xs text-gray-400 shrink-0 pt-0.5">
                    <svg class="h-3.5 w-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"
                      />
                      <circle cx="9" cy="7" r="4" />
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M23 21v-2a4 4 0 0 0-3-3.87"
                      />
                      <path d="M16 3.13a4 4 0 0 1 0 7.75" />
                    </svg>
                    <span>{{ event.attendees.length }}</span>
                  </div>
                }
              </div>
            }
          </div>
        }

        <!-- Load More Button -->
        @if (hasMore()) {
          <div class="flex justify-center py-6">
            <button
              type="button"
              (click)="loadMore.emit()"
              class="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-blue-600 bg-blue-50 rounded-lg hover:bg-blue-100 transition-colors"
              [disabled]="loadingMore()"
              data-testid="agenda-load-more"
            >
              @if (loadingMore()) {
                <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                  <circle
                    class="opacity-25"
                    cx="12"
                    cy="12"
                    r="10"
                    stroke="currentColor"
                    stroke-width="4"
                  ></circle>
                  <path
                    class="opacity-75"
                    fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
                  ></path>
                </svg>
                <span i18n="@@calendar.agenda.loading">Loading...</span>
              } @else {
                <span i18n="@@calendar.agenda.loadMore">Load more events</span>
              }
            </button>
          </div>
        }
      </div>
    }
  `,
})
export class CalendarAgendaComponent implements AfterViewInit {
  @ViewChild('scrollContainer') scrollContainer?: ElementRef<HTMLDivElement>;

  events = signal<CalendarEventDto[]>([]);
  batchCount = signal<number>(1);
  loading = signal<boolean>(false);
  loadingMore = signal<boolean>(false);
  hasMore = signal<boolean>(true);

  @Input() set eventsInput(value: CalendarEventDto[]) {
    this.events.set(value);
  }

  @Input() set batchCountInput(value: number) {
    this.batchCount.set(value);
  }

  @Input() set loadingInput(value: boolean) {
    this.loading.set(value);
  }

  @Input() set loadingMoreInput(value: boolean) {
    this.loadingMore.set(value);
  }

  @Input() set hasMoreInput(value: boolean) {
    this.hasMore.set(value);
  }

  @Output() eventClicked = new EventEmitter<CalendarEventDto>();
  @Output() loadMore = new EventEmitter<void>();

  dayGroups = computed<AgendaDayGroup[]>(() => groupEventsByDay(this.events(), this.batchCount()));

  ngAfterViewInit(): void {
    this.scrollToToday();
  }

  formatEventTime(event: CalendarEventDto, day: Date): string {
    return formatAgendaEventTime(event, day);
  }

  onEventClick(event: CalendarEventDto): void {
    this.eventClicked.emit(event);
  }

  private scrollToToday(): void {
    if (!this.scrollContainer) return;
    const todayEl = this.scrollContainer.nativeElement.querySelector('[class*="bg-blue-50"]');
    if (todayEl) {
      todayEl.scrollIntoView({ block: 'start' });
    }
  }
}
