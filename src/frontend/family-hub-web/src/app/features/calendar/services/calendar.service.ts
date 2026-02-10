import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import {
  GET_CALENDAR_EVENTS,
  GET_CALENDAR_EVENT,
  CREATE_CALENDAR_EVENT,
  UPDATE_CALENDAR_EVENT,
  CANCEL_CALENDAR_EVENT,
} from '../graphql/calendar.operations';
import { catchError, map, of } from 'rxjs';

export interface CalendarEventAttendeeDto {
  userId: string;
}

export interface CalendarEventDto {
  id: string;
  familyId: string;
  createdBy: string;
  title: string;
  description: string | null;
  location: string | null;
  startTime: string;
  endTime: string;
  isAllDay: boolean;
  type: string;
  isCancelled: boolean;
  createdAt: string;
  updatedAt: string;
  attendees: CalendarEventAttendeeDto[];
}

export interface CreateCalendarEventInput {
  title: string;
  description?: string | null;
  location?: string | null;
  startTime: string;
  endTime: string;
  isAllDay: boolean;
  type: string;
  attendeeIds: string[];
}

export interface UpdateCalendarEventInput {
  title: string;
  description?: string | null;
  location?: string | null;
  startTime: string;
  endTime: string;
  isAllDay: boolean;
  type: string;
  attendeeIds: string[];
}

@Injectable({
  providedIn: 'root',
})
export class CalendarService {
  private apollo = inject(Apollo);

  getCalendarEvents(familyId: string, startDate: string, endDate: string) {
    return this.apollo
      .query<{ calendarEvents: CalendarEventDto[] }>({
        query: GET_CALENDAR_EVENTS,
        variables: { familyId, startDate, endDate },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.calendarEvents ?? []),
        catchError((error) => {
          console.error('Failed to fetch calendar events:', error);
          return of([]);
        }),
      );
  }

  getCalendarEvent(id: string) {
    return this.apollo
      .query<{ calendarEvent: CalendarEventDto | null }>({
        query: GET_CALENDAR_EVENT,
        variables: { id },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.calendarEvent ?? null),
        catchError((error) => {
          console.error('Failed to fetch calendar event:', error);
          return of(null);
        }),
      );
  }

  createCalendarEvent(input: CreateCalendarEventInput) {
    return this.apollo
      .mutate<{ createCalendarEvent: CalendarEventDto }>({
        mutation: CREATE_CALENDAR_EVENT,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.createCalendarEvent ?? null),
        catchError((error) => {
          console.error('Failed to create calendar event:', error);
          return of(null);
        }),
      );
  }

  updateCalendarEvent(id: string, input: UpdateCalendarEventInput) {
    return this.apollo
      .mutate<{ updateCalendarEvent: CalendarEventDto }>({
        mutation: UPDATE_CALENDAR_EVENT,
        variables: { id, input },
      })
      .pipe(
        map((result) => result.data?.updateCalendarEvent ?? null),
        catchError((error) => {
          console.error('Failed to update calendar event:', error);
          return of(null);
        }),
      );
  }

  cancelCalendarEvent(id: string) {
    return this.apollo
      .mutate<{ cancelCalendarEvent: boolean }>({
        mutation: CANCEL_CALENDAR_EVENT,
        variables: { id },
      })
      .pipe(
        map((result) => result.data?.cancelCalendarEvent ?? false),
        catchError((error) => {
          console.error('Failed to cancel calendar event:', error);
          return of(false);
        }),
      );
  }
}
