-- Auth module: add preferred_locale column to users
ALTER TABLE auth.users ADD COLUMN IF NOT EXISTS preferred_locale character varying(10) NOT NULL DEFAULT 'en';
