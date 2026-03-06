-- Auth module: schema and users table
CREATE SCHEMA IF NOT EXISTS auth;

CREATE TABLE IF NOT EXISTS auth.users (
    id uuid NOT NULL,
    email character varying(320) NOT NULL,
    name character varying(200) NOT NULL,
    username character varying(100),
    external_user_id character varying(255) NOT NULL,
    external_provider character varying(50) NOT NULL DEFAULT 'KEYCLOAK',
    family_id uuid,
    email_verified boolean NOT NULL DEFAULT false,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    last_login_at timestamp with time zone,
    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_users PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_users_email ON auth.users (email);
CREATE UNIQUE INDEX IF NOT EXISTS ix_users_external_user_id ON auth.users (external_user_id);
CREATE INDEX IF NOT EXISTS ix_users_family_id ON auth.users (family_id);
