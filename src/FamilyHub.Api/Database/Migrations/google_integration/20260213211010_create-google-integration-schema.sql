-- Google Integration module: schema, google_account_links, oauth_states tables
CREATE SCHEMA IF NOT EXISTS google_integration;

CREATE TABLE IF NOT EXISTS google_integration.google_account_links (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    google_account_id character varying(255) NOT NULL,
    google_email character varying(320) NOT NULL,
    encrypted_access_token text NOT NULL,
    encrypted_refresh_token text NOT NULL,
    access_token_expires_at timestamp with time zone NOT NULL,
    granted_scopes text NOT NULL,
    status character varying(50) NOT NULL DEFAULT 'Active',
    last_sync_at timestamp with time zone,
    last_error text,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_google_account_links PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_google_account_links_google_account_id ON google_integration.google_account_links (google_account_id);
CREATE INDEX IF NOT EXISTS ix_google_account_links_status ON google_integration.google_account_links (status);
CREATE UNIQUE INDEX IF NOT EXISTS ix_google_account_links_user_id ON google_integration.google_account_links (user_id);

CREATE TABLE IF NOT EXISTS google_integration.oauth_states (
    state character varying(128) NOT NULL,
    user_id uuid NOT NULL,
    code_verifier character varying(128) NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    expires_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_oauth_states PRIMARY KEY (state)
);

CREATE INDEX IF NOT EXISTS ix_oauth_states_expires_at ON google_integration.oauth_states (expires_at);
