import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DateTimePickerComponent } from './date-time-picker.component';

describe('DateTimePickerComponent', () => {
  let component: DateTimePickerComponent;
  let fixture: ComponentFixture<DateTimePickerComponent>;
  let nativeElement: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DateTimePickerComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DateTimePickerComponent);
    component = fixture.componentInstance;
    nativeElement = fixture.nativeElement;
  });

  function render(): void {
    fixture.detectChanges();
  }

  function queryByTestId(testId: string): HTMLElement | null {
    return nativeElement.querySelector(`[data-testid="${testId}"]`);
  }

  describe('inline rendering', () => {
    it('should render calendar grid without needing a click', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      const grid = queryByTestId('dt-calendar-grid');
      expect(grid).toBeTruthy();
      const buttons = grid?.querySelectorAll('button');
      expect(buttons?.length).toBe(42);
    });

    it('should not render trigger or popover elements', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      expect(queryByTestId('dt-trigger')).toBeNull();
      expect(queryByTestId('dt-popover')).toBeNull();
      expect(queryByTestId('dt-done')).toBeNull();
    });

    it('should display month label', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      expect(queryByTestId('dt-month-label')?.textContent).toContain('March');
    });

    it('should have correct data-testid attributes', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'my-dt';
      render();

      expect(queryByTestId('my-dt')).toBeTruthy();
      expect(queryByTestId('my-dt-calendar-grid')).toBeTruthy();
      expect(queryByTestId('my-dt-month-label')).toBeTruthy();
    });
  });

  describe('initialization from inputs', () => {
    it('should initialize edit signals from input values', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.isAllDay = false;
      render();

      expect(component.editStartTime()).toBe('2026-03-15T09:00:00.000Z');
      expect(component.editEndTime()).toBe('2026-03-15T10:00:00.000Z');
      expect(component.editAllDay()).toBe(false);
    });

    it('should set viewMonth from startTime', () => {
      component.startTime = '2026-06-20T09:00:00.000Z';
      component.endTime = '2026-06-20T10:00:00.000Z';
      component.testId = 'dt';
      render();

      expect(queryByTestId('dt-month-label')?.textContent).toContain('June');
    });

    it('should update when inputs change', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      expect(queryByTestId('dt-month-label')?.textContent).toContain('March');

      // Simulate parent changing inputs
      component.startTime = '2026-07-10T14:00:00.000Z';
      component.endTime = '2026-07-10T15:00:00.000Z';
      component.ngOnChanges({
        startTime: {
          currentValue: component.startTime,
          previousValue: '2026-03-15T09:00:00.000Z',
          firstChange: false,
          isFirstChange: () => false,
        },
      });
      fixture.detectChanges();

      expect(queryByTestId('dt-month-label')?.textContent).toContain('July');
      expect(component.editStartTime()).toBe('2026-07-10T14:00:00.000Z');
    });
  });

  describe('month navigation', () => {
    it('should navigate to next month', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      queryByTestId('dt-next-month')?.click();
      fixture.detectChanges();

      expect(queryByTestId('dt-month-label')?.textContent).toContain('April');
    });

    it('should navigate to previous month', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      queryByTestId('dt-prev-month')?.click();
      fixture.detectChanges();

      expect(queryByTestId('dt-month-label')?.textContent).toContain('February');
    });

    it('should have prev and next month buttons', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      expect(queryByTestId('dt-prev-month')).toBeTruthy();
      expect(queryByTestId('dt-next-month')).toBeTruthy();
    });
  });

  describe('emit on interaction', () => {
    it('should emit dateTimeChanged on date selection', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      const spy = vi.fn();
      component.dateTimeChanged.subscribe(spy);

      // Click a calendar day button (day 20)
      const grid = queryByTestId('dt-calendar-grid');
      const buttons = grid?.querySelectorAll('button');
      const day20 = Array.from(buttons ?? []).find((b) => b.textContent?.trim() === '20');
      day20?.click();
      fixture.detectChanges();

      expect(spy).toHaveBeenCalledTimes(1);
      expect(spy).toHaveBeenCalledWith(
        expect.objectContaining({
          startTime: expect.any(String),
          endTime: expect.any(String),
          isAllDay: false,
        }),
      );
    });

    it('should emit dateTimeChanged on time adjustment', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      const spy = vi.fn();
      component.dateTimeChanged.subscribe(spy);

      queryByTestId('dt-start-plus')?.click();
      fixture.detectChanges();

      expect(spy).toHaveBeenCalledTimes(1);
      expect(spy).toHaveBeenCalledWith(
        expect.objectContaining({
          startTime: expect.any(String),
          endTime: expect.any(String),
          isAllDay: false,
        }),
      );
    });

    it('should emit dateTimeChanged on all-day toggle', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      const spy = vi.fn();
      component.dateTimeChanged.subscribe(spy);

      const checkbox = queryByTestId('dt-allday') as HTMLInputElement;
      checkbox.checked = true;
      checkbox.dispatchEvent(new Event('change'));
      fixture.detectChanges();

      expect(spy).toHaveBeenCalledTimes(1);
      expect(spy).toHaveBeenCalledWith(
        expect.objectContaining({
          isAllDay: true,
        }),
      );
    });
  });

  describe('time controls', () => {
    it('should show time inputs when not all-day', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.isAllDay = false;
      component.testId = 'dt';
      render();

      expect(queryByTestId('dt-time-inputs')).toBeTruthy();
      expect(queryByTestId('dt-start-time')).toBeTruthy();
      expect(queryByTestId('dt-end-time')).toBeTruthy();
    });

    it('should hide time inputs when all-day', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.isAllDay = true;
      component.testId = 'dt';
      render();

      expect(queryByTestId('dt-time-inputs')).toBeNull();
    });

    it('should have increment/decrement buttons for time', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      expect(queryByTestId('dt-start-minus')).toBeTruthy();
      expect(queryByTestId('dt-start-plus')).toBeTruthy();
      expect(queryByTestId('dt-end-minus')).toBeTruthy();
      expect(queryByTestId('dt-end-plus')).toBeTruthy();
    });

    it('should have all-day checkbox', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.testId = 'dt';
      render();

      expect(queryByTestId('dt-allday')).toBeTruthy();
    });
  });

  describe('disabled state', () => {
    it('should disable all controls when disabled', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.disabled = true;
      component.testId = 'dt';
      render();

      const prevBtn = queryByTestId('dt-prev-month') as HTMLButtonElement;
      const nextBtn = queryByTestId('dt-next-month') as HTMLButtonElement;
      const allDayCheckbox = queryByTestId('dt-allday') as HTMLInputElement;

      expect(prevBtn.disabled).toBe(true);
      expect(nextBtn.disabled).toBe(true);
      expect(allDayCheckbox.disabled).toBe(true);
    });

    it('should not emit on date selection when disabled', () => {
      component.startTime = '2026-03-15T09:00:00.000Z';
      component.endTime = '2026-03-15T10:00:00.000Z';
      component.disabled = true;
      component.testId = 'dt';
      render();

      const spy = vi.fn();
      component.dateTimeChanged.subscribe(spy);

      // Try clicking a day
      const grid = queryByTestId('dt-calendar-grid');
      const buttons = grid?.querySelectorAll('button');
      buttons?.[15]?.click();
      fixture.detectChanges();

      expect(spy).not.toHaveBeenCalled();
    });
  });
});
