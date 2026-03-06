-- Storage module: schema, stored_files table
CREATE SCHEMA IF NOT EXISTS storage;

CREATE TABLE IF NOT EXISTS storage.stored_files (
    storage_key character varying(255) NOT NULL,
    data bytea NOT NULL,
    mime_type character varying(50) NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_stored_files PRIMARY KEY (storage_key)
);
