import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { EventDialogComponent } from './event-dialog.component';
import { CalendarService, CalendarEventDto } from '../../services/calendar.service';
import { UserService } from '../../../../core/user/user.service';
import { InvitationService } from '../../../family/services/invitation.service';

const mockEvent: CalendarEventDto = {
  id: 'event-1',
  familyId: 'family-1',
  createdBy: 'user-1',
  title: 'Doctor Appointment',
  description: 'Annual checkup',
  location: 'Hospital',
  startTime: '2026-03-01T09:00:00Z',
  endTime: '2026-03-01T10:00:00Z',
  isAllDay: false,
  type: 'Medical',
  isCancelled: false,
  createdAt: '2026-02-01T00:00:00Z',
  updatedAt: '2026-02-01T00:00:00Z',
  attendees: [{ userId: 'user-1' }],
};

describe('EventDialogComponent — Cancel Confirmation', () => {
  let component: EventDialogComponent;
  let fixture: ComponentFixture<EventDialogComponent>;
  let nativeElement: HTMLElement;
  let calendarService: { cancelCalendarEvent: ReturnType<typeof vi.fn> };
  let invitationService: { getFamilyMembers: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    calendarService = {
      cancelCalendarEvent: vi.fn(),
    };
    invitationService = {
      getFamilyMembers: vi.fn().mockReturnValue(of([])),
    };

    await TestBed.configureTestingModule({
      imports: [EventDialogComponent],
      providers: [
        { provide: CalendarService, useValue: calendarService },
        { provide: UserService, useValue: { currentUser: () => null } },
        { provide: InvitationService, useValue: invitationService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EventDialogComponent);
    component = fixture.componentInstance;
    component.event = mockEvent;
    fixture.detectChanges();
    nativeElement = fixture.nativeElement;
  });

  it('should not show confirmation dialog initially', () => {
    expect(queryByTestId('confirmation-dialog-overlay')).toBeNull();
  });

  it('should show confirmation dialog when Cancel Event button is clicked', () => {
    queryByTestId('cancel-event-button')?.click();
    fixture.detectChanges();

    expect(queryByTestId('confirmation-dialog-overlay')).toBeTruthy();
  });

  it('should display event title in confirmation message', () => {
    queryByTestId('cancel-event-button')?.click();
    fixture.detectChanges();

    const message = queryByTestId('confirmation-dialog-message')?.textContent?.trim();
    expect(message).toContain('Doctor Appointment');
    expect(message).toContain('cannot be undone');
  });

  it('should call calendarService.cancelCalendarEvent on confirm', () => {
    calendarService.cancelCalendarEvent.mockReturnValue(of(true));

    queryByTestId('cancel-event-button')?.click();
    fixture.detectChanges();

    queryByTestId('confirmation-dialog-confirm')?.click();
    fixture.detectChanges();

    expect(calendarService.cancelCalendarEvent).toHaveBeenCalledWith('event-1');
  });

  it('should emit eventCancelled on successful cancel', () => {
    calendarService.cancelCalendarEvent.mockReturnValue(of(true));
    const spy = vi.fn();
    component.eventCancelled.subscribe(spy);

    queryByTestId('cancel-event-button')?.click();
    fixture.detectChanges();

    queryByTestId('confirmation-dialog-confirm')?.click();
    fixture.detectChanges();

    expect(spy).toHaveBeenCalledTimes(1);
  });

  it('should hide confirmation dialog on dismiss (Go Back)', () => {
    queryByTestId('cancel-event-button')?.click();
    fixture.detectChanges();
    expect(queryByTestId('confirmation-dialog-overlay')).toBeTruthy();

    queryByTestId('confirmation-dialog-cancel')?.click();
    fixture.detectChanges();

    expect(queryByTestId('confirmation-dialog-overlay')).toBeNull();
  });

  it('should set error message on cancel failure', () => {
    calendarService.cancelCalendarEvent.mockReturnValue(of(false));

    queryByTestId('cancel-event-button')?.click();
    fixture.detectChanges();

    queryByTestId('confirmation-dialog-confirm')?.click();
    fixture.detectChanges();

    expect(queryByTestId('event-error')?.textContent?.trim()).toBe('Failed to cancel event');
  });

  it('should set error message on cancel error', () => {
    calendarService.cancelCalendarEvent.mockReturnValue(throwError(() => new Error('Network')));

    queryByTestId('cancel-event-button')?.click();
    fixture.detectChanges();

    queryByTestId('confirmation-dialog-confirm')?.click();
    fixture.detectChanges();

    expect(queryByTestId('event-error')?.textContent?.trim()).toBe('An error occurred');
  });

  it('should hide confirmation dialog after cancel error', () => {
    calendarService.cancelCalendarEvent.mockReturnValue(throwError(() => new Error('Network')));

    queryByTestId('cancel-event-button')?.click();
    fixture.detectChanges();

    queryByTestId('confirmation-dialog-confirm')?.click();
    fixture.detectChanges();

    expect(queryByTestId('confirmation-dialog-overlay')).toBeNull();
  });

  it('should not show Cancel Event button in create mode', async () => {
    const createFixture = TestBed.createComponent(EventDialogComponent);
    createFixture.componentInstance.selectedDate = new Date(2026, 2, 1);
    createFixture.detectChanges();

    expect(
      createFixture.nativeElement.querySelector('[data-testid="cancel-event-button"]'),
    ).toBeNull();
  });

  function queryByTestId(testId: string): HTMLElement | null {
    return nativeElement.querySelector(`[data-testid="${testId}"]`);
  }
});
