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
  attendeeIds: string[];
}

export interface UpdateCalendarEventInput {
  title: string;
  description?: string | null;
  location?: string | null;
  startTime: string;
  endTime: string;
  isAllDay: boolean;
  attendeeIds: string[];
}

@Injectable({
  providedIn: 'root',
})
export class CalendarService {
  private apollo = inject(Apollo);

  getCalendarEvents(familyId: string, startDate: string, endDate: string) {
    return this.apollo
      .query<{ family: { calendars: CalendarEventDto[] } }>({
        query: GET_CALENDAR_EVENTS,
        variables: { familyId, startDate, endDate },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.family?.calendars ?? []),
        catchError((error) => {
          console.error('Failed to fetch calendar events:', error);
          return of([]);
        }),
      );
  }

  getCalendarEvent(id: string) {
    return this.apollo
      .query<{ family: { calendar: CalendarEventDto | null } }>({
        query: GET_CALENDAR_EVENT,
        variables: { id },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.family?.calendar ?? null),
        catchError((error) => {
          console.error('Failed to fetch calendar event:', error);
          return of(null);
        }),
      );
  }

  createCalendarEvent(input: CreateCalendarEventInput) {
    return this.apollo
      .mutate<{ family: { calendar: { create: CalendarEventDto } } }>({
        mutation: CREATE_CALENDAR_EVENT,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.family?.calendar?.create ?? null),
        catchError((error) => {
          console.error('Failed to create calendar event:', error);
          return of(null);
        }),
      );
  }

  updateCalendarEvent(id: string, input: UpdateCalendarEventInput) {
    return this.apollo
      .mutate<{ family: { calendar: { update: CalendarEventDto } } }>({
        mutation: UPDATE_CALENDAR_EVENT,
        variables: { id, input },
      })
      .pipe(
        map((result) => result.data?.family?.calendar?.update ?? null),
        catchError((error) => {
          console.error('Failed to update calendar event:', error);
          return of(null);
        }),
      );
  }

  cancelCalendarEvent(id: string) {
    return this.apollo
      .mutate<{ family: { calendar: { cancel: boolean } } }>({
        mutation: CANCEL_CALENDAR_EVENT,
        variables: { id },
      })
      .pipe(
        map((result) => result.data?.family?.calendar?.cancel ?? false),
        catchError((error) => {
          console.error('Failed to cancel calendar event:', error);
          return of(false);
        }),
      );
  }
}
