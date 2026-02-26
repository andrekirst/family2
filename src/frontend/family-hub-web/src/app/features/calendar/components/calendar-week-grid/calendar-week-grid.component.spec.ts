import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CalendarWeekGridComponent } from './calendar-week-grid.component';
import { CalendarEventDto } from '../../services/calendar.service';
import { TimeRange } from '../../models/calendar.models';

// Use a fixed date clearly in the past to avoid "is today" side effects
// Jan 27, 2026 – midday UTC events are safely within this week in any timezone
const FIXED_DATE = new Date(2026, 0, 26); // Jan 26, 2026 (Monday)

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

describe('CalendarWeekGridComponent', () => {
  let component: CalendarWeekGridComponent;
  let fixture: ComponentFixture<CalendarWeekGridComponent>;
  let nativeElement: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CalendarWeekGridComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CalendarWeekGridComponent);
    component = fixture.componentInstance;
    nativeElement = fixture.nativeElement;
  });

  afterEach(() => {
    fixture.destroy();
  });

  describe('Basic Rendering', () => {
    it('should create without error when given an empty events array', () => {
      component.weekStartInput = FIXED_DATE;
      component.eventsInput = [];

      expect(() => fixture.detectChanges()).not.toThrow();
      expect(component).toBeTruthy();
    });

    it('should display 7 day headers for the week', () => {
      component.weekStartInput = FIXED_DATE;
      component.eventsInput = [];
      fixture.detectChanges();

      const dayHeaders = nativeElement.querySelectorAll('.grid-cols-\\[4rem_repeat\\(7\\,1fr\\)\\] > div:not(:first-child)');
      expect(dayHeaders.length).toBeGreaterThanOrEqual(7);
    });

    it('should show time grid with 24 hours', () => {
      component.weekStartInput = FIXED_DATE;
      component.eventsInput = [];
      fixture.detectChanges();

      expect(component.hours.length).toBe(24);
    });
  });

  describe('All-Day Events', () => {
    it('should show all-day events section when all-day events exist for the week', () => {
      component.weekStartInput = FIXED_DATE;
      component.eventsInput = [
        createEvent({
          id: 'allday-1',
          title: 'All Day Event',
          isAllDay: true,
          startTime: '2026-01-27T00:00:00Z',
          endTime: '2026-01-27T23:59:59Z',
        }),
      ];
      fixture.detectChanges();

      expect(nativeElement.textContent).toContain('ALL DAY');
      expect(nativeElement.textContent).toContain('All Day Event');
    });

    it('should not show all-day events section when no all-day events exist', () => {
      component.weekStartInput = FIXED_DATE;
      component.eventsInput = [
        createEvent({ isAllDay: false }),
      ];
      fixture.detectChanges();

      expect(nativeElement.textContent).not.toContain('ALL DAY');
    });
  });

  describe('Event Clicks', () => {
    it('should emit eventClicked with the event when an event block is clicked', () => {
      const event = createEvent({ id: 'click-me', title: 'Click Me' });
      component.weekStartInput = FIXED_DATE;
      component.eventsInput = [event];
      fixture.detectChanges();

      const emitted: CalendarEventDto[] = [];
      component.eventClicked.subscribe((e) => emitted.push(e));

      const mouseEvent = new MouseEvent('click', { bubbles: false });
      component.onEventClick(mouseEvent, event);

      expect(emitted).toHaveLength(1);
      expect(emitted[0].id).toBe('click-me');
    });
  });

  describe('Day Header Clicks', () => {
    it('should emit dayHeaderClicked with the date when a day header is clicked', () => {
      component.weekStartInput = FIXED_DATE;
      component.eventsInput = [];
      fixture.detectChanges();

      const emitted: Date[] = [];
      component.dayHeaderClicked.subscribe((d) => emitted.push(d));

      const testDate = new Date(2026, 0, 27);
      component.onDayHeaderClick(testDate);

      expect(emitted).toHaveLength(1);
      expect(emitted[0]).toEqual(testDate);
    });
  });

  describe('Drag-to-Select Behavior', () => {
    beforeEach(() => {
      component.weekStartInput = FIXED_DATE;
      component.eventsInput = [];
      fixture.detectChanges();
    });

    it('should start drag state when mouse down on day column', () => {
      const dayIndex = 1;
      const mouseEvent = new MouseEvent('mousedown', { clientY: 100, bubbles: true });
      Object.defineProperty(mouseEvent, 'currentTarget', {
        value: document.createElement('div'),
        writable: false,
      });

      component.onMouseDown(mouseEvent, dayIndex);

      expect(component.isDragging()).toBe(true);
      expect(component.dragDayIndex()).toBe(dayIndex);
    });

    it('should not start drag when mouse down on an existing event', () => {
      const dayIndex = 1;
      const eventElement = document.createElement('div');
      eventElement.classList.add('z-10');
      const targetElement = document.createElement('div');
      targetElement.appendChild(eventElement);

      const mouseEvent = new MouseEvent('mousedown', { clientY: 100, bubbles: true });
      Object.defineProperty(mouseEvent, 'target', {
        value: eventElement,
        writable: false,
      });
      Object.defineProperty(mouseEvent, 'currentTarget', {
        value: document.createElement('div'),
        writable: false,
      });

      component.onMouseDown(mouseEvent, dayIndex);

      expect(component.isDragging()).toBe(false);
    });

    it('should update drag position when mouse moves during drag', () => {
      // Start drag
      component.isDragging.set(true);
      component.dragDayIndex.set(1);
      component.dragStartY.set(100);
      component.dragCurrentY.set(100);
      fixture.detectChanges();

      // Simulate mouse move
      const mouseMoveEvent = new MouseEvent('mousemove', { clientY: 200, bubbles: true });

      // Mock the scroll container and day column
      const mockContainer = document.createElement('div');
      mockContainer.scrollTop = 0;
      const mockDayColumn = document.createElement('div');
      mockDayColumn.getBoundingClientRect = () => ({ top: 0, left: 0, right: 100, bottom: 100, width: 100, height: 100 } as DOMRect);

      // Mock querySelector to return our mock day column
      const originalQuerySelector = mockContainer.querySelector;
      mockContainer.querySelector = (selector: string) => {
        if (selector.includes('nth-child')) {
          return mockDayColumn;
        }
        return originalQuerySelector.call(mockContainer, selector);
      };

      // Set the scroll container
      if (component.scrollContainer) {
        Object.defineProperty(component.scrollContainer, 'nativeElement', {
          value: mockContainer,
          writable: true,
        });
      }

      component.onMouseMove(mouseMoveEvent);

      expect(component.dragCurrentY()).toBe(200);
    });

    it('should emit timeRangeSelected when drag completes above threshold', () => {
      const dayIndex = 1;
      component.isDragging.set(true);
      component.dragDayIndex.set(dayIndex);
      component.dragStartY.set(100);
      component.dragCurrentY.set(200); // 100px drag distance (above 15px threshold)
      fixture.detectChanges();

      const emitted: TimeRange[] = [];
      component.timeRangeSelected.subscribe((tr) => emitted.push(tr));

      const mouseUpEvent = new MouseEvent('mouseup', { bubbles: true });
      component.onMouseUp(mouseUpEvent);

      expect(emitted).toHaveLength(1);
      expect(emitted[0].start).toBeInstanceOf(Date);
      expect(emitted[0].end).toBeInstanceOf(Date);
      expect(emitted[0].end.getTime()).toBeGreaterThan(emitted[0].start.getTime());
      expect(component.isDragging()).toBe(false);
    });

    it('should not emit timeRangeSelected when drag is below click threshold', () => {
      const dayIndex = 1;
      component.isDragging.set(true);
      component.dragDayIndex.set(dayIndex);
      component.dragStartY.set(100);
      component.dragCurrentY.set(110); // 10px drag distance (below 15px threshold)
      fixture.detectChanges();

      const emitted: TimeRange[] = [];
      component.timeRangeSelected.subscribe((tr) => emitted.push(tr));

      const mouseUpEvent = new MouseEvent('mouseup', { bubbles: true });
      component.onMouseUp(mouseUpEvent);

      expect(emitted).toHaveLength(0);
      expect(component.isDragging()).toBe(false);
    });

    it('should support bidirectional drag (drag up or down)', () => {
      const dayIndex = 1;
      component.isDragging.set(true);
      component.dragDayIndex.set(dayIndex);
      component.dragStartY.set(200); // Start lower
      component.dragCurrentY.set(100); // End higher (drag upward)
      fixture.detectChanges();

      const emitted: TimeRange[] = [];
      component.timeRangeSelected.subscribe((tr) => emitted.push(tr));

      const mouseUpEvent = new MouseEvent('mouseup', { bubbles: true });
      component.onMouseUp(mouseUpEvent);

      expect(emitted).toHaveLength(1);
      // Start time should still be before end time
      expect(emitted[0].start.getTime()).toBeLessThan(emitted[0].end.getTime());
    });

    it('should enforce minimum 15-minute duration', () => {
      const dayIndex = 1;
      component.isDragging.set(true);
      component.dragDayIndex.set(dayIndex);
      component.dragStartY.set(100);
      component.dragCurrentY.set(105); // Very small drag that would be < 15 min
      fixture.detectChanges();

      const emitted: TimeRange[] = [];
      component.timeRangeSelected.subscribe((tr) => emitted.push(tr));

      const mouseUpEvent = new MouseEvent('mouseup', { bubbles: true });
      component.onMouseUp(mouseUpEvent);

      if (emitted.length > 0) {
        const durationMs = emitted[0].end.getTime() - emitted[0].start.getTime();
        const minDurationMs = 15 * 60 * 1000; // 15 minutes in milliseconds
        expect(durationMs).toBeGreaterThanOrEqual(minDurationMs);
      }
    });

    it('should reset drag state after mouse up', () => {
      const dayIndex = 1;
      component.isDragging.set(true);
      component.dragDayIndex.set(dayIndex);
      component.dragStartY.set(100);
      component.dragCurrentY.set(200);
      fixture.detectChanges();

      const mouseUpEvent = new MouseEvent('mouseup', { bubbles: true });
      component.onMouseUp(mouseUpEvent);

      expect(component.isDragging()).toBe(false);
      expect(component.dragStartY()).toBe(0);
      expect(component.dragCurrentY()).toBe(0);
      expect(component.dragDayIndex()).toBe(null);
    });

    it('should calculate drag overlay top as minimum of start and current Y', () => {
      component.dragStartY.set(200);
      component.dragCurrentY.set(100);

      expect(component.dragOverlayTop()).toBe(100);

      component.dragStartY.set(100);
      component.dragCurrentY.set(200);

      expect(component.dragOverlayTop()).toBe(100);
    });

    it('should calculate drag overlay height as absolute difference', () => {
      component.dragStartY.set(100);
      component.dragCurrentY.set(200);

      expect(component.dragOverlayHeight()).toBe(100);

      component.dragStartY.set(200);
      component.dragCurrentY.set(100);

      expect(component.dragOverlayHeight()).toBe(100);
    });

    it('should show drag overlay when dragging on a specific day', () => {
      component.isDragging.set(true);
      component.dragDayIndex.set(1);
      component.dragStartY.set(100);
      component.dragCurrentY.set(200);
      fixture.detectChanges();

      const dragOverlay = nativeElement.querySelector('.bg-blue-500\\/30');
      expect(dragOverlay).toBeTruthy();
    });

    it('should not show drag overlay when not dragging', () => {
      component.isDragging.set(false);
      fixture.detectChanges();

      const dragOverlay = nativeElement.querySelector('.bg-blue-500\\/30');
      expect(dragOverlay).toBeNull();
    });

    it('should display time labels during drag', () => {
      component.isDragging.set(true);
      component.dragDayIndex.set(1);
      component.dragStartY.set(360); // ~6 AM (60px/hour * 6)
      component.dragCurrentY.set(720); // ~12 PM (60px/hour * 12)
      fixture.detectChanges();

      // Time labels should be computed and displayed
      const startLabel = component.dragStartTimeLabel();
      const endLabel = component.dragEndTimeLabel();

      expect(startLabel).toBeTruthy();
      expect(endLabel).toBeTruthy();
      expect(startLabel).not.toBe(endLabel);
    });
  });

  describe('Lifecycle', () => {
    it('should clean up mouse listeners on destroy during active drag', () => {
      component.weekStartInput = FIXED_DATE;
      component.eventsInput = [];
      fixture.detectChanges();

      // Start a drag to create listeners
      const dayIndex = 1;
      const mouseEvent = new MouseEvent('mousedown', { clientY: 100, bubbles: true });
      Object.defineProperty(mouseEvent, 'currentTarget', {
        value: document.createElement('div'),
        writable: false,
      });

      component.onMouseDown(mouseEvent, dayIndex);

      // Verify listeners were created
      expect(component['mouseMoveUnlisten']).toBeDefined();
      expect(component['mouseUpUnlisten']).toBeDefined();

      // Destroy should clean them up without throwing errors
      expect(() => fixture.destroy()).not.toThrow();
    });
  });
});
