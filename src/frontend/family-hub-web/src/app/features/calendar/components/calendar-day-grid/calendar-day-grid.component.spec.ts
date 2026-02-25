import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CalendarDayGridComponent } from './calendar-day-grid.component';
import { CalendarEventDto } from '../../services/calendar.service';

// Use a fixed date clearly in the past to avoid "is today" side effects
// Jan 27, 2026 – midday UTC events are safely within this day in any timezone
const FIXED_DATE = new Date(2026, 0, 27); // Jan 27, 2026 (local)

function createEvent(overrides: Partial<CalendarEventDto> = {}): CalendarEventDto {
  return {
    id: 'event-1',
    familyId: 'family-1',
    createdBy: 'user-1',
    title: 'Test Event',
    description: null,
    location: null,
    startTime: '2026-01-27T09:00:00Z',
    endTime: '2026-01-27T10:00:00Z',
    isAllDay: false,
    isCancelled: false,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
    attendees: [],
    ...overrides,
  };
}

describe('CalendarDayGridComponent', () => {
  let component: CalendarDayGridComponent;
  let fixture: ComponentFixture<CalendarDayGridComponent>;
  let nativeElement: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CalendarDayGridComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CalendarDayGridComponent);
    component = fixture.componentInstance;
    nativeElement = fixture.nativeElement;
  });

  afterEach(() => {
    fixture.destroy();
  });

  it('should create without error when given an empty events array', () => {
    component.selectedDateInput = FIXED_DATE;
    component.eventsInput = [];
    component.loadingInput = false;

    expect(() => fixture.detectChanges()).not.toThrow();
    expect(component).toBeTruthy();
  });

  it('should show <app-calendar-day-skeleton> when loadingInput is true', () => {
    component.selectedDateInput = FIXED_DATE;
    component.eventsInput = [];
    component.loadingInput = true;
    fixture.detectChanges();

    const skeleton = nativeElement.querySelector('app-calendar-day-skeleton');
    expect(skeleton).toBeTruthy();
  });

  it('should hide skeleton and show time grid when loadingInput is false', () => {
    component.selectedDateInput = FIXED_DATE;
    component.eventsInput = [];
    component.loadingInput = false;
    fixture.detectChanges();

    const skeleton = nativeElement.querySelector('app-calendar-day-skeleton');
    const scrollContainer = nativeElement.querySelector('.overflow-y-auto');
    expect(skeleton).toBeNull();
    expect(scrollContainer).toBeTruthy();
  });

  it('should show all-day events section when all-day events exist for the date', () => {
    component.selectedDateInput = FIXED_DATE;
    component.eventsInput = [
      createEvent({
        id: 'allday-1',
        title: 'All Day Event',
        isAllDay: true,
        startTime: '2026-01-27T00:00:00Z',
        endTime: '2026-01-27T23:59:59Z',
      }),
    ];
    component.loadingInput = false;
    fixture.detectChanges();

    expect(nativeElement.textContent).toContain('ALL DAY');
    expect(nativeElement.textContent).toContain('All Day Event');
  });

  it('should not show all-day events section when no all-day events exist', () => {
    component.selectedDateInput = FIXED_DATE;
    component.eventsInput = [
      createEvent({ isAllDay: false }),
    ];
    component.loadingInput = false;
    fixture.detectChanges();

    expect(nativeElement.textContent).not.toContain('ALL DAY');
  });

  it('should emit timeSlotClicked with a Date when an empty time slot is clicked', () => {
    component.selectedDateInput = FIXED_DATE;
    component.eventsInput = [];
    component.loadingInput = false;
    fixture.detectChanges();

    const emitted: Date[] = [];
    component.timeSlotClicked.subscribe((d) => emitted.push(d));

    // Find the day column (the clickable area with the Angular (click) binding)
    const dayColumn = nativeElement.querySelector('.relative.border-l.border-gray-200') as HTMLElement;
    expect(dayColumn).toBeTruthy();
    dayColumn.dispatchEvent(new MouseEvent('click', { clientY: 120, bubbles: true }));

    expect(emitted).toHaveLength(1);
    expect(emitted[0]).toBeInstanceOf(Date);
  });

  it('should emit eventClicked with the event when an event block is clicked', () => {
    const event = createEvent({ id: 'click-me', title: 'Click Me' });
    component.selectedDateInput = FIXED_DATE;
    component.eventsInput = [event];
    component.loadingInput = false;
    fixture.detectChanges();

    const emitted: CalendarEventDto[] = [];
    component.eventClicked.subscribe((e) => emitted.push(e));

    const mouseEvent = new MouseEvent('click', { bubbles: false });
    component.onEventClick(mouseEvent, event);

    expect(emitted).toHaveLength(1);
    expect(emitted[0].id).toBe('click-me');
  });
});
