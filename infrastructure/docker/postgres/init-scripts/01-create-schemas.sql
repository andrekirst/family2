-- Family Hub - PostgreSQL Schema Initialization
-- Creates separate schemas for each DDD module

-- Create schemas for all 8 modules
CREATE SCHEMA IF NOT EXISTS auth;
CREATE SCHEMA IF NOT EXISTS calendar;
CREATE SCHEMA IF NOT EXISTS tasks;
CREATE SCHEMA IF NOT EXISTS shopping;
CREATE SCHEMA IF NOT EXISTS health;
CREATE SCHEMA IF NOT EXISTS meal_planning;
CREATE SCHEMA IF NOT EXISTS finance;
CREATE SCHEMA IF NOT EXISTS communication;

-- Create schema for Zitadel (separate database will be created by Zitadel itself)
-- This is just for reference

-- Enable UUID extension for all schemas
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create RLS helper function for family group isolation
CREATE OR REPLACE FUNCTION current_user_id()
RETURNS UUID AS $$
BEGIN
    -- Extract user ID from JWT claims (set by application context)
    -- This will be set by the application using SET LOCAL
    RETURN NULLIF(CURRENT_SETTING('app.current_user_id', TRUE), '')::UUID;
EXCEPTION
    WHEN OTHERS THEN
        RETURN NULL;
END;
$$ LANGUAGE plpgsql STABLE;

-- Grant usage on schemas to familyhub user
GRANT USAGE ON SCHEMA auth TO familyhub;
GRANT USAGE ON SCHEMA calendar TO familyhub;
GRANT USAGE ON SCHEMA tasks TO familyhub;
GRANT USAGE ON SCHEMA shopping TO familyhub;
GRANT USAGE ON SCHEMA health TO familyhub;
GRANT USAGE ON SCHEMA meal_planning TO familyhub;
GRANT USAGE ON SCHEMA finance TO familyhub;
GRANT USAGE ON SCHEMA communication TO familyhub;

-- Grant all privileges on all tables in schemas (for migrations)
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA auth TO familyhub;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA calendar TO familyhub;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA tasks TO familyhub;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA shopping TO familyhub;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA health TO familyhub;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA meal_planning TO familyhub;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA finance TO familyhub;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA communication TO familyhub;

-- Grant all privileges on all sequences in schemas
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA auth TO familyhub;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA calendar TO familyhub;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA tasks TO familyhub;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA shopping TO familyhub;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA health TO familyhub;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA meal_planning TO familyhub;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA finance TO familyhub;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA communication TO familyhub;

-- Set default privileges for future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA auth GRANT ALL ON TABLES TO familyhub;
ALTER DEFAULT PRIVILEGES IN SCHEMA calendar GRANT ALL ON TABLES TO familyhub;
ALTER DEFAULT PRIVILEGES IN SCHEMA tasks GRANT ALL ON TABLES TO familyhub;
ALTER DEFAULT PRIVILEGES IN SCHEMA shopping GRANT ALL ON TABLES TO familyhub;
ALTER DEFAULT PRIVILEGES IN SCHEMA health GRANT ALL ON TABLES TO familyhub;
ALTER DEFAULT PRIVILEGES IN SCHEMA meal_planning GRANT ALL ON TABLES TO familyhub;
ALTER DEFAULT PRIVILEGES IN SCHEMA finance GRANT ALL ON TABLES TO familyhub;
ALTER DEFAULT PRIVILEGES IN SCHEMA communication GRANT ALL ON TABLES TO familyhub;

-- Log completion
DO $$
BEGIN
    RAISE NOTICE 'Family Hub database schemas created successfully';
    RAISE NOTICE 'Schemas: auth, calendar, tasks, shopping, health, meal_planning, finance, communication';
END $$;
