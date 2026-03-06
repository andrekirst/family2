-- Row Level Security policies for all modules

-- Auth RLS
DROP POLICY IF EXISTS user_self_policy ON auth.users;
ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;
CREATE POLICY user_self_policy ON auth.users
    FOR ALL
    USING ("id"::text = current_setting('app.current_user_id', true));

-- Family RLS
DROP POLICY IF EXISTS family_member_policy ON family.families;
ALTER TABLE family.families ENABLE ROW LEVEL SECURITY;
CREATE POLICY family_member_policy ON family.families
    FOR ALL
    USING ("id"::text = current_setting('app.current_family_id', true));

-- Calendar events RLS
DROP POLICY IF EXISTS calendar_event_family_policy ON calendar.calendar_events;
ALTER TABLE calendar.calendar_events ENABLE ROW LEVEL SECURITY;
CREATE POLICY calendar_event_family_policy ON calendar.calendar_events
    FOR ALL
    USING ("family_id"::text = current_setting('app.current_family_id', true));

-- Calendar attendees RLS
DROP POLICY IF EXISTS calendar_attendee_family_policy ON calendar.calendar_event_attendees;
ALTER TABLE calendar.calendar_event_attendees ENABLE ROW LEVEL SECURITY;
CREATE POLICY calendar_attendee_family_policy ON calendar.calendar_event_attendees
    FOR ALL
    USING ("calendar_event_id" IN (
        SELECT "id" FROM calendar.calendar_events
        WHERE "family_id"::text = current_setting('app.current_family_id', true)
    ));

-- EventChain chain_definitions RLS
DROP POLICY IF EXISTS chain_definition_family_policy ON event_chain.chain_definitions;
ALTER TABLE event_chain.chain_definitions ENABLE ROW LEVEL SECURITY;
CREATE POLICY chain_definition_family_policy ON event_chain.chain_definitions
    FOR ALL
    USING ("family_id"::text = current_setting('app.current_family_id', true));

-- EventChain chain_executions RLS
DROP POLICY IF EXISTS chain_execution_family_policy ON event_chain.chain_executions;
ALTER TABLE event_chain.chain_executions ENABLE ROW LEVEL SECURITY;
CREATE POLICY chain_execution_family_policy ON event_chain.chain_executions
    FOR ALL
    USING ("family_id"::text = current_setting('app.current_family_id', true));
