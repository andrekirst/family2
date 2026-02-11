import { gql } from 'apollo-angular';

export const GET_CALENDAR_EVENTS = gql`
  query GetCalendarEvents($familyId: UUID!, $startDate: DateTime!, $endDate: DateTime!) {
    calendar {
      calendarEvents(familyId: $familyId, startDate: $startDate, endDate: $endDate) {
        id
        familyId
        createdBy
        title
        description
        location
        startTime
        endTime
        isAllDay
        type
        isCancelled
        createdAt
        updatedAt
        attendees {
          userId
        }
      }
    }
  }
`;

export const GET_CALENDAR_EVENT = gql`
  query GetCalendarEvent($id: UUID!) {
    calendar {
      calendarEvent(id: $id) {
        id
        familyId
        createdBy
        title
        description
        location
        startTime
        endTime
        isAllDay
        type
        isCancelled
        createdAt
        updatedAt
        attendees {
          userId
        }
      }
    }
  }
`;

export const CREATE_CALENDAR_EVENT = gql`
  mutation CreateCalendarEvent($input: CreateCalendarEventRequestInput!) {
    calendar {
      createCalendarEvent(input: $input) {
        id
        familyId
        createdBy
        title
        description
        location
        startTime
        endTime
        isAllDay
        type
        isCancelled
        createdAt
        updatedAt
        attendees {
          userId
        }
      }
    }
  }
`;

export const UPDATE_CALENDAR_EVENT = gql`
  mutation UpdateCalendarEvent($id: UUID!, $input: UpdateCalendarEventRequestInput!) {
    calendar {
      updateCalendarEvent(id: $id, input: $input) {
        id
        familyId
        createdBy
        title
        description
        location
        startTime
        endTime
        isAllDay
        type
        isCancelled
        createdAt
        updatedAt
        attendees {
          userId
        }
      }
    }
  }
`;

export const CANCEL_CALENDAR_EVENT = gql`
  mutation CancelCalendarEvent($id: UUID!) {
    calendar {
      cancelCalendarEvent(id: $id)
    }
  }
`;
