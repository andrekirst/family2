import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, Subject } from 'rxjs';
import { EventContextComponent } from './event-context.component';
import { CalendarService, CalendarEventDto } from '../../services/calendar.service';
import { UserService } from '../../../../core/user/user.service';
import { InvitationService } from '../../../family/services/invitation.service';
import { ContextPanelService } from '../../../../shared/services/context-panel.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { FamilyMemberDto } from '../../../family/models/invitation.models';

const mockMembers: FamilyMemberDto[] = [
  {
    id: 'member-1',
    userId: 'user-1',
    userName: 'Alice Smith',
    userEmail: 'alice@test.com',
    role: 'Owner',
    joinedAt: '2026-01-01T00:00:00Z',
    isActive: true,
    avatarId: null,
  },
  {
    id: 'member-2',
    userId: 'user-2',
    userName: 'Bob Jones',
    userEmail: 'bob@test.com',
    role: 'Member',
    joinedAt: '2026-01-01T00:00:00Z',
    isActive: true,
    avatarId: null,
  },
];

function createEvent(overrides: Partial<CalendarEventDto> = {}): CalendarEventDto {
  return {
    id: 'evt-1',
    familyId: 'fam-1',
    createdBy: 'user-1',
    title: 'Team Meeting',
    description: 'Weekly standup',
    location: 'Room 42',
    startTime: '2026-03-01T09:00:00Z',
    endTime: '2026-03-01T10:00:00Z',
    isAllDay: false,
    isCancelled: false,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
    attendees: [{ userId: 'user-1' }, { userId: 'user-2' }],
    ...overrides,
  };
}

