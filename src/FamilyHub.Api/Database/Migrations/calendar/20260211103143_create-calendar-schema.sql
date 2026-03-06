-- Calendar module: schema, calendar_events, calendar_event_attendees tables
-- Note: the 'type' column was removed in a later migration (RemoveCalendarEventType),
-- so the consolidated schema does not include it.
CREATE SCHEMA IF NOT EXISTS calendar;

CREATE TABLE IF NOT EXISTS calendar.calendar_events (
    id uuid NOT NULL,
    family_id uuid NOT NULL,
    created_by uuid NOT NULL,
    title character varying(200) NOT NULL,
    description character varying(2000),
    location character varying(500),
    start_time timestamp with time zone NOT NULL,
    end_time timestamp with time zone NOT NULL,
    is_all_day boolean NOT NULL,
    is_cancelled boolean NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_calendar_events PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_calendar_events_created_by_start_time ON calendar.calendar_events (created_by, start_time);
CREATE INDEX IF NOT EXISTS ix_calendar_events_family_id_start_time ON calendar.calendar_events (family_id, start_time);

CREATE TABLE IF NOT EXISTS calendar.calendar_event_attendees (
    calendar_event_id uuid NOT NULL,
    user_id uuid NOT NULL,
    CONSTRAINT pk_calendar_event_attendees PRIMARY KEY (calendar_event_id, user_id),
    CONSTRAINT fk_calendar_event_attendees_calendar_events_calendar_event_id FOREIGN KEY (calendar_event_id)
        REFERENCES calendar.calendar_events (id) ON DELETE CASCADE
);