describe('EventContextComponent', () => {
  let calendarService: {
    updateCalendarEvent: ReturnType<typeof vi.fn>;
    createCalendarEvent: ReturnType<typeof vi.fn>;
    cancelCalendarEvent: ReturnType<typeof vi.fn>;
  };
  let invitationService: { getFamilyMembers: ReturnType<typeof vi.fn> };
  let contextPanelService: { setItemId: ReturnType<typeof vi.fn>; close: ReturnType<typeof vi.fn> };
  let toastService: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    calendarService = {
      updateCalendarEvent: vi.fn().mockReturnValue(of(createEvent())),
      createCalendarEvent: vi.fn().mockReturnValue(of(createEvent({ id: 'new-evt-1' }))),
      cancelCalendarEvent: vi.fn().mockReturnValue(of(true)),
    };
    invitationService = {
      getFamilyMembers: vi.fn().mockReturnValue(of(mockMembers)),
    };
    contextPanelService = {
      setItemId: vi.fn(),
      close: vi.fn(),
    };
    toastService = {
      success: vi.fn(),
      error: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [EventContextComponent],
      providers: [
        { provide: CalendarService, useValue: calendarService },
        {
          provide: UserService,
          useValue: { currentUser: () => ({ id: 'user-1', name: 'Alice' }) },
        },
        { provide: InvitationService, useValue: invitationService },
        { provide: ContextPanelService, useValue: contextPanelService },
        { provide: ToastService, useValue: toastService },
        { provide: Router, useValue: { events: new Subject().asObservable() } },
      ],
    }).compileComponents();
  });

  function createComponent(
    inputs: { event?: CalendarEventDto | null; selectedDate?: Date | null } = {},
  ) {
    const fixture = TestBed.createComponent(EventContextComponent);
    const component = fixture.componentInstance;
    if (inputs.event !== undefined) component.event = inputs.event;
    if (inputs.selectedDate !== undefined) component.selectedDate = inputs.selectedDate;
    fixture.detectChanges();
    return { fixture, component, nativeElement: fixture.nativeElement as HTMLElement };
  }

  function queryByTestId(nativeElement: HTMLElement, testId: string): HTMLElement | null {
    return nativeElement.querySelector(`[data-testid="${testId}"]`);
  }

  describe('Edit mode (event provided)', () => {
    it('should render event context container', () => {
      const { nativeElement } = createComponent({ event: createEvent() });
      expect(queryByTestId(nativeElement, 'event-context')).toBeTruthy();
    });

    it('should display event title', () => {
      const { nativeElement } = createComponent({ event: createEvent() });
      expect(queryByTestId(nativeElement, 'event-context-title')?.textContent?.trim()).toContain(
        'Team Meeting',
      );
    });

    it('should display date and time', () => {
      const { nativeElement } = createComponent({ event: createEvent() });
      expect(queryByTestId(nativeElement, 'event-context-datetime')).toBeTruthy();
    });

    it('should display location', () => {
      const { nativeElement } = createComponent({ event: createEvent() });
      expect(queryByTestId(nativeElement, 'event-context-location')?.textContent).toContain(
        'Room 42',
      );
    });

    it('should not display location when null', () => {
      const { nativeElement } = createComponent({ event: createEvent({ location: null }) });
      expect(queryByTestId(nativeElement, 'event-context-location')).toBeNull();
    });

    it('should display description', () => {
      const { nativeElement } = createComponent({ event: createEvent() });
      expect(queryByTestId(nativeElement, 'event-context-description')?.textContent).toContain(
        'Weekly standup',
      );
    });

    it('should resolve attendee names', () => {
      const { nativeElement } = createComponent({ event: createEvent() });
      const attendees = queryByTestId(nativeElement, 'event-context-attendees');
      expect(attendees?.textContent).toContain('Alice Smith');
      expect(attendees?.textContent).toContain('Bob Jones');
    });

    it('should not show attendee count in header', () => {
      const { nativeElement } = createComponent({ event: createEvent() });
      const attendees = queryByTestId(nativeElement, 'event-context-attendees');
      expect(attendees?.textContent).not.toContain('(2)');
    });

    it('should not have edit button', () => {
      const { nativeElement } = createComponent({ event: createEvent() });
      expect(queryByTestId(nativeElement, 'event-context-edit')).toBeNull();
    });

    it('should show cancel event button for active events', () => {
      const { nativeElement } = createComponent({ event: createEvent() });
      expect(queryByTestId(nativeElement, 'event-context-cancel')).toBeTruthy();
    });

    it('should not show cancel event button for cancelled events', () => {
      const { nativeElement } = createComponent({ event: createEvent({ isCancelled: true }) });
      expect(queryByTestId(nativeElement, 'event-context-cancel')).toBeNull();
    });

    it('should show cancelled badge when event is cancelled', () => {
      const { nativeElement } = createComponent({ event: createEvent({ isCancelled: true }) });
      expect(queryByTestId(nativeElement, 'event-context-cancelled')?.textContent?.trim()).toBe(
        'Cancelled',
      );
    });

    it('should not show cancelled badge when event is active', () => {
      const { nativeElement } = createComponent({
        event: createEvent({ isCancelled: false }),
      });
      expect(queryByTestId(nativeElement, 'event-context-cancelled')).toBeNull();
    });

    it('should not call service on title save alone', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onTitleSaved('Updated Title');
      expect(calendarService.updateCalendarEvent).not.toHaveBeenCalled();
    });

    it('should call updateCalendarEvent on title save then save()', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onTitleSaved('Updated Title');
      component.save();
      expect(calendarService.updateCalendarEvent).toHaveBeenCalledWith(
        'evt-1',
        expect.objectContaining({ title: 'Updated Title' }),
      );
    });

    it('should show title error when saving empty title', () => {
      const { component, fixture, nativeElement } = createComponent({ event: createEvent() });
      component.onTitleSaved('');
      fixture.detectChanges();
      expect(queryByTestId(nativeElement, 'event-context-title-error')?.textContent?.trim()).toBe(
        'Title is required',
      );
    });

    it('should not call service on datetime change alone', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onDateTimeChanged({
        startTime: '2026-03-02T09:00:00Z',
        endTime: '2026-03-02T10:00:00Z',
        isAllDay: false,
      });
      expect(calendarService.updateCalendarEvent).not.toHaveBeenCalled();
    });

    it('should call updateCalendarEvent on datetime change then save()', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onDateTimeChanged({
        startTime: '2026-03-02T09:00:00Z',
        endTime: '2026-03-02T10:00:00Z',
        isAllDay: false,
      });
      component.save();
      expect(calendarService.updateCalendarEvent).toHaveBeenCalled();
    });

    it('should show saving indicator during save', () => {
      const { component, fixture, nativeElement } = createComponent({ event: createEvent() });
      calendarService.updateCalendarEvent.mockReturnValue(new Subject());
      component.onTitleSaved('New Title');
      component.save();
      fixture.detectChanges();
      const saveBtn = queryByTestId(nativeElement, 'event-context-save');
      expect(saveBtn?.textContent).toContain('Saving...');
    });

    it('should show error message on save failure', () => {
      const { component, fixture, nativeElement } = createComponent({ event: createEvent() });
      calendarService.updateCalendarEvent.mockReturnValue(of(null));
      component.onTitleSaved('New Title');
      component.save();
      fixture.detectChanges();
      expect(queryByTestId(nativeElement, 'event-context-error')?.textContent).toContain(
        'Failed to save',
      );
    });

    it('should clear error message on field interaction', () => {
      const { component } = createComponent({ event: createEvent() });
      calendarService.updateCalendarEvent.mockReturnValue(of(null));
      component.onTitleSaved('New Title');
      component.save();
      expect(component.errorMessage()).toContain('Failed');
      component.onDescriptionSaved('new desc');
      expect(component.errorMessage()).toBeNull();
    });
  });

  describe('Create mode (selectedDate with time)', () => {
    it('should initialize with empty title', () => {
      const { component } = createComponent({ selectedDate: new Date(2026, 2, 15, 14, 30) });
      expect(component.title()).toBe('');
    });

    it('should pre-select current user as attendee', () => {
      const { component } = createComponent({ selectedDate: new Date(2026, 2, 15, 14, 30) });
      expect(component.selectedAttendees()).toContain('user-1');
    });

    it('should set start time from selected date', () => {
      const { component } = createComponent({ selectedDate: new Date(2026, 2, 15, 14, 30) });
      const start = new Date(component.startTime());
      expect(start.getHours()).toBe(14);
      expect(start.getMinutes()).toBe(30);
    });

    it('should set end time +1 hour from start', () => {
      const { component } = createComponent({ selectedDate: new Date(2026, 2, 15, 14, 30) });
      const start = new Date(component.startTime());
      const end = new Date(component.endTime());
      expect(end.getTime() - start.getTime()).toBe(3600000);
    });

    it('should not call service on title save alone in create mode', () => {
      const { component } = createComponent({ selectedDate: new Date(2026, 2, 15, 14, 30) });
      component.onTitleSaved('New Event');
      expect(calendarService.createCalendarEvent).not.toHaveBeenCalled();
    });

    it('should call createCalendarEvent on title save then save()', () => {
      const { component } = createComponent({ selectedDate: new Date(2026, 2, 15, 14, 30) });
      component.onTitleSaved('New Event');
      component.save();
      expect(calendarService.createCalendarEvent).toHaveBeenCalledWith(
        expect.objectContaining({ title: 'New Event' }),
      );
    });

    it('should transition to edit mode after create', () => {
      const { component } = createComponent({ selectedDate: new Date(2026, 2, 15, 14, 30) });
      component.onTitleSaved('New Event');
      component.save();
      expect(contextPanelService.setItemId).toHaveBeenCalledWith('new-evt-1');
      expect(component.eventId()).toBe('new-evt-1');
    });

    it('should emit eventCreated after create', () => {
      const { component } = createComponent({ selectedDate: new Date(2026, 2, 15, 14, 30) });
      const spy = vi.fn();
      component.eventCreated.subscribe(spy);
      component.onTitleSaved('New Event');
      component.save();
      expect(spy).toHaveBeenCalledWith(expect.objectContaining({ id: 'new-evt-1' }));
    });

    it('should not show cancel button for new events', () => {
      const { nativeElement } = createComponent({
        selectedDate: new Date(2026, 2, 15, 14, 30),
      });
      expect(queryByTestId(nativeElement, 'event-context-cancel')).toBeNull();
    });
  });

  describe('Create mode with month-view date (no time)', () => {
    it('should default to 9-10 AM', () => {
      const { component } = createComponent({
        selectedDate: new Date(2026, 2, 15, 0, 0, 0, 0),
      });
      const start = new Date(component.startTime());
      const end = new Date(component.endTime());
      expect(start.getHours()).toBe(9);
      expect(end.getHours()).toBe(10);
    });
  });

  describe('Dirty tracking', () => {
    it('should not be dirty initially in edit mode', () => {
      const { component } = createComponent({ event: createEvent() });
      expect(component.isDirty()).toBe(false);
    });

    it('should be dirty after title change', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onTitleSaved('Changed Title');
      expect(component.isDirty()).toBe(true);
    });

    it('should be dirty after description change', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onDescriptionSaved('New description');
      expect(component.isDirty()).toBe(true);
    });

    it('should be dirty after datetime change', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onDateTimeChanged({
        startTime: '2026-04-01T09:00:00Z',
        endTime: '2026-04-01T10:00:00Z',
        isAllDay: false,
      });
      expect(component.isDirty()).toBe(true);
    });

    it('should not be dirty after successful save', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onTitleSaved('Changed Title');
      expect(component.isDirty()).toBe(true);
      component.save();
      expect(component.isDirty()).toBe(false);
    });

    it('should not be dirty after successful create', () => {
      const { component } = createComponent({ selectedDate: new Date(2026, 2, 15, 14, 30) });
      component.onTitleSaved('New Event');
      expect(component.isDirty()).toBe(true);
      component.save();
      expect(component.isDirty()).toBe(false);
    });

    it('should be dirty in create mode when title has content', () => {
      const { component } = createComponent({ selectedDate: new Date(2026, 2, 15, 14, 30) });
      expect(component.isDirty()).toBe(false); // empty title
      component.onTitleSaved('Some title');
      expect(component.isDirty()).toBe(true);
    });
  });

  describe('Save button', () => {
    it('should render save button', () => {
      const { nativeElement } = createComponent({ event: createEvent() });
      expect(queryByTestId(nativeElement, 'event-context-save')).toBeTruthy();
    });

    it('should disable save button when no changes', () => {
      const { nativeElement } = createComponent({ event: createEvent() });
      const btn = queryByTestId(nativeElement, 'event-context-save') as HTMLButtonElement;
      expect(btn.disabled).toBe(true);
    });

    it('should enable save button when dirty', () => {
      const { component, fixture, nativeElement } = createComponent({ event: createEvent() });
      component.onTitleSaved('Changed Title');
      fixture.detectChanges();
      const btn = queryByTestId(nativeElement, 'event-context-save') as HTMLButtonElement;
      expect(btn.disabled).toBe(false);
    });

    it('should hide save button when cancelled', () => {
      const { nativeElement } = createComponent({ event: createEvent({ isCancelled: true }) });
      expect(queryByTestId(nativeElement, 'event-context-save')).toBeNull();
    });

    it('should show "Create Event" in create mode', () => {
      const { component, fixture, nativeElement } = createComponent({
        selectedDate: new Date(2026, 2, 15, 14, 30),
      });
      component.onTitleSaved('New Event');
      fixture.detectChanges();
      const btn = queryByTestId(nativeElement, 'event-context-save');
      expect(btn?.textContent?.trim()).toBe('Create Event');
    });

    it('should show "Save Changes" in edit mode when dirty', () => {
      const { component, fixture, nativeElement } = createComponent({ event: createEvent() });
      component.onTitleSaved('Changed Title');
      fixture.detectChanges();
      const btn = queryByTestId(nativeElement, 'event-context-save');
      expect(btn?.textContent?.trim()).toBe('Save Changes');
    });
  });

  describe('Save success feedback', () => {
    beforeEach(() => {
      vi.useFakeTimers();
    });

    afterEach(() => {
      vi.useRealTimers();
    });

    it('should show success state on button after save', () => {
      const { component, fixture, nativeElement } = createComponent({ event: createEvent() });
      component.onTitleSaved('Updated Title');
      component.save();
      fixture.detectChanges();
      const btn = queryByTestId(nativeElement, 'event-context-save');
      expect(btn?.textContent).toContain('Saved!');
      expect(component.saveSuccess()).toBe(true);
    });

    it('should clear success state after timeout', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onTitleSaved('Updated Title');
      component.save();
      expect(component.saveSuccess()).toBe(true);
      vi.advanceTimersByTime(2000);
      expect(component.saveSuccess()).toBe(false);
    });

    it('should call toastService.success on successful update', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onTitleSaved('Updated Title');
      component.save();
      expect(toastService.success).toHaveBeenCalledWith('Changes saved');
    });

    it('should call toastService.success on successful create', () => {
      const { component } = createComponent({ selectedDate: new Date(2026, 2, 15, 14, 30) });
      component.onTitleSaved('New Event');
      component.save();
      expect(toastService.success).toHaveBeenCalledWith('Event created');
    });

    it('should not call toastService on save failure', () => {
      const { component } = createComponent({ event: createEvent() });
      calendarService.updateCalendarEvent.mockReturnValue(of(null));
      component.onTitleSaved('New Title');
      component.save();
      expect(toastService.success).not.toHaveBeenCalled();
    });

    it('should apply green background when saveSuccess is true', () => {
      const { component, fixture, nativeElement } = createComponent({ event: createEvent() });
      component.onTitleSaved('Updated Title');
      component.save();
      fixture.detectChanges();
      const btn = queryByTestId(nativeElement, 'event-context-save');
      expect(btn?.className).toContain('bg-green-600');
    });
  });

  describe('Cancel event flow', () => {
    it('should show confirmation dialog on cancel click', () => {
      const { nativeElement, fixture } = createComponent({ event: createEvent() });
      queryByTestId(nativeElement, 'event-context-cancel')?.click();
      fixture.detectChanges();
      expect(queryByTestId(nativeElement, 'confirmation-dialog-overlay')).toBeTruthy();
    });

    it('should call cancelCalendarEvent on confirm', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onCancelEvent();
      component.onCancelConfirmed();
      expect(calendarService.cancelCalendarEvent).toHaveBeenCalledWith('evt-1');
    });

    it('should emit eventCancelled on successful cancel', () => {
      const { component } = createComponent({ event: createEvent() });
      const spy = vi.fn();
      component.eventCancelled.subscribe(spy);
      component.onCancelEvent();
      component.onCancelConfirmed();
      expect(spy).toHaveBeenCalled();
    });

    it('should close context panel on successful cancel', () => {
      const { component } = createComponent({ event: createEvent() });
      component.onCancelEvent();
      component.onCancelConfirmed();
      expect(contextPanelService.close).toHaveBeenCalled();
    });

    it('should dismiss confirmation dialog', () => {
      const { component, fixture } = createComponent({ event: createEvent() });
      component.onCancelEvent();
      fixture.detectChanges();
      component.onCancelDismissed();
      fixture.detectChanges();
      expect(component.showCancelConfirmation()).toBe(false);
    });
  });
});
